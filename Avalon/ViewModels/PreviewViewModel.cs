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
using System.Text.RegularExpressions;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using Avalonia.Collections;

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

        private bool DualWorkerBusy = false;

        private byte[] bytes;

        private byte[] tempbytes;

        private PdfDocument pdfSource;

        private PDFRenderer Renderer;

        MuPDFContext Context = null;

        MuPDFContext ContextDual = null;

        private Regex Regex = null;

        private AvaloniaList<int> searchPages = new AvaloniaList<int>() { };
        public AvaloniaList<int> SearchPages
        {
            get { return searchPages; }
            set { searchPages = value; OnPropertyChanged("SearchPages"); }
        }

        private AvaloniaList<string> searchPagesText = new AvaloniaList<string>() { };
        public AvaloniaList<string> SearchPagesText
        {
            get { return searchPagesText; }
            set { searchPagesText = value; OnPropertyChanged("SearchPagesText"); }
        }

        private int searchPageIndex = 0;
        public int SearchPageIndex
        {
            get { return searchPageIndex; }
            set { searchPageIndex = value; SetSearchPage(); OnPropertyChanged("SearchPageIndex"); }
        }

        private int searchItems = 0;
        public int SearchItems
        {
            get { return searchItems; }
            set { searchItems = value; OnPropertyChanged("SearchItems"); }
        }

        private bool searchMode = false;
        public bool SearchMode
        {
            get { return searchMode; }
            set { searchMode = value; OnPropertyChanged("SearchMode"); }
        }

        private bool searchBusy = false;
        public bool SearchBusy
        {
            get { return searchBusy; }
            set { searchBusy = value; OnPropertyChanged("SearchBusy"); }
        }

        private CancellationTokenSource DualCts = new CancellationTokenSource();

        private CancellationTokenSource SearchCts = new CancellationTokenSource();

        private bool FileAvailable = false;

        private int progress = 0;
        public int Progress
        {
            get { return progress; }
            set { progress = value; OnPropertyChanged("Progress"); }
        }

        public void GetRenderControl(PDFRenderer renderer)
        {
            Renderer = renderer;
        }

        private void SetFile()
        {

            if (!FileWorkerBusy)
            {
                FileWorkerBusy = true;
                FileAvailable = false;
                TwopageModeAvail = false;
                TwopageMode = false;

                StopSearch();
                ClearSearch();

                if (Renderer.IsEffectivelyVisible)
                {
                    Renderer.IsVisible = false;
                }

                Task.Run(()=>SetMainFile());
            }
        }

        private async Task SetMainFile()
        {
            string path = RequestFile.Sökväg;

            bytes = await ReadBytesWithProgress(path);

            if (RequestFile.Sökväg == path)
            {
                await SafeDispose();
                await SetupMainFile();

                FileWorkerBusy = false;
                FileAvailable = true;

                if (Pagecount > 1)
                {
                    SetDualFile();
                }
            }
            else
            {
                FileWorkerBusy = false;
                SetFile();
            }
        }

        private async Task<byte[]> ReadBytesWithProgress(string path)
        {
            Progress = 0;

            using (MemoryStream ms = new MemoryStream())
            {
                using (Stream source = File.OpenRead(path))
                {
                    long total = source.Length;


                    byte[] buffer = new byte[4096];
                    int bytesRead;

                    int steps = (int)(total / buffer.Length);
                    int leap = steps / 20;

                    int i = 0;

                    while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        ms.Write(buffer, 0, bytesRead);

                        if (leap > 20)
                        {
                            if (i % leap == 0)
                            {
                                Progress = 100 * i / steps;
                            }
                        }

                        i++;
                    }
                }
                Progress = 0;

                return ms.ToArray();
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

            Dispatcher.UIThread.Invoke(() => { Renderer.Initialize(PreviewFile, 4, 0, 0.5); });
            Dispatcher.UIThread.Invoke(() => { Renderer.IsVisible = true; });
        }

        private async void SetDualFile()
        {
            DualCts = new CancellationTokenSource();

            await Task.Run(() => GetDualPageFile());
            await Task.Run(() => GetDualPage(DualCts.Token));
            TwopageModeAvail = true;
        }


        private async Task GetDualPageFile()
        {
            Debug.WriteLine("DUAL PAGE FETCH");
            MemoryStream stream = new MemoryStream(bytes);
            pdfSource = new PdfDocument(new PdfReader(stream));
        }


        private async Task GetDualPage(CancellationToken Token)
        {
            try
            {

                using (MemoryStream ms = new MemoryStream())
                {
                    PdfDocument pdf = new PdfDocument(new PdfWriter(ms));

                    for (int i = 1; i < Pagecount + 1; i = i + 2)
                    {
                        Token.ThrowIfCancellationRequested();

                        float width1 = 0;
                        float height1 = 0;
                        float width2 = 0;
                        float height2 = 0;

                        PdfPage page1 = null;
                        PdfPage page2 = null;

                        page1 = pdfSource.GetPage(i);
                        iText.Kernel.Geom.Rectangle size1 = page1.GetPageSize();
                        height1 = size1.GetHeight();
                        width1 = size1.GetWidth();

                        if (i + 1 <= pdfSource.GetNumberOfPages())
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

                        if (i + 1 <= pdfSource.GetNumberOfPages())
                        {
                            PdfFormXObject pageCopy2 = page2.CopyAsFormXObject(pdf);
                            canvas.AddXObjectAt(pageCopy2, width1, 0);
                        }
                    }

                    DualWorkerBusy = false;

                    pdf.Close();

                    tempbytes = ms.ToArray();
                }
                await SafeDualDispose();
                ContextDual = new MuPDFContext();
                PreviewFileDual = new MuPDFDocument(ContextDual, tempbytes, InputFileTypes.PDF);

                Debug.WriteLine("DUAL PAGE SET");
                DualWorkerBusy = false;
            }
            catch
            {
                Debug.WriteLine("CT CANCEL");
                DualWorkerBusy = false;
            }

            

        }

        public async Task SafeDispose()
        {
            DualCts.Cancel();

            while (RenderWorkerBusy || DualWorkerBusy || SearchBusy)
            {
                Debug.WriteLine("Waiting");
                await Task.Delay(25);
            }

            if (PreviewFile != null)
            {
                Dispatcher.UIThread.Invoke(() =>
                {
                    DisposeHighlight();

                    Renderer?.ReleaseResources();
                    PreviewFile?.Dispose();
                    Context?.Dispose();
                });

                FileAvailable = false;
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
            if (FileWorkerBusy || SearchBusy)
            {
                return;
            }

            if (!RenderWorkerBusy)
            {
                RenderPage(RequestPage1);
            }
        }

        public void RenderPage(int pagenr)
        {

            DisposeHighlight();
                
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

            if (SearchPages != null)
            {
                if (SearchPages.Contains(pagenr))
                {
                    Renderer.Search(Regex);
                    searchPageIndex = SearchPages.IndexOf(pagenr);
                    OnPropertyChanged("SearchPageIndex");
                }
            }

            Renderer.IsVisible = true;
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

        public void Search(string text)
        {
            if (FileAvailable && text != "" && text != null)
            {
                ClearSearch();

                Regex = new Regex(text, RegexOptions.IgnoreCase);

                if (SearchPages.Count > 0)
                {
                    SearchPages.Clear();
                }

                SearchCts = new CancellationTokenSource();

                Task.Run(() => SearchDocumentAsync(SearchCts.Token));

            }
        }

        public async void SearchDocumentAsync(CancellationToken Token)
        {
            try
            {
                SearchBusy = true;
                SearchPagesText.Clear();

                for (int i = 0; i < Pagecount; i++)
                {
                    Token.ThrowIfCancellationRequested();

                    using (IDisposable disposable = PreviewFile.GetStructuredTextPage(i))
                    {
                        MuPDFStructuredTextPage StructuredPage = (MuPDFStructuredTextPage)disposable;

                        int nr = StructuredPage.Search(Regex).Count();

                        if (nr != 0)
                        {
                            SearchPages.Add(i);
                            SearchItems = SearchItems + 1;
                            SearchPagesText.Add("Page: " + (i + 1).ToString() + " - " + nr + " items");
                        }
                    }
                }

                if (SearchItems > 0)
                {

                    SearchBusy = false;

                    Dispatcher.UIThread.Invoke(() =>
                    {
                        SearchPageIndex = 0;
                        RequestPage1 = SearchPages[SearchPageIndex];
                        Renderer.Search(Regex);
                    });

                }
                
            }
            catch
            {
                Debug.WriteLine("CTS SEARCH ABORT");
                SearchBusy = false;
            }

        }

        public void NextSearchPage()
        {
            if (Regex != null && SearchPages != null)
            {
                if (SearchPageIndex < SearchPages.Count - 1)
                {
                    SearchPageIndex++;
                }
            }
        }

        public void PrevSearchPage()
        {
            if (Regex != null && SearchPages != null)
            {
                if (SearchPageIndex > 0)
                {
                    SearchPageIndex--;
                }
            }
        }
        
        private void SetSearchPage()
        {
            if (SearchMode && SearchItems != 0)
            {
                RequestPage1 = SearchPages[SearchPageIndex];
            }
        }

        private void DisposeHighlight()
        {
            if (Renderer.HighlightedRegions != null)
            {
                Renderer.HighlightedRegions = null;
            }
        }



        public async Task StopSearch()
        {
            if (SearchBusy)
            {
                SearchCts.Cancel();

                while (SearchBusy)
                {
                    await Task.Delay(25);
                }
            }
        }


        public void ClearSearch()
        {
            SearchItems = 0;
            SearchPageIndex = 0;

            SearchPagesText.Clear();
            SearchPages.Clear();
        }


        public async Task CloseRenderer()
        {
            await StopSearch();
            ClearSearch();

            await SafeDispose();

        }
    }
}