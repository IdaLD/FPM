using Newtonsoft.Json.Bson;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPM.Model
{
    public class ProjectData : INotifyPropertyChanged
    {

        private string namn = string.Empty;
        public string Namn
        {
            get { return namn; }
            set { namn = value; RaisePropertyChanged("Namn"); }
        }

        private string notes = string.Empty;
        public string Notes
        {
            get { return notes; }
            set { notes = value; RaisePropertyChanged("Notes"); }
        }

        private List<string> type = new List<string>();
        public List<string> Type
        {
            get { return type; }
            set { type = value; RaisePropertyChanged("Type"); }
        }

        private string typefilter = string.Empty;
        public string Typefilter
        {
            get { return typefilter; }
            set { typefilter = value; RaisePropertyChanged("Typefilter"); FilterByType(); }
        }

        private List<string> filetypes = new List<string>();
        public List<string> Filetypes
        {
            get { return filetypes; }
            set { filetypes = value; RaisePropertyChanged("Filetypes"); }
        }

        public ObservableCollection<FileData> StoredFiles = new ObservableCollection<FileData>();

        private IEnumerable<FileData> filteredFiles = null;
        public IEnumerable<FileData> FilteredFiles
        {
            get { return filteredFiles; }
            set { filteredFiles = value; RaisePropertyChanged("FilteredFiles"); }
        }


        public void AddFile(FileData file)
        {
            StoredFiles.Add(file);
        }

        public void Newfile(string filepath)
        {
            if (!StoredFiles.Any(x => x.Sökväg == filepath))
            {
                StoredFiles.Add(new FileData
                {
                    Namn = System.IO.Path.GetFileNameWithoutExtension(filepath),
                    Filtyp = "New",
                    Sökväg = filepath
                });
                SetFiletypeList();
            }
        }

        public void RemoveFiles(IList<FileData> files)
        {
            foreach(FileData file in files)
            {
                StoredFiles.Remove(file);
            }
            SetFiletypeList();
            FilterByType();
        }

        public void RenameProject(string projectName)
        {
            Namn = projectName;
            SetFiletypeList();
        }

        public void SetType(IList<FileData> files, string type)
        {
            foreach(FileData file in files)
            {
                file.Filtyp = type;
            }
            SetFiletypeList();
        }

        public void FilterByType()
        {
            FilteredFiles = StoredFiles.Where(x => x.Filtyp == Typefilter);
        }


        public void SetFiletypeList()
        {
            Filetypes.Clear();

            List<string> filetypes = StoredFiles.Select(x=>x.Filtyp).Distinct().ToList();

            foreach (string filetype in filetypes)
            {
                Filetypes.Add(filetype);
            }
        }

        public void ClearMetadata(IList<FileData> files)
        {
            foreach (FileData file in files)
            {
                file.Handling = "";
                file.Status = "";
                file.Datum = "";
                file.Ritningstyp = "";
                file.Beskrivning1 = "";
                file.Beskrivning2 = "";
                file.Beskrivning3 = "";
                file.Beskrivning4 = "";
                file.Revidering = "";
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
