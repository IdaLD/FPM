using Avalonia.Controls;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing.Printing;
using System.Linq;

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

        private string category = "Project";
        public string Category
        {
            get { return category; }
            set { category = value; RaisePropertyChanged("Category"); }
        }

        private string notes = string.Empty;
        public string Notes
        {
            get { return notes; }
            set { notes = value; RaisePropertyChanged("Notes"); }
        }

        private string[] colors;

        public string[] Colors
        {
            get { return colors; }
            set { colors = value; RaisePropertyChanged("Colors"); }
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
                if (!StoredFiles.Contains(file))
                {
                    StoredFiles.Add(file);
                }
            }
            SetFiletypeList();
        }

        public void AddFile(FileData file)
        {
            if (!StoredFiles.Contains(file))
            {
                StoredFiles.Add(file);
            }
            SetFiletypeList();
        }

        public void Newfile(string filepath, string type="New")
        {
            if (!StoredFiles.Any(x => x.Sökväg == filepath))
            {
                StoredFiles.Add(new FileData
                {
                    Namn = System.IO.Path.GetFileNameWithoutExtension(filepath),
                    Filtyp = type,
                    Uppdrag = Namn,
                    Sökväg = filepath
                });
                SetFiletypeList();
            }
        }

        public void RemoveFile(FileData file)
        {
            StoredFiles.Remove(file);
        }

        public void SetFiletypeList()
        {
            Filetypes.Clear();
            FiletypesTree.Clear();

            List<string> filetypes = StoredFiles.Select(x=>x.Filtyp).Distinct().ToList();

            filetypes.Sort();

            foreach (string filetype in filetypes)
            {
                Filetypes.Add(filetype);

                int nrFiles = StoredFiles.Where(x => x.Filtyp == filetype).Count();
                FiletypesTree.Add(filetype + "\t" + "(" + nrFiles + ")" + "\t\t\t\t\t\t\t\t\t" + Namn);
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
