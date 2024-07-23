using System.ComponentModel;
using Avalonia.Media.Imaging;
using System.Threading.Tasks;
using System.Diagnostics;
using MuPDFCore;
using System.Threading;
using System.IO;
using Avalonia.Layout;
using Avalonia.Platform;
using Avalonia;
using Avalon.Model;
using Avalonia.Controls.Shapes;

namespace Avalon.ViewModels
{
    public class PreviewViewModel : ViewModelBase, INotifyPropertyChanged
    {
        public PreviewViewModel() { }

        public MuPDFDocument previewFile = null;
        public MuPDFDocument PreviewFile
        {
            get { return previewFile; }
            set { previewFile = value; OnPropertyChanged("PreviewFile"); }
        }


        public bool twopageMode = true;
        public bool TwopageMode
        {
            get { return twopageMode; }
            set { twopageMode = value; OnPropertyChanged("TwopageMode"); }
        }

        public bool linkedPageMode = true;
        public bool LinkedPageMode
        {
            get { return linkedPageMode; }
            set { linkedPageMode = value; OnPropertyChanged("LinkedPageMode"); }
        }

        public FileData currentFile = null;
        public FileData CurrentFile
        {
            get { return currentFile; }
            set { currentFile = value; OnPropertyChanged("CurrentFile"); }
        }

        public FileData requestFile = null;
        public FileData RequestFile
        {
            get { return requestFile; }
            set { requestFile = value; SetFile(); OnPropertyChanged("RequestFile"); }
        }

        public int requestPage1 = 0;
        public int RequestPage1
        {
            get { return requestPage1; }
            set
            {
                if (PageInRange(value))
                {
                    requestPage1 = value;
                    OnPropertyChanged("RequestPage1");
                }
            }
        }

        public int requestPage2 = 0;
        public int RequestPage2
        {
            get 
            { 
                if (TwopageMode)
                {
                    return RequestPage1 + 1;
                }
                else
                {
                    return requestPage2;
                }
                
            }
            set
            {
                if (PageInRange(value))
                {
                    requestPage2 = value;
                    OnPropertyChanged("RequestPage2");
                }
            }
        }

        public int currentPage1 = 0;
        public int CurrentPage1
        {
            get { return currentPage1; }
            set { currentPage1 = value; OnPropertyChanged("CurrentPage1"); }
        }

        public int currentPage2 = 0;
        public int CurrentPage2
        {
            get { return currentPage2; }
            set { currentPage2 = value; OnPropertyChanged("CurrentPage2"); }
        }


        public int pagecount = 0;
        public int Pagecount
        {
            get { return pagecount; }
            set { pagecount = value; OnPropertyChanged("Pagecount"); }
        }

        private bool dimmedBackground = false;
        public bool DimmedBackground
        {
            get { return dimmedBackground; }
            set { dimmedBackground = value; OnPropertyChanged("DimmedBackground"); }
        }

        private bool FileWorkerBusy = false;


        private void SetFile()
        {
            if (!FileWorkerBusy)
            {
                string path = RequestFile.Sökväg;
                SetFileTask(path);
            }
        }

        private async void SetFileTask(string path)
        {
            FileWorkerBusy = true;
            SafeDispose();
            if (false)
            {
                //byte[] bytes = await File.ReadAllBytesAsync(path);
                //PreviewFile = new MuPDFDocument(new MuPDFContext(), bytes, InputFileTypes.PDF);
            }
            else
            {
                PreviewFile = new MuPDFDocument(new MuPDFContext(), path);
            }
            
            if (RequestFile.Sökväg == path)
            {
                Pagecount = PreviewFile.Pages.Count;
                CurrentFile = RequestFile;
                requestPage1 = 0;
                requestPage2 = 1;
                FileWorkerBusy = false;
            }
            else
            {
                SetFileTask(path);
            }

        }

        private async Task SetFileTask_OLD(string path)
        {
            FileWorkerBusy = true;
            SafeDispose();
            //byte[] bytes = await File.ReadAllBytesAsync(path);
            //PreviewFile = new MuPDFDocument(new MuPDFContext(), bytes, InputFileTypes.PDF);
            PreviewFile = new MuPDFDocument(new MuPDFContext(), path);

            if (RequestFile.Sökväg == path)
            {
                Pagecount = PreviewFile.Pages.Count;
                RequestPage1 = 0;
                RequestPage2 = 1;
                CurrentFile = RequestFile;
                FileWorkerBusy = false;
            }
            else
            {
                SetFileTask(path);
            }
        }

        private void SafeDispose()
        {
            if (PreviewFile != null)
            {
                PreviewFile.Dispose();
            }
        }

        public void NextPage(bool SecondPage = false)
        {
            Debug.WriteLine("NEXT PAGE");
            if (!TwopageMode)
            {
                RequestPage1 = RequestPage1 + 1;
            }


            if (TwopageMode)
            {
                if (LinkedPageMode)
                {
                    RequestPage1 = RequestPage1 + 2;
                    RequestPage2 = RequestPage1 + 1;
                }
                else
                {
                    if (!SecondPage)
                    {
                        RequestPage1 = RequestPage1 + 1;
                    }
                    else
                    {
                        RequestPage2 = RequestPage2 + 1;
                    }
                }
            }


        }

        public void PrevPage(bool SecondPage = false)
        {
            Debug.WriteLine("PREV PAGE");
            if (!TwopageMode)
            {
                RequestPage1 = RequestPage1 - 1;
            }

            if (TwopageMode)
            {
                if (LinkedPageMode)
                {
                    RequestPage1 = RequestPage1 - 2;
                    RequestPage2 = RequestPage1 + 1;
                }
                else
                {
                    if (!SecondPage)
                    {
                        RequestPage1 = RequestPage1 - 1;
                    }
                    else
                    {
                        RequestPage2 = RequestPage2 - 1;
                    }
                }
            }
        }

        public bool PageInRange(int pagenr)
        {
            if (pagenr >= 0 && pagenr < Pagecount)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void toggle_pw_mode()
        {
            TwopageMode = !TwopageMode;

            if (TwopageMode)
            {
                if (CurrentPage1 %2 == 0)
                {
                    RequestPage1 = CurrentPage1;
                }
                else
                {
                    RequestPage1 = CurrentPage1 - 1;
                }
                Debug.WriteLine(RequestPage1);
            }

        }

        public void ToggleLinkedMode()
        {
            if (LinkedPageMode)
            {
                if (CurrentPage1 % 2 == 0)
                {
                    RequestPage1 = CurrentPage1;
                }
                else
                {
                    RequestPage1 = CurrentPage1 - 1;
                }
            }
            else
            {
                RequestPage2 = RequestPage1 + 1;
            }
        }

    }
}