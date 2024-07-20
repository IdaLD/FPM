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
using MuPDFCore.MuPDFRenderer;
using System.Reflection.Metadata;
using System;
using Avalon.Model;
using static MuPDFCore.MuPDFStructuredTextBlock;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Avalonia.Media;
using Avalonia.Controls.Shapes;
using Newtonsoft.Json.Bson;
using System.Data;
using Material.Styles.Converters;

namespace Avalon.ViewModels
{
    public class PreviewViewModel : ViewModelBase, INotifyPropertyChanged
    {
        public PreviewViewModel() { }

        private WriteableBitmap? imageFromBinding = null;
        public WriteableBitmap? ImageFromBinding
        {
            get { return imageFromBinding; }
            set { imageFromBinding = value; }
        }

        private WriteableBitmap? imageFromBinding2 = null;
        public WriteableBitmap? ImageFromBinding2
        {
            get { return imageFromBinding2; }
            set { imageFromBinding2 = value; }
        }

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
            set { requestFile = value; OnPropertyChanged("RequestFile"); InitFile(); }
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
                    SetPage();
                }
            }
        }

        public int requestPage2 = 0;
        public int RequestPage2
        {
            get { return requestPage2; }
            set
            {
                if (PageInRange(value))
                {
                    requestPage2 = value;
                    OnPropertyChanged("RequestPage2");
                    SetPage(true);
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
            set { pagecount = value; OnPropertyChanged("Pagecount"); BitmapContainer = new WriteableBitmap[pagecount]; }
        }

        private WriteableBitmap[] BitmapContainer = new WriteableBitmap[1];

        private bool bitmapsStored = false;
        public bool BitmapsStored
        {
            get { return bitmapsStored; }
            set { bitmapsStored = value; OnPropertyChanged("BitmapsStored"); }
        }

        private double Scale
        {
            get
            {
                if (RequestFile.Filtyp == "Drawing")
                {
                    return 0.75;
                }
                else
                {
                    return 1.50;
                }

            }
        }


        Task<WriteableBitmap> BitmapTask = null;

        Task FileTask = null;

        private byte[] FileBytes = null;

        private bool FileWorkerBusy = false;
        private bool BitmapWorkerBusy = false;

        private MuPDFContext Context = new MuPDFContext();

        private async void InitFile()
        {
            if (!FileWorkerBusy)
            {
                Debug.WriteLine("Initializing new file: " + RequestFile.Namn);
                LinkedPageMode = true;
                FileWorkerBusy = true;

                ClearBitmaps();

                string filepath = RequestFile.Sökväg;

                FileTask = Task.Run(() => GetFile(filepath));
                FileTask.ContinueWith(delegate { CheckFile(filepath); });

            }
        }

        private void ClearBitmaps()
        {
            ImageFromBinding = null; OnPropertyChanged("ImageFromBinding");
            ImageFromBinding2 = null; OnPropertyChanged("ImageFromBinding2");
        }

        private async Task SafeDispose()
        {
            if (PreviewFile != null)
            {
                while (BitmapWorkerBusy) { Debug.WriteLine("Waiting to dispose of file..."); }
                PreviewFile.Dispose();
                Debug.WriteLine("File Disposed");
            }
        }

        private async Task GetFile(string filepath)
        {
            FileBytes = await Task.Run(() => File.ReadAllBytesAsync(filepath));
        }


        private async void CheckFile(string filepath)
        {
            if (RequestFile.Sökväg == filepath)
            {
                await SafeDispose();

                PreviewFile = new MuPDFDocument(Context, FileBytes, InputFileTypes.PDF);

                Pagecount = PreviewFile.Pages.Count;
                CurrentFile = RequestFile;
                FileWorkerBusy = false;

                RequestPage1 = 0;

                Debug.WriteLine("Done");
            }
            else
            {
                Debug.WriteLine("Rerunning");
                FileWorkerBusy = false;
                InitFile();
            }
        }

        private void SetPage(bool SecondPage = false)
        {
            if (!TwopageMode)
            {
                SetSinglePage1();
            }
            else
            {
                if (LinkedPageMode)
                {
                    SetDualPage();
                }
                else
                {
                    if (!SecondPage)
                    {
                        SetSinglePage1();
                    }
                    else
                    {
                        SetSinglePage2();
                    }
                }

            }
            
        }

        private async void SetSinglePage1()
        {
            int pagenr = RequestPage1;

            if (!BitmapWorkerBusy && !FileWorkerBusy)
            {
                BitmapWorkerBusy = true;
                ImageFromBinding = await Task.Run(() => GetPage(PreviewFile, pagenr, Scale));
                OnPropertyChanged("ImageFromBinding");
                BitmapWorkerBusy = false;

                if (RequestPage1 == pagenr)
                {
                    CurrentPage1 = pagenr;
                }
                else
                {
                    SetSinglePage1();
                }
            }
        }

        private async void SetSinglePage2()
        {
            int pagenr = RequestPage2;

            if (!BitmapWorkerBusy && !FileWorkerBusy)
            {
                BitmapWorkerBusy = true;
                ImageFromBinding2 = await Task.Run(() => GetPage(PreviewFile, pagenr, Scale));
                OnPropertyChanged("ImageFromBinding2");
                BitmapWorkerBusy = false;

                if (RequestPage2 == pagenr)
                {
                    CurrentPage2 = pagenr;
                }
                else
                {
                    SetSinglePage2();
                }
            }
        }

        private async void SetDualPage()
        {
            int pagenr = RequestPage1;

            if (!BitmapWorkerBusy && !FileWorkerBusy)
            {
                BitmapWorkerBusy = true;

                ImageFromBinding = await Task.Run(() => GetPage(PreviewFile, pagenr, Scale));
                ImageFromBinding2 = await Task.Run(() => GetPage(PreviewFile, pagenr + 1, Scale));

                OnPropertyChanged("ImageFromBinding");
                OnPropertyChanged("ImageFromBinding2");
                
                BitmapWorkerBusy = false;

                if (RequestPage1 == pagenr)
                {
                    CurrentPage1 = pagenr;
                }
                else
                {
                    SetDualPage();
                }
            }
        }



        private async Task<WriteableBitmap> GetPage(MuPDFDocument file, int pagenr, double zoom)
        {
            if (PageInRange(pagenr))
            {
                if (BitmapContainer[pagenr] != null)
                {
                    return BitmapContainer[pagenr];
                }
                else
                {
                    MuPDFCore.Rectangle bounds = file.Pages[pagenr].Bounds;
                    RoundedRectangle roundedBounds = bounds.Round(zoom);
                    WriteableBitmap bitmap = new WriteableBitmap(new PixelSize(roundedBounds.Width, roundedBounds.Height), new Vector(96, 96), PixelFormat.Rgba8888, AlphaFormat.Premul);

                    using (ILockedFramebuffer fb = bitmap.Lock())
                    {
                        file.Render(pagenr, bounds, zoom, MuPDFCore.PixelFormats.RGBA, fb.Address);
                    }
                    BitmapContainer[pagenr] = bitmap;
                    return bitmap;
                }
            }
            else
            {
                return null;
            }
        }


        public void NextPage(bool SecondPage = false)
        {
            if (!TwopageMode)
            {
                RequestPage1 = RequestPage1 + 1;
            }


            if (TwopageMode)
            {
                if (LinkedPageMode)
                {
                    RequestPage1 = RequestPage1 + 2;
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
            if (!TwopageMode)
            {
                RequestPage1 = RequestPage1 - 1;
            }

            if (TwopageMode)
            {
                if (LinkedPageMode)
                {
                    RequestPage1 = RequestPage1 - 2;
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

        private bool PageInRange(int pagenr)
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