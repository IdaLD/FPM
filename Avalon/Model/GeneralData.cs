using Avalonia;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Avalonia.Media;
using Org.BouncyCastle.Asn1.BC;

namespace Avalon.Model
{
    public class GeneralData : INotifyPropertyChanged
    {
        private string savePath = "C:\\FIlePathManager";
        public string SavePath
        {
            get { return savePath; }
            set { savePath = value; RaisePropertyChanged("SavePath"); }
        }

        private Color color1 = Color.Parse("#333333");
        public Color Color1
        {
            get { return color1; }
            set { color1 = value; RaisePropertyChanged("Color1"); }
        }

        private Color color2 = Color.Parse("#444444");
        public Color Color2
        {
            get { return color2; }
            set { color2 = value; RaisePropertyChanged("Color2"); }
        }

        private Color color3 = Color.Parse("#dfe6e9");
        public Color Color3
        {
            get { return color3; }
            set { color3 = value; RaisePropertyChanged("Color3"); }
        }

        private Color color4 = Color.Parse("#999999");
        public Color Color4
        {
            get { return color4; }
            set { color4 = value; RaisePropertyChanged("Color4"); }
        }


        private bool cornerRadiusVal = true;
        public bool CornerRadiusVal
        {
            get { return cornerRadiusVal; }
            set { cornerRadiusVal = value; RaisePropertyChanged("CornerRadiusVal"); SetCornerRadius(); }
        }

        private CornerRadius cornerRadius = new CornerRadius(10);
        public CornerRadius CornerRadius
        {
            get { return cornerRadius; }
            set { cornerRadius = value; RaisePropertyChanged("CornerRadius"); }
        }

        private bool borderVal = false;
        public bool BorderVal
        {
            get { return borderVal; }
            set { borderVal = value; RaisePropertyChanged("BorderVal"); SetBorder(); }
        }

        private Thickness border = new Thickness(0);
        public Thickness Border
        {
            get { return border; }
            set { border = value; RaisePropertyChanged("Border"); }
        }

        private int fontSize = 16;
        public int FontSize
        {
            get { return fontSize; }
            set { fontSize = value; RaisePropertyChanged("FontSize"); RaisePropertyChanged("RowHeight"); GetSystemFonts(); }
        }
        public int RowHeight
        {
            get { return FontSize + 2; }
        }

        private ObservableCollection<string> availableFonts = new ObservableCollection<string> () {};
        public ObservableCollection<string> AvailableFonts
        {
            get { return availableFonts; }
            set { availableFonts = value; RaisePropertyChanged("AvailableFonts"); }
        }


        private string selectedFontString;
        public string SelectedFontString
        {
            get { return selectedFontString; }
            set { selectedFontString = value; RaisePropertyChanged("SelectedFontString"); SelectedFont = new FontFamily(SelectedFontString); }
        }


        private FontFamily selectedFont = new FontFamily("Ubuntu");
        public FontFamily SelectedFont
        {
            get { return selectedFont; }
            set { selectedFont = value; RaisePropertyChanged("SelectedFont"); }
        }



        private ObservableCollection<string> collections = new ObservableCollection<string>();
        public ObservableCollection<string> Collections
        {
            get { return collections; }
            set { collections = value; RaisePropertyChanged("Collections"); }
        }

        private bool embeddedPreviewerOpen = false;
        public bool EmbeddedPreviewerOpen
        {
            get { return embeddedPreviewerOpen; }
            set { embeddedPreviewerOpen = value; RaisePropertyChanged("EmbeddedPreviewerOpen"); }
        }


        private void GetSystemFonts()
        {
            AvailableFonts.Clear();

            foreach (FontFamily fontFamily in FontManager.Current.SystemFonts)
            {
                AvailableFonts.Add(fontFamily.Name);
                Debug.WriteLine(fontFamily.FamilyNames);
            }

        }


        private void SetCornerRadius()
        {
            if (CornerRadiusVal)
            {
                CornerRadius = new CornerRadius(10);
            }
            else
            {
                CornerRadius = new CornerRadius(0);
            }
        }

        private void SetBorder()
        {
            if (BorderVal)
            {
                Border = new Thickness(0.6);
            }
            else
            {
                Border = new Thickness(0);
            }
        }

        private void RaisePropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
