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
using System.Runtime.InteropServices;
using System;
using System.Security.Cryptography;
using iText.Kernel.Pdf;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Layout.Properties;
using iText.Kernel.Pdf.Xobject;
using iText.Kernel.Pdf.Canvas;
using Org.BouncyCastle.Asn1.BC;

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

        public MuPDFDocument previewFileDual = null;
        public MuPDFDocument PreviewFileDual
        {
            get { return previewFileDual; }
            set { previewFileDual = value; OnPropertyChanged("PreviewFileDual"); }
        }


        public bool twopageMode = false;
        public bool TwopageMode
        {
            get { return twopageMode; }
            set { twopageMode = value; OnPropertyChanged("TwopageMode"); }
        }

        public bool twopageModeAvail = false;
        public bool TwopageModeAvail
        {
            get { return twopageModeAvail; }
            set { twopageModeAvail = value; OnPropertyChanged("TwopageModeAvail"); }
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

        private byte[] bytes;

        private byte[] tempbytes;

        private void SetFile()
        {
            if (!FileWorkerBusy)
            {
                string path = RequestFile.Sökväg;
                SetFileTask(path);
            }
        }

        private PdfDocument pdfSource;

        private async void SetFileTask(string path)
        {
            //FileWorkerBusy = true;
            TwopageMode = false;
            SafeDispose();
            TwopageModeAvail = false;
            bytes = File.ReadAllBytes(path);

            IntPtr ptr = Marshal.AllocHGlobal(bytes.Length);

            Marshal.Copy(bytes, 0, ptr, bytes.Length);
            IDisposable dispIntPtr = new DisposableIntPtr(ptr);
            PreviewFile = new MuPDFDocument(new MuPDFContext(), ptr, bytes.Length, InputFileTypes.PDF, ref dispIntPtr);

            RequestPage1 = 0;
            Pagecount = PreviewFile.Pages.Count;
            CurrentFile = RequestFile;

            if (PreviewFile.Pages.Count > 1)
            {
                GetDualPageFile();
                GetDualPage();
                TwopageModeAvail = true;
            }


        }

        private void GetDualPageFile()
        {
            Debug.WriteLine("DUAL PAGE FETCH");
            DateTime t0 = DateTime.Now;

            MemoryStream stream = new MemoryStream(bytes);

            pdfSource = new PdfDocument(new PdfReader(stream));

            Pagecount = pdfSource.GetNumberOfPages();

            Debug.WriteLine(Pagecount);

            Debug.WriteLine(DateTime.Now - t0);
        }


        private void GetDualPage()
        {
            DateTime t0 = DateTime.Now;

            using (MemoryStream ms = new MemoryStream())
            {
                PdfDocument pdf = new PdfDocument(new PdfWriter(ms));

                for (int i = 1; i < pdfSource.GetNumberOfPages(); i = i + 2) 
                {
                    PdfPage page1 = pdfSource.GetPage(i);
                    PdfPage page2 = pdfSource.GetPage(i+1);

                    iText.Kernel.Geom.Rectangle size1 = page1.GetPageSize();
                    iText.Kernel.Geom.Rectangle size2 = page2.GetPageSize();

                    float height1 = size1.GetHeight();
                    float width1 = size1.GetWidth();

                    float height2 = size2.GetHeight();
                    float width2 = size2.GetWidth();

                    float newWidth = width1 + width2;
                    float newHeight = Math.Max(height1, height2);

                    iText.Kernel.Geom.Rectangle bounds = new iText.Kernel.Geom.Rectangle(newWidth, newHeight);

                    PageSize nUpPageSize = new PageSize(bounds);

                    
                    PdfPage targetPage = pdf.AddNewPage(nUpPageSize);
                    PdfCanvas canvas = new PdfCanvas(targetPage);

                    PdfFormXObject pageCopy1 = page1.CopyAsFormXObject(pdf);
                    PdfFormXObject pageCopy2 = page2.CopyAsFormXObject(pdf);

                    canvas.AddXObjectAt(pageCopy1, 0, 0);
                    canvas.AddXObjectAt(pageCopy2, width1, 0);
                }


                pdf.Close();

                tempbytes = ms.ToArray();
            }

            PreviewFileDual = new MuPDFDocument(new MuPDFContext(), tempbytes, InputFileTypes.PDF);


            Debug.WriteLine(DateTime.Now - t0);
            Debug.WriteLine("DUAL PAGE SET");

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

            if (PreviewFileDual != null)
            {
                PreviewFileDual.Dispose();
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