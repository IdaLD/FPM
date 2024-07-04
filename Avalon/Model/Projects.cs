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

        private ProjectData currentProject = null;
        public ProjectData CurrentProject
        {
            get { return currentProject; }
            set { currentProject = value; RaisePropertyChanged("CurrentProject"); UpdateFilter(); }
        }

        private string type = null;
        public string Type
        {
            get { return type; }
            set { type = value; RaisePropertyChanged("Type"); UpdateFilter(); }
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


        public void AddProject(ProjectData project)
        {
            StoredProjects.Add(project);
        }

        public void NewProject(string name)
        {
            if (!StoredProjects.Any(x => x.Namn == name))
            {
                CurrentProject = new ProjectData{Namn = name};
                StoredProjects.Add(CurrentProject);

                SetProjectlist();
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

        public void SetType(string type)
        {
            Type = type;
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
