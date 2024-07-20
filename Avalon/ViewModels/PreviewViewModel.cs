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




        public bool twopageMode = false;
        public bool TwopageMode
        {
            get { return twopageMode; }
            set { twopageMode = value; OnPropertyChanged("TwopageMode"); }
        }

        public bool sourcMode = false;
        public bool SourceMode
        {
            get { return sourcMode; }
            set { sourcMode = value; OnPropertyChanged("SourceMode"); }
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

        public int requestPage = 0;
        public int RequestPage
        {
            get { return requestPage; }
            set { requestPage = value; OnPropertyChanged("RequestPage"); SetPage(); }
        }

        public int currentPage = 0;
        public int CurrentPage
        {
            get { return currentPage; }
            set { currentPage = value; OnPropertyChanged("CurrentPage"); }
        }


        public int pagecount = 0;
        public int Pagecount
        {
            get { return pagecount; }
            set { pagecount = value; OnPropertyChanged("Pagecount"); }
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

        public DateTime totaltime;

        private async void InitFile()
        {
            if (!FileWorkerBusy)
            {

                Debug.WriteLine("Initializing new file: " + RequestFile.Namn);
                FileWorkerBusy = true;

                ClearBitmaps();

                string filepath = RequestFile.Sökväg;

                FileTask = Task.Run(() => GetFile3(filepath));
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

        private async Task GetFile2(string filepath)
        {
            BinaryReader binReader = new BinaryReader(File.Open(filepath, FileMode.Open, FileAccess.Read));
            binReader.BaseStream.Position = 0;
            FileBytes = binReader.ReadBytes(Convert.ToInt32(binReader.BaseStream.Length));
            binReader.Close();
        }

        private async Task GetFile3(string filepath)
        {
            await SafeDispose();
            int approach = 1;
            MuPDFContext Context = new MuPDFContext();


            DateTime t0 = DateTime.Now;
            if (approach == 0)
            {
                PreviewFile = new MuPDFDocument(new MuPDFContext(), filepath);
            }
            if (approach == 1)
            {
                byte[] bytes = File.ReadAllBytes(filepath);
                PreviewFile = new MuPDFDocument(Context, bytes, InputFileTypes.PDF);
            }
            if (approach == 2)
            {
                FileStream fs = File.Open(filepath, FileMode.Open, FileAccess.Read);
                MemoryStream ms = new MemoryStream();
                fs.CopyTo(ms);
                PreviewFile = new MuPDFDocument(new MuPDFContext(), ref ms, InputFileTypes.PDF);
            }
            PreviewFile.ClipToPageBounds = true;
            

            DateTime t1 = DateTime.Now;

            totaltime = totaltime + (t1 - t0);
            Debug.WriteLine(PreviewFile.Pages.Count);

        }


        private async void CheckFile(string filepath)
        {
            if (RequestFile.Sökväg == filepath)
            {
                //await SafeDispose();
                ConstructFile(FileBytes);

                Debug.WriteLine("Done");
            }
            else
            {
                Debug.WriteLine("Rerunning");
                FileWorkerBusy = false;
                InitFile();
            }
        }

        private void ConstructFile(byte[] FileBytes)
        {
            //PreviewFile = new MuPDFDocument(new MuPDFContext(), FileBytes, InputFileTypes.PDF);

            Pagecount = PreviewFile.Pages.Count;
            CurrentFile = RequestFile;
            FileWorkerBusy = false;
            RequestPage = 0;
        }



        private void SetPage()
        {
            Debug.WriteLine("SETPAGE: " + RequestPage);
            Debug.WriteLine("Checking page");
            if (!BitmapWorkerBusy && !FileWorkerBusy)
            {
                BitmapWorkerBusy = true;
                PageFromFile(RequestPage);
            }
        }


        private async void PageFromFile(int pagenr)
        {
            
            BitmapTask = Task.Run(() => GetPage(pagenr, Scale));

            ImageFromBinding = await BitmapTask;

            if (TwopageMode == true)
            {
                ImageFromBinding2 = null;
                if (PageInRange(pagenr + 1))
                {
                    BitmapTask = Task.Run(() => GetPage(pagenr + 1, Scale));
                    ImageFromBinding2 = await BitmapTask;
                }
            }

            OnPropertyChanged("ImageFromBinding");
            if (TwopageMode == true)
            {
                OnPropertyChanged("ImageFromBinding2");
            }

            BitmapTask.ContinueWith(delegate { CheckPage(pagenr);});
            
        }

        private void PageFromStorage(int pagenr)
        {

        }

        private void CheckPage(int pagenr)
        {
            if (RequestPage != pagenr)
            {
                PageFromFile(RequestPage);
            }
            else
            {
                CurrentPage = pagenr;
                BitmapWorkerBusy = false;
            }

        }

        private async Task<WriteableBitmap> GetPage(int pagenr, double zoom)
        {
            Debug.WriteLine("Fetching page");
            DateTime t0 = DateTime.Now;
            MuPDFCore.Rectangle bounds = PreviewFile.Pages[pagenr].Bounds;
            RoundedRectangle roundedBounds = bounds.Round(zoom);
            WriteableBitmap bitmap = new WriteableBitmap(new PixelSize(roundedBounds.Width, roundedBounds.Height), new Vector(96, 96), PixelFormat.Rgba8888, AlphaFormat.Premul);

            using (ILockedFramebuffer fb = bitmap.Lock())
            {
                PreviewFile.Render(pagenr, bounds, zoom, MuPDFCore.PixelFormats.RGBA, fb.Address);
            }
            DateTime t1 = DateTime.Now;
            totaltime = totaltime + (t1-t0);
            Debug.WriteLine(totaltime);
            return bitmap;
        }


        public void NextPage()
        {
            if (!TwopageMode)
            {
                if (PageInRange(RequestPage + 1))
                {
                    RequestPage = RequestPage + 1;
                }
            }
            else
            {
                if (PageInRange(RequestPage + 2))
                {
                    RequestPage = RequestPage + 2;
                }
            }
        }

        public void PrevPage()
        {
            if (!TwopageMode)
            {
                if (PageInRange(RequestPage - 1))
                {
                    RequestPage = RequestPage - 1;
                }
            }
            else
            {
                if (PageInRange(RequestPage - 2))
                {
                    RequestPage = RequestPage - 2;
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

        public void on_toggle_sourcemode()
        {

        }


        public void toggle_pw_mode()
        {
            TwopageMode = !TwopageMode;

            if (TwopageMode)
            {
                if (CurrentPage %2 == 0)
                {
                    RequestPage = CurrentPage;
                }
                else
                {
                    RequestPage = CurrentPage - 1;
                }
                Debug.WriteLine(RequestPage);
            }

        }

    }
}