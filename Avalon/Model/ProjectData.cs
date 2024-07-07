using Newtonsoft.Json.Bson;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Avalon.Model
{
    public class ProjectData : INotifyPropertyChanged
    {
        private ObservableCollection<FileData> storedFiles = new ObservableCollection<FileData>();
        public ObservableCollection<FileData> StoredFiles
        {
            get { return storedFiles; }
            set { storedFiles = value; RaisePropertyChanged("StoredFiles"); }
        }

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

        private List<string> filetypes = new List<string>();
        public List<string> Filetypes
        {
            get { return filetypes; }
            set { filetypes = value; RaisePropertyChanged("Filetypes"); }
        }

        private ObservableCollection<string> filetypesTree = new ObservableCollection<string>();
        public ObservableCollection<string> FiletypesTree
        {
            get { return filetypesTree; }
            set { filetypesTree = value; RaisePropertyChanged("FiletypesTree"); }
        }

        public bool[] MetaCheckStore = new bool[15];

        public void AddFiles(IList<FileData> files)
        {
            foreach(FileData file in files)
            {
                StoredFiles.Add(file);
            }
            SetFiletypeList();
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

        public void RenameProject(string projectName)
        {
            Namn = projectName;
            SetFiletypeList();
        }

        public void SetFiletypeList()
        {
            Filetypes.Clear();
            Filetypes.Add("All Types");

            List<string> filetypes = StoredFiles.Select(x=>x.Filtyp).Distinct().ToList();

            foreach (string filetype in filetypes)
            {
                Filetypes.Add(filetype);
            }

            SetFiletypeTree();
        }

        public void SetFiletypeTree()
        {
            FiletypesTree.Clear();
            foreach (string item in Filetypes)
            {
                if (item != "All Types")
                {
                    FiletypesTree.Add(item + "\t\t\t\t\t\t\t\t\t" + Namn);
                }
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
