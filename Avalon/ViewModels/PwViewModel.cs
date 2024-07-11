using System;
using System.Collections.Generic;
using Avalon.ViewModels;
using System.ComponentModel;
using Avalonia.Media.Imaging;
using Docnet.Core.Readers;
using Avalonia.Platform;
using Avalonia;
using Docnet.Core.Models;
using Docnet.Core;
using System.Runtime.InteropServices;

namespace Avalon.ViewModels
{
	public class PwViewModel : ViewModelBase, INotifyPropertyChanged
    {
		public PwViewModel() { }

        private WriteableBitmap? imageFromBinding = null;
        public WriteableBitmap? ImageFromBinding
        {
            get { return imageFromBinding; }
            set { imageFromBinding = value; OnPropertyChanged("ImageFromBinding"); }
        }

        private WriteableBitmap? imageFromBinding2 = null;
        public WriteableBitmap? ImageFromBinding2
        {
            get { return imageFromBinding2; }
            set { imageFromBinding2 = value; OnPropertyChanged("ImageFromBinding2"); }
        }

        public IDocReader docReader { get; set; } = null;

        public int _pw_pagenr = 0;
        public int pw_pagenr
        {
            get { return _pw_pagenr; }
            set { _pw_pagenr = value; OnPropertyChanged("pw_pagenr"); }
        }
        public int _pw_pagenr_view = 1;
        public int pw_pagenr_view
        {
            get { return _pw_pagenr_view; }
            set { _pw_pagenr_view = value; OnPropertyChanged("pw_pagenr_view"); }
        }

        public int _pw_pagecount_view = 1;
        public int pw_pagecount_view
        {
            get { return _pw_pagecount_view; }
            set { _pw_pagecount_view = value; OnPropertyChanged("pw_pagecount_view"); }
        }

        public bool _pw_dualmode = false;
        public bool pw_dualmode
        {
            get { return _pw_dualmode; }
            set { _pw_dualmode = value; OnPropertyChanged("pw_dualmode"); }
        }


        public void create_preview_file(string filepath, int fak)
        {
            if (docReader != null)
            {
                docReader.Dispose();
            }

            try
            {
                pw_pagenr = 0;
                docReader = DocLib.Instance.GetDocReader(filepath, new PageDimensions(fak * 1080 / 2, fak * 1920 / 2));
                pw_pagecount_view = docReader.GetPageCount();
                pw_pagenr_view = 1;

            }
            catch { return; }
        }

        public void clear_preview_file()
        {
            docReader = null;
            ImageFromBinding = null;
            ImageFromBinding2 = null;
        }

        public void next_preview_page()
        {
            if (pw_pagenr < docReader.GetPageCount() - 1)
            {
                if (pw_dualmode == false)
                {
                    pw_pagenr++;
                    preview_page(pw_pagenr, 0);
                }


                if (pw_dualmode == true)
                {
                    pw_pagenr = pw_pagenr + 2;

                    preview_page(pw_pagenr, 0);
                    preview_page(pw_pagenr + 1, 1);
                }

                pw_pagenr_view = pw_pagenr + 1;
            }
        }

        public void previous_preview_page()
        {
            if (pw_dualmode == false)
            {
                if (pw_pagenr > 0)
                {
                    pw_pagenr--;
                    preview_page(pw_pagenr, 0);
                }
            }

            if (pw_dualmode == true)
            {
                if (pw_pagenr > 1)
                {
                    preview_page(pw_pagenr - 2, 0);
                    preview_page(pw_pagenr - 1, 1);

                    pw_pagenr = pw_pagenr - 2;
                }
            }

            pw_pagenr_view = pw_pagenr + 1;

        }

        public void selected_page(int pagenr)
        {
            if (pagenr != pw_pagenr)
            {
                if (pw_dualmode == false)
                {
                    preview_page(pagenr, 0);
                }

                if (pw_dualmode == true)
                {
                    preview_page(pagenr, 0);
                    preview_page(pagenr + 1, 1);
                }

                pw_pagenr = pagenr;
            }
        }

        public void toggle_pw_mode()
        {
            pw_dualmode = !pw_dualmode;
            start_preview_page();
        }

        public void start_preview_page()
        {
            imageFromBinding = null;
            imageFromBinding2 = null;

            if (pw_dualmode == false)
            {
                pw_pagenr = 0;
                preview_page(pw_pagenr, 0);
            }
            if (pw_dualmode == true)
            {
                pw_pagenr = 0;
                preview_page(pw_pagenr, 0);
                preview_page(pw_pagenr + 1, 1);
            }

            pw_pagenr_view = pw_pagenr + 1;
        }

        public void preview_page(int pagenr, int mode)
        {
            if (docReader != null && docReader.GetPageCount() - 1 >= pagenr)
            {

                IPageReader page = docReader.GetPageReader(pagenr);

                byte[] rawBytes = page.GetImage();
                int width = page.GetPageWidth();
                int height = page.GetPageHeight();

                Avalonia.Vector dpi = new Avalonia.Vector(96, 96);

                if (mode == 0)
                {
                    ImageFromBinding = new WriteableBitmap(new PixelSize(width, height), dpi, Avalonia.Platform.PixelFormat.Bgra8888, AlphaFormat.Premul);
                    using (var frameBuffer = ImageFromBinding.Lock())
                    {
                        Marshal.Copy(rawBytes, 0, frameBuffer.Address, rawBytes.Length);
                    }
                    ImageFromBinding2 = null;
                }

                if (mode == 1)
                {
                    ImageFromBinding2 = new WriteableBitmap(new PixelSize(width, height), dpi, Avalonia.Platform.PixelFormat.Bgra8888, AlphaFormat.Premul);
                    using (var frameBuffer = ImageFromBinding2.Lock())
                    {
                        Marshal.Copy(rawBytes, 0, frameBuffer.Address, rawBytes.Length);
                    }
                }

            }
        }

    }
}