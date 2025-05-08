using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace Avalon.Model
{
    public class GeneralData : INotifyPropertyChanged
    {
        private string savePath = "C:\\FIlePathManager\\Projects.json";
        public string SavePath
        {
            get { return savePath; }
            set { savePath = value; RaisePropertyChanged("SavePath"); }
        }

        private ObservableCollection<string> collections = new ObservableCollection<string>();
        public ObservableCollection<string> Collections
        {
            get { return collections; }
            set { collections = value; RaisePropertyChanged("Collections"); Debug.WriteLine(Collections.Count); }
        }




        private void RaisePropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
