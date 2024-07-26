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
using Avalonia.Controls;
using Avalonia.Threading;
using MuPDFCore.MuPDFRenderer;
using System.Drawing;
using Avalonia.Media;
using Avalonia.Interactivity;

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
            set { twopageMode = value;  OnPropertyChanged("TwopageMode"); if (!FileWorkerBusy) { SetPage(); }}
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
                    SetPage();
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
            set { dimmedBackground = value; SetDimmedMode(); OnPropertyChanged("DimmedBackground"); }
        }

        private bool fileWorkerBusy = false;
        public bool FileWorkerBusy
        {
            get { return fileWorkerBusy; }
            set { fileWorkerBusy = value; OnPropertyChanged("FileWorkerBusy"); }
        }


        private bool RenderWorkerBusy = false;

        private byte[] bytes;

        private byte[] tempbytes;

        private Task MainFileTask;

        private PdfDocument pdfSource;

        private PDFRenderer Renderer;

        MuPDFContext Context = null;

        MuPDFContext ContextDual = null;

        DateTime t0;

        public void GetRenderControl(PDFRenderer renderer)
        {
            Renderer = renderer;
        }

        private void SetFile()
        {
            Debug.WriteLine(FileWorkerBusy);
            if (!FileWorkerBusy)
            {
                FileWorkerBusy = true;
                SetMainFile();
            }
        }

        private void SetMainFile()
        {
            
            TwopageModeAvail = false;

            if(Renderer.IsEffectivelyVisible)
            {
                Renderer.IsVisible = false;
            }

            string path = RequestFile.Sökväg;

            MainFileTask = Task.Run(() => GetMainFile(path));
            MainFileTask.ContinueWith(delegate { CheckMainFile(path); });
        }



        private async Task GetMainFile(string path)
        {
            bytes = await Task.Run(() => File.ReadAllBytesAsync(path));
        }

        private async Task CheckMainFile(string path)
        {
            if (RequestFile.Sökväg == path)
            {
                await SafeDispose();
                await SetupMainFile();

                FileWorkerBusy = false;

                if (Pagecount > 1)
                {
                    SetDualFile();
                }
            }
            else
            {
                Debug.WriteLine("Rerunning");
                FileWorkerBusy = false;
                SetFile();
            }
        }

        private async Task SetupMainFile()
        {
            Context = new MuPDFContext();
            PreviewFile = new MuPDFDocument(Context, bytes, InputFileTypes.PDF);

            Pagecount = PreviewFile.Pages.Count;
            CurrentFile = RequestFile;
            RequestPage1 = 0;
            CurrentPage1 = 0;
            TwopageMode = false;

            Dispatcher.UIThread.Invoke(() => { Renderer.Initialize(PreviewFile, 1, 0, 0.5); });
            Dispatcher.UIThread.Invoke(() => { Renderer.IsVisible = true; });
        }

        private async void SetDualFile()
        {
            await Task.Run(() => GetDualPageFile());
            await Task.Run(() => GetDualPage());
            TwopageModeAvail = true;
        }


        private async Task GetDualPageFile()
        {
            Debug.WriteLine("DUAL PAGE FETCH");
            MemoryStream stream = new MemoryStream(bytes);
            pdfSource = new PdfDocument(new PdfReader(stream));
        }


        private async Task GetDualPage()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                PdfDocument pdf = new PdfDocument(new PdfWriter(ms));

                Debug.WriteLine(Pagecount);
                for (int i = 1; i < Pagecount + 1; i = i + 2) 
                {
                    
                    float width1 = 0;
                    float height1 = 0;
                    float width2 = 0;
                    float height2 = 0;

                    PdfPage page1 = null;
                    PdfPage page2 = null;


                    if (FileWorkerBusy)
                    {
                        Debug.WriteLine("ABORT!");
                        return;
                    }

                    page1 = pdfSource.GetPage(i);
                    iText.Kernel.Geom.Rectangle size1 = page1.GetPageSize();
                    height1 = size1.GetHeight();
                    width1 = size1.GetWidth();

                    if (i+1 <= pdfSource.GetNumberOfPages())
                    {
                        page2 = pdfSource.GetPage(i + 1);
                        iText.Kernel.Geom.Rectangle size2 = page2.GetPageSize();
                        height2 = size2.GetHeight();
                        width2 = size2.GetWidth();
                    }

                    float newWidth = width1 + width2;
                    float newHeight = Math.Max(height1, height2);

                    iText.Kernel.Geom.Rectangle bounds = new iText.Kernel.Geom.Rectangle(newWidth, newHeight);

                    PageSize nUpPageSize = new PageSize(bounds);

                    PdfPage targetPage = pdf.AddNewPage(nUpPageSize);
                    PdfCanvas canvas = new PdfCanvas(targetPage);

                    PdfFormXObject pageCopy1 = page1.CopyAsFormXObject(pdf);

                    canvas.AddXObjectAt(pageCopy1, 0, 0);

                    if (i+1 <= pdfSource.GetNumberOfPages())
                    {
                        PdfFormXObject pageCopy2 = page2.CopyAsFormXObject(pdf);
                        canvas.AddXObjectAt(pageCopy2, width1, 0);
                    }
                }


                pdf.Close();

                tempbytes = ms.ToArray();
            }
            await SafeDualDispose();
            ContextDual = new MuPDFContext();
            PreviewFileDual = new MuPDFDocument(ContextDual, tempbytes, InputFileTypes.PDF);


            Debug.WriteLine("DUAL PAGE SET");

        }

        public async Task SafeDispose()
        {

            while (RenderWorkerBusy)
            {
                Debug.WriteLine("Waiting");
            }

            if (PreviewFile != null)
            {
                Dispatcher.UIThread.Invoke(() =>
                {
                    Renderer.ReleaseResources();
                    PreviewFile?.Dispose();
                    Context?.Dispose();
                });
            }
        }

        public async Task SafeDualDispose()
        {
            if (PreviewFileDual != null)
            {
                PreviewFileDual?.Dispose();
                ContextDual?.Dispose();
            }
        }

        private async void SetPage()
        {
            if (FileWorkerBusy)
            {
                return;
            }

            if (!RenderWorkerBusy)
            {
                RenderPage(RequestPage1);
            }
        }

        private void RenderPage(int pagenr)
        {
            Renderer.IsVisible = false;

            if (TwopageMode)
            {
                Renderer.Initialize(PreviewFileDual, 1, pagenr / 2, 0.5);
            }
            else
            {
                Renderer.Initialize(PreviewFile, 1, pagenr, 0.5);
            }

            CurrentPage1 = RequestPage1;
            Renderer.IsVisible = true;
        }

        private async Task RenderPage2(int pagenr)
        {
            RenderWorkerBusy = true;
            //Renderer.IsVisible = false;
            t0 = DateTime.Now;
            if (TwopageMode)
            {
                await Renderer.InitializeAsync(PreviewFileDual, 1, pagenr/2, 0.5);
            }
            else
            {
                await Renderer.InitializeAsync(PreviewFile, 1, pagenr, 0.5);
            }


            if (RequestPage1 == pagenr)
            {
                CurrentPage1 = RequestPage1;
                //Renderer.IsVisible = true;
                RenderWorkerBusy = false;
            }
            else
            {
                RenderPage(RequestPage1);
            }
            Debug.WriteLine(DateTime.Now - t0);
        }


        public void NextPage(bool SecondPage = false)
        {
            t0 = DateTime.Now;
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

        public void SetDimmedMode()
        {
            if (DimmedBackground)
            {
                Renderer.PageBackground = new SolidColorBrush(Colors.AntiqueWhite);
            }
            else
            {
                Renderer.PageBackground = new SolidColorBrush(Colors.White);
            }

            SetPage();
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