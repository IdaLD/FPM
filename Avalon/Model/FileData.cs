using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Avalon.Model
{
    public class FileData : INotifyPropertyChanged
    {

        private string namn = string.Empty;
        public string Namn
        {
            get { return namn; }
            set { namn = value; RaisePropertyChanged("Namn"); }
        }

        private string fileStatus = string.Empty;
        public string FileStatus
        {
            get { return fileStatus; }
            set { fileStatus = value; RaisePropertyChanged("FileStatus"); }
        }

        private string tagg = string.Empty;
        public string Tagg
        {
            get { return tagg; }
            set { tagg = value; RaisePropertyChanged("Tagg"); }
        }

        private string färg = string.Empty;
        public string Färg
        {
            get { return färg; }
            set { färg = value; RaisePropertyChanged("Färg"); }
        }

        private string handling = string.Empty;
        public string Handling
        {
            get { return handling; }
            set { handling = value; RaisePropertyChanged("Handling"); }
        }

        private string status = string.Empty;
        public string Status
        {
            get { return status; }
            set { status = value; RaisePropertyChanged("Status"); }
        }

        private string datum = string.Empty;
        public string Datum
        {
            get { return datum; }
            set { datum = value; RaisePropertyChanged("Datum"); }
        }

        private string ritningstyp = string.Empty;
        public string Ritningstyp
        {
            get { return ritningstyp; }
            set { ritningstyp = value; RaisePropertyChanged("Ritningstyp"); }
        }

        private string beskrivning1 = string.Empty;
        public string Beskrivning1
        {
            get { return beskrivning1; }
            set { beskrivning1 = value; RaisePropertyChanged("Beskrivning1"); }
        }

        private string beskrivning2 = string.Empty;
        public string Beskrivning2
        {
            get { return beskrivning2; }
            set { beskrivning2 = value; RaisePropertyChanged("Beskrivning2"); }
        }

        private string beskrivning3 = string.Empty;
        public string Beskrivning3
        {
            get { return beskrivning3; }
            set { beskrivning3 = value; RaisePropertyChanged("Beskrivning3"); }
        }

        private string beskrivning4 = string.Empty;
        public string Beskrivning4
        {
            get { return beskrivning4; }
            set { beskrivning4 = value; RaisePropertyChanged("Beskrivning4"); }
        }

        private string uppdrag = string.Empty;
        public string Uppdrag
        {
            get { return uppdrag; }
            set { uppdrag = value; RaisePropertyChanged("Uppdrag"); }
        }

        private string filtyp = string.Empty;
        public string Filtyp
        {
            get { return filtyp; }
            set { filtyp = value; RaisePropertyChanged("Filtyp"); }
        }

        private string revidering = string.Empty;
        public string Revidering
        {
            get { return revidering; }
            set { revidering = value; RaisePropertyChanged("Revidering"); }
        }

        private string sökväg = string.Empty;
        public string Sökväg
        {
            get { return sökväg; }
            set { sökväg = value; RaisePropertyChanged("Sökväg"); }
        }

        private int defaultPage;
        public int DefaultPage
        {
            get { return defaultPage; }
            set { defaultPage = value; RaisePropertyChanged("DefaultPage"); }
        }

        private ObservableCollection<PageData> favPages = new ObservableCollection<PageData>();
        public ObservableCollection<PageData> FavPages
        {
            get { return favPages; }
            set { favPages = value; RaisePropertyChanged("FavPages"); }
        }

        private string info = string.Empty;
        public string Info
        {
            get { return info; }
            set { info = value; RaisePropertyChanged("Info"); }
        }


        private void RaisePropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
