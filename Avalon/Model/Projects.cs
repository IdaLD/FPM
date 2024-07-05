using Avalonia.Collections;
using Newtonsoft.Json.Bson;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPM.Model
{
    public class Projects : INotifyPropertyChanged
    {
        public ObservableCollection<ProjectData> StoredProjects = new ObservableCollection<ProjectData>();

        private List<string> projectList = new List<string>();
        public List<string> ProjectList
        {
            get { return projectList; }
            set { projectList = value; RaisePropertyChanged("ProjectList"); }
        }

        private ProjectData currentProject = new ProjectData(){ Namn = "New Project"};
        public ProjectData CurrentProject
        {
            get { return currentProject; }
            set { currentProject = value; RaisePropertyChanged("CurrentProject"); UpdateFilter(); UpdateMetaCheck(); }
        }

        private string type = null;
        public string Type
        {
            get { return type; }
            set { type = value; RaisePropertyChanged("Type"); UpdateFilter(); Debug.WriteLine("TypeSetting"); }
        }

        private IEnumerable<FileData> filteredFiles = null;
        public IEnumerable<FileData> FilteredFiles
        {
            get { return filteredFiles; }
            set { filteredFiles = value; RaisePropertyChanged("FilteredFiles"); RaisePropertyChanged("NrFilteredFiles"); }
        }
        public int NrFilteredFiles
        {
            get {
                if (FilteredFiles == null) { return 0; }
                else { return FilteredFiles.Count();}
                }
        }

        public int NrSelectedFiles
        {
            get
            {
                if (CurrentFiles == null) { return 0; }
                else { return CurrentFiles.Count(); }
            }
        }

        private IList<FileData> currentFiles = null;
        public IList<FileData> CurrentFiles
        {
            get { return currentFiles; }
            set { currentFiles = value; RaisePropertyChanged("CurrentFiles"); RaisePropertyChanged("CurrentFile"); RaisePropertyChanged("NrSelectedFiles"); }
        }

        private FileData currentFile = null;
        public FileData CurrentFile
        {
            get
            {
                if (CurrentFiles != null)
                {
                    return CurrentFiles.LastOrDefault();
                }
                else
                {
                    return null;
                }
            }
        }

        public bool Meta_1
        {
            get { return FetchMetaCheck(0); }
            set { CurrentProject.MetaCheckStore[0] = value; RaisePropertyChanged("Meta_1"); }
        }
        public bool Meta_2
        {
            get { return FetchMetaCheck(1); }
            set { CurrentProject.MetaCheckStore[1] = value; RaisePropertyChanged("Meta_2"); }
        }
        public bool Meta_3
        {
            get { return FetchMetaCheck(2); }
            set { CurrentProject.MetaCheckStore[2] = value; RaisePropertyChanged("Meta_3"); }
        }
        public bool Meta_4
        {
            get { return FetchMetaCheck(3); }
            set { CurrentProject.MetaCheckStore[3] = value; RaisePropertyChanged("Meta_4"); }
        }
        public bool Meta_5
        {
            get { return FetchMetaCheck(4); }
            set { CurrentProject.MetaCheckStore[4] = value; RaisePropertyChanged("Meta_5"); }
        }
        public bool Meta_6
        {
            get { return FetchMetaCheck(5); }
            set { CurrentProject.MetaCheckStore[5] = value; RaisePropertyChanged("Meta_6"); }
        }
        public bool Meta_7
        {
            get { return FetchMetaCheck(6); }
            set { CurrentProject.MetaCheckStore[6] = value; RaisePropertyChanged("Meta_7"); }
        }
        public bool Meta_8
        {
            get { return FetchMetaCheck(7); }
            set { CurrentProject.MetaCheckStore[7] = value; RaisePropertyChanged("Meta_8"); }
        }
        public bool Meta_9
        {
            get { return FetchMetaCheck(8); }
            set { CurrentProject.MetaCheckStore[8] = value; RaisePropertyChanged("Meta_9"); }
        }
        public bool Meta_10
        {
            get { return FetchMetaCheck(9); }
            set { CurrentProject.MetaCheckStore[9] = value; RaisePropertyChanged("Meta_10"); }
        }
        public bool Meta_11
        {
            get { return FetchMetaCheck(10); }
            set { CurrentProject.MetaCheckStore[10] = value; RaisePropertyChanged("Meta_11"); }
        }
        public bool Meta_12
        {
            get { return FetchMetaCheck(11); }
            set { CurrentProject.MetaCheckStore[11] = value; RaisePropertyChanged("Meta_12"); }
        }
        public bool Meta_13
        {
            get { return FetchMetaCheck(12); }
            set { CurrentProject.MetaCheckStore[12] = value; RaisePropertyChanged("Meta_13"); }
        }
        public bool Meta_14
        {
            get { return FetchMetaCheck(13); }
            set { CurrentProject.MetaCheckStore[13] = value; RaisePropertyChanged("Meta_14"); }
        }
        public bool Meta_15
        {
            get { return FetchMetaCheck(14); }
            set { CurrentProject.MetaCheckStore[14] = value; RaisePropertyChanged("Meta_15"); }
        }

        public bool[] MetaCheckDefault = { true, true, true, true, false, false, false, true, true, true, false, false, false, false, false };

        public void NewProject(string name)
        {
            if (!StoredProjects.Any(x => x.Namn == name))
            {
                ProjectData newProject = new ProjectData{Namn = name};
                newProject.MetaCheckStore = MetaCheckDefault;

                StoredProjects.Add(newProject);
                CurrentProject = newProject;

                SetProjectlist();
                SetDefaultType();
            }
        }

        public void RemoveProject()
        {
            StoredProjects.Remove(CurrentProject);
            SetProjectlist();
            SetDefaultSelection();
        }

        public void RemoveSelectedFiles()
        {
            foreach (FileData file in CurrentFiles)
            {
                CurrentProject.StoredFiles.Remove(file);
            }
            
            CurrentProject.SetFiletypeList();

            if (FilteredFiles != null)
            {
                SetDefaultSelection();
            }
        }

        public void SetProject(string name)
        {
            CurrentProject = StoredProjects.FirstOrDefault(x => x.Namn == name);
        }

        public bool FetchMetaCheck(int i)
        {
            if (CurrentProject.MetaCheckStore[i] != null) 
            {
                return CurrentProject.MetaCheckStore[i];
            }

            else
            {
                return MetaCheckDefault[i];
            }
        }

        public void UpdateMetaCheck()
        {
            RaisePropertyChanged("Meta_1");
            RaisePropertyChanged("Meta_2");
            RaisePropertyChanged("Meta_3");
            RaisePropertyChanged("Meta_4");
            RaisePropertyChanged("Meta_5");
            RaisePropertyChanged("Meta_6");
            RaisePropertyChanged("Meta_7");
            RaisePropertyChanged("Meta_8");
            RaisePropertyChanged("Meta_9");
            RaisePropertyChanged("Meta_10");
            RaisePropertyChanged("Meta_11");
            RaisePropertyChanged("Meta_12");
            RaisePropertyChanged("Meta_13");
            RaisePropertyChanged("Meta_14");
        }

        public bool[] GetMetaCheckState()
        {
            return currentProject.MetaCheckStore;
        }

        public void SetAllMetaCheckState()
        {
            bool[] checkstate = GetMetaCheckState();

            foreach (ProjectData project in StoredProjects)
            {
                project.MetaCheckStore = checkstate;
            }
        }

        public void SetType(string type)
        {
            Type = type;
        }

        public void SetDefaultType()
        {
            Type = "All Types";
        }

        public void SetTypeSelected(string type)
        {
            foreach(FileData file in CurrentFiles)
            {
                file.Filtyp = type;
            }
            currentProject.SetFiletypeList();
            UpdateFilter();
        }

        public void SetDefaultSelection()
        {
            CurrentProject = GetProject(ProjectList.FirstOrDefault());
            Type = "All Types";
        }

        public void UpdateFilter()
        {
            if (Type != "All Types")
            {
                FilteredFiles = CurrentProject.StoredFiles.Where(x => x.Filtyp == Type);
            }
            else
            {
                FilteredFiles = CurrentProject.StoredFiles;
            }
        }

        public ProjectData GetProject(string name)
        {
            return StoredProjects.FirstOrDefault(x => x.Namn == name);
        }

        public void SetProjectlist()
        {
            ProjectList.Clear();

            List<string> newList = StoredProjects.Select(x => x.Namn).Distinct().ToList();

            newList.Sort();

            foreach(string item in newList)
            {
                ProjectList.Add(item);
            }
        }

        public ProjectData GetDefaultProject()
        {
            return StoredProjects.FirstOrDefault();
        }

        public void TransferFiles(string toProjectName)
        {
            ProjectData toProject = GetProject(toProjectName);

            if (toProject == null)
            {
                NewProject(toProjectName);
                toProject = GetProject(toProjectName);
            }

            toProject.AddFiles(CurrentFiles);

            RemoveSelectedFiles();

            toProject.SetFiletypeList();
            CurrentProject.SetFiletypeList();
        }

        public void ClearSelectedMetadata()
        {
            foreach (FileData file in CurrentFiles)
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
