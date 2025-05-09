using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Avalon.Model
{
    public class StoreData : INotifyPropertyChanged
    {

        private ObservableCollection<ProjectData> storedProjects = new ObservableCollection<ProjectData>();
        public ObservableCollection<ProjectData> StoredProjects
        {
            get { return storedProjects; }
            set { storedProjects = value; RaisePropertyChanged("StoredProjects"); }
        }

        private GeneralData general = new GeneralData();
        public GeneralData General
        {
            get { return general; }
            set { general = value; RaisePropertyChanged("General"); }
        }


        private void RaisePropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
