using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Avalon.Model;
using Avalonia.Controls;
using Material.Icons;
using Material.Icons.Avalonia;
using Org.BouncyCastle.Asn1.BC;

namespace Avalon.ViewModels
{
    public class ProjectViewModel : ViewModelBase, INotifyPropertyChanged
    {
        public ProjectViewModel()
        {
            NewProject("New Project");

            CurrentProject = StoredProjects.FirstOrDefault();
            SetProjectlist();
            SetProject("New Project");
            SetType("All Types");

        }

        private ObservableCollection<ProjectData> storedProjects = new ObservableCollection<ProjectData>();
        public ObservableCollection<ProjectData> StoredProjects
        {
            get { return storedProjects; }
            set { storedProjects = value; OnPropertyChanged("StoredProjects"); }
        }

        private List<string> projectList = new List<string>();
        public List<string> ProjectList
        {
            get { return projectList; }
            set { projectList = value; OnPropertyChanged("ProjectList"); }
        }

        private ProjectData currentProject;
        public ProjectData CurrentProject
        {
            get { return currentProject; }
            set { currentProject = value; OnPropertyChanged("CurrentProject"); UpdateFilter(); UpdateMetaCheck(); }
        }

        private string type = null;
        public string Type
        {
            get { return type; }
            set { type = value; OnPropertyChanged("Type"); UpdateFilter(); }
        }

        private ObservableCollection<FileData> filteredFiles = new ObservableCollection<FileData>();
        public ObservableCollection<FileData> FilteredFiles
        {
            get { return filteredFiles; }
            set { filteredFiles = value; OnPropertyChanged("FilteredFiles"); }
        }

        private ObservableCollection<FileData> trayFiles = new ObservableCollection<FileData>();
        public ObservableCollection<FileData> TrayFiles
        {
            get { return trayFiles; }
            set { trayFiles = value; OnPropertyChanged("TrayFiles"); }
        }

        public int NrFilteredFiles
        {
            get
            {
                if (FilteredFiles == null) { return 0; }
                else { return FilteredFiles.Count(); }
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
            set
            {
                currentFiles = value;
                OnPropertyChanged("FiletypesTree");
                OnPropertyChanged("CurrentFiles");
                OnPropertyChanged("CurrentFile");
                OnPropertyChanged("NrSelectedFiles");
            }
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
            set { CurrentProject.MetaCheckStore[0] = value; OnPropertyChanged("Meta_1"); }
        }
        public bool Meta_2
        {
            get { return FetchMetaCheck(1); }
            set { CurrentProject.MetaCheckStore[1] = value; OnPropertyChanged("Meta_2"); }
        }
        public bool Meta_3
        {
            get { return FetchMetaCheck(2); }
            set { CurrentProject.MetaCheckStore[2] = value; OnPropertyChanged("Meta_3"); }
        }
        public bool Meta_4
        {
            get { return FetchMetaCheck(3); }
            set { CurrentProject.MetaCheckStore[3] = value; OnPropertyChanged("Meta_4"); }
        }
        public bool Meta_5
        {
            get { return FetchMetaCheck(4); }
            set { CurrentProject.MetaCheckStore[4] = value; OnPropertyChanged("Meta_5"); }
        }
        public bool Meta_6
        {
            get { return FetchMetaCheck(5); }
            set { CurrentProject.MetaCheckStore[5] = value; OnPropertyChanged("Meta_6"); }
        }
        public bool Meta_7
        {
            get { return FetchMetaCheck(6); }
            set { CurrentProject.MetaCheckStore[6] = value; OnPropertyChanged("Meta_7"); }
        }
        public bool Meta_8
        {
            get { return FetchMetaCheck(7); }
            set { CurrentProject.MetaCheckStore[7] = value; OnPropertyChanged("Meta_8"); }
        }
        public bool Meta_9
        {
            get { return FetchMetaCheck(8); }
            set { CurrentProject.MetaCheckStore[8] = value; OnPropertyChanged("Meta_9"); }
        }
        public bool Meta_10
        {
            get { return FetchMetaCheck(9); }
            set { CurrentProject.MetaCheckStore[9] = value; OnPropertyChanged("Meta_10"); }
        }
        public bool Meta_11
        {
            get { return FetchMetaCheck(10); }
            set { CurrentProject.MetaCheckStore[10] = value; OnPropertyChanged("Meta_11"); }
        }
        public bool Meta_12
        {
            get { return FetchMetaCheck(11); }
            set { CurrentProject.MetaCheckStore[11] = value; OnPropertyChanged("Meta_12"); }
        }
        public bool Meta_13
        {
            get { return FetchMetaCheck(12); }
            set { CurrentProject.MetaCheckStore[12] = value; OnPropertyChanged("Meta_13"); }
        }
        public bool Meta_14
        {
            get { return FetchMetaCheck(13); }
            set { CurrentProject.MetaCheckStore[13] = value; OnPropertyChanged("Meta_14"); }
        }
        public bool Meta_15
        {
            get { return FetchMetaCheck(14); }
            set { CurrentProject.MetaCheckStore[14] = value; OnPropertyChanged("Meta_15"); }
        }
        public bool Meta_16
        {
            get { return FetchMetaCheck(15); }
            set { CurrentProject.MetaCheckStore[15] = value; OnPropertyChanged("Meta_16"); }
        }

        public bool[] MetaCheckDefault = { true, true, true, true, true, false, false, false, true, true, true, false, false, false, false, false };

        public void NewProject(string name)
        {
            if (!StoredProjects.Any(x => x.Namn == name))
            {
                ProjectData newProject = new ProjectData { Namn = name };
                newProject.MetaCheckStore = MetaCheckDefault;

                StoredProjects.Add(newProject);
                CurrentProject = newProject;

                SetProjectlist();
                SetDefaultType();
                SortProjects();
            }
        }

        public void RemoveProject()
        {
            StoredProjects.Remove(CurrentProject);
            SetProjectlist();
            SetDefaultSelection();
            SortProjects();
        }

        public void RemoveProjects(List<ProjectData> list)
        {
            foreach (ProjectData project in list)
            {
                StoredProjects.Remove(project);
            }

            SetProjectlist();
            SetDefaultSelection();
            SortProjects();
        }

        public void RenameProject(string projectName)
        {
            CurrentProject.Namn = projectName;

            if (CurrentProject.Category != "Search")
            {
                foreach (FileData file in CurrentProject.StoredFiles)
                {
                    file.Uppdrag = projectName;
                }
                CurrentProject.SetFiletypeList();
            }
        }

        public List<MenuItem> GetAllowedTypes()
        {
            if (CurrentProject.Category == "Project")
            {
                return new List<MenuItem>() 
                {
                    new MenuItem(){Header="Drawing", Icon=new MaterialIcon(){Kind=MaterialIconKind.FilePdfBox } },
                    new MenuItem(){Header="Document", Icon=new MaterialIcon(){Kind=MaterialIconKind.FileDocument } },
                    new MenuItem(){Header="Other", Icon=new MaterialIcon(){Kind=MaterialIconKind.FileQuestion } }
                };
            }
            if (CurrentProject.Category == "Library")
            {

                return new List<MenuItem>()
                {
                    new MenuItem(){Header="General", Icon=new MaterialIcon(){Kind=MaterialIconKind.AlphaLBoxOutline } },
                    new MenuItem(){Header="Loads", Icon=new MaterialIcon(){Kind=MaterialIconKind.AlphaLBoxOutline } },
                    new MenuItem(){Header="Concrete", Icon=new MaterialIcon(){Kind=MaterialIconKind.AlphaCBoxOutline } },
                    new MenuItem(){Header="Steel", Icon=new MaterialIcon(){Kind=MaterialIconKind.AlphaSBoxOutline } },
                    new MenuItem(){Header="Timber", Icon=new MaterialIcon(){Kind=MaterialIconKind.AlphaTBoxOutline } },
                    new MenuItem(){Header="FEM", Icon=new MaterialIcon(){Kind=MaterialIconKind.AlphaFBoxOutline } },
                    new MenuItem(){Header="Mechanics", Icon=new MaterialIcon(){Kind=MaterialIconKind.AlphaMBoxOutline } },
                    new MenuItem(){Header="Dynamics", Icon=new MaterialIcon(){Kind=MaterialIconKind.AlphaDBoxOutline } },
                    new MenuItem(){Header="Geotechnics", Icon=new MaterialIcon(){Kind=MaterialIconKind.AlphaGBoxOutline } },
                    new MenuItem(){Header="Other", Icon=new MaterialIcon(){Kind=MaterialIconKind.AlphaGBoxOutline } },
                };
            }

            if (CurrentProject.Category == "Archive")
            {
                return new List<MenuItem>()
                {
                    new MenuItem(){Header="Portal Frame", Icon=new MaterialIcon(){Kind=MaterialIconKind.AlphaPBoxOutline } },
                    new MenuItem(){Header="Slab", Icon=new MaterialIcon(){Kind=MaterialIconKind.AlphaSBoxOutline } },
                    new MenuItem(){Header="Beam", Icon=new MaterialIcon(){Kind=MaterialIconKind.AlphaBBoxOutline } },
                    new MenuItem(){Header="Composite", Icon=new MaterialIcon(){Kind=MaterialIconKind.AlphaCBoxOutline } },
                    new MenuItem(){Header="Concrete deck", Icon=new MaterialIcon(){Kind=MaterialIconKind.AlphaCBoxOutline } },
                    new MenuItem(){Header="Integral", Icon=new MaterialIcon(){Kind=MaterialIconKind.AlphaIBoxOutline } },
                    new MenuItem(){Header="Steel", Icon=new MaterialIcon(){Kind=MaterialIconKind.AlphaSBoxOutline } },
                    new MenuItem(){Header="Post tension", Icon=new MaterialIcon(){Kind=MaterialIconKind.AlphaSBoxOutline } },
                    new MenuItem(){Header="Substructure", Icon=new MaterialIcon(){Kind=MaterialIconKind.AlphaSBoxOutline } },
                    new MenuItem(){Header="Other", Icon=new MaterialIcon(){Kind=MaterialIconKind.AlphaLBoxOutline } }
                };
            }

            else
            {
                return null;
            }
        }

        public void SortProjects()
        {
            List<ProjectData> search = StoredProjects.Where(x => x.Category == "Search").ToList();
            List<ProjectData> favorites = StoredProjects.Where(x => x.Category == "Favorites").ToList();
            List<ProjectData> sortedLibrary = StoredProjects.Where(x => x.Category == "Library").OrderBy(x => x.Namn).ToList();
            List<ProjectData> sortedArchive = StoredProjects.Where(x => x.Category == "Archive").OrderBy(x => x.Namn).ToList();
            List<ProjectData> sortedProject = StoredProjects.Where(x => x.Category == "Project").OrderBy(x => x.Namn).ToList();

            StoredProjects.Clear();

            foreach (var project in search) { StoredProjects.Add(project); }
            foreach (var project in favorites) { StoredProjects.Add(project); }
            foreach (var project in sortedLibrary) { StoredProjects.Add(project); }
            foreach (var project in sortedArchive) { StoredProjects.Add(project); }
            foreach (var project in sortedProject) { StoredProjects.Add(project); }

            SetProjectlist();
        }

        public void AddSelectedToTray()
        {
            foreach(FileData file in CurrentFiles)
            {
                TrayFiles.Add(file);
            }
        }

        public void UpdateCurrentPagenr(int nr)
        {
            CurrentFile.DefaultPage = nr;
        }

        public void RemoveSelectedFiles()
        {
            foreach (FileData file in CurrentFiles)
            {
                CurrentProject.RemoveFile(file);
            }

            CurrentProject.SetFiletypeList();

            if (FilteredFiles == null)
            {
                SetDefaultSelection();
            }
        }

        public void SetProject(string name)
        {
            ProjectData project = StoredProjects.FirstOrDefault(x => x.Namn == name);

            SelectProjectAsync(project);

            if (!CurrentProject.Filetypes.Contains(Type))
            {
                Type = "All Types";
            }
        }

        public async Task SelectProjectAsync(ProjectData project)
        {
            CurrentProject = project;
        }

        public void SetProjecCategory(string name)
        {
            if (currentProject.Category != "Search")
            {
                CurrentProject.Category = name;
                SortProjects();
            }
        }

        public bool FetchMetaCheck(int i)
        {
            if (i >= CurrentProject.MetaCheckStore.Length)
            {
                return MetaCheckDefault[i];
            }
            else
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

        }

        public void UpdateMetaCheck()
        {
            OnPropertyChanged("Meta_1");
            OnPropertyChanged("Meta_2");
            OnPropertyChanged("Meta_3");
            OnPropertyChanged("Meta_4");
            OnPropertyChanged("Meta_5");
            OnPropertyChanged("Meta_6");
            OnPropertyChanged("Meta_7");
            OnPropertyChanged("Meta_8");
            OnPropertyChanged("Meta_9");
            OnPropertyChanged("Meta_10");
            OnPropertyChanged("Meta_11");
            OnPropertyChanged("Meta_12");
            OnPropertyChanged("Meta_13");
            OnPropertyChanged("Meta_14");
            OnPropertyChanged("Meta_15");
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
            foreach (FileData file in CurrentFiles)
            {
                file.Filtyp = type;
            }
            currentProject.SetFiletypeList();
            UpdateFilter();
        }

        public void SetDefaultSelection()
        {
            string defaultProject = StoredProjects.Where(x => x.Category != "Search").FirstOrDefault().Namn;
            CurrentProject = GetProject(defaultProject);
            Type = "All Types";
        }

        public void UpdateFilter()
        {
            FilteredFiles.Clear();

            if (Type != "All Types")
            {
                foreach (FileData file in CurrentProject.StoredFiles.Where(x => x.Filtyp == Type).OrderBy(x => x.Namn))
                {
                    FilteredFiles.Add(file);
                }
            }

            else
            {
                foreach (FileData file in CurrentProject.StoredFiles.OrderBy(x => x.Namn).OrderByDescending(x => x.Filtyp))
                {
                    FilteredFiles.Add(file);
                }
            }
            OnPropertyChanged("NrFilteredFiles");
        }


        public ProjectData GetProject(string name)
        {
            return StoredProjects.FirstOrDefault(x => x.Namn == name);
        }

        public void SetProjectlist()
        {
            ProjectList.Clear();

            List<string> newList = StoredProjects.Select(x => x.Namn).Distinct().ToList();

            foreach (string item in newList)
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

        public void AddFavorite()
        {
            ProjectData FavProject = StoredProjects.FirstOrDefault(x => x.Namn == "Favorites");

            if (FavProject == null)
            {
                FavProject = new ProjectData { Namn = "Favorites", Category = "Favorites" };
                FavProject.MetaCheckStore = MetaCheckDefault;
                StoredProjects.Add(FavProject);
                SortProjects();
            }

            FavProject.AddFiles(CurrentFiles);
        }

        public void RemoveFavorite()
        {
            ProjectData FavProject = StoredProjects.FirstOrDefault(x => x.Namn == "Favorites");
            FavProject.RemoveFile(CurrentFile);
            UpdateFilter();
        }

        public void UpdateFavorite()
        {
            TrayFiles = StoredProjects.FirstOrDefault(x => x.Namn == "Favorites").StoredFiles;
        }

        public void SeachFiles(string searchtext)
        {
            RemoveProjects(StoredProjects.Where(x => x.Category == "Search").ToList());

            NewProject(searchtext);
            SetProject(searchtext);
            SetProjecCategory("Search");

            ObservableCollection<FileData> results = new ObservableCollection<FileData>();

            foreach (ProjectData project in StoredProjects.Where(x => x.Category != "Search"))
            {
                foreach (FileData file in project.StoredFiles)
                {
                    bool finished = false;

                    string b0 = file.Namn;
                    string b1 = file.Beskrivning1;
                    string b2 = file.Beskrivning2;
                    string b3 = file.Beskrivning3;
                    string b4 = file.Tagg;

                    if (b0 != null && !finished) { if (b0.ToLower().Contains(searchtext.ToLower())) { results.Add(file); finished = true; } }
                    if (b1 != null && !finished) { if (b1.ToLower().Contains(searchtext.ToLower())) { results.Add(file); finished = true; } }
                    if (b2 != null && !finished) { if (b2.ToLower().Contains(searchtext.ToLower())) { results.Add(file); finished = true; } }
                    if (b3 != null && !finished) { if (b3.ToLower().Contains(searchtext.ToLower())) { results.Add(file); finished = true; } }
                    if (b3 != null && !finished) { if (b4.ToLower().Contains(searchtext.ToLower())) { results.Add(file); finished = true; } }
                }
            }

            results.DistinctBy(x => x.Sökväg);

            CurrentProject.StoredFiles.Clear();
            CurrentProject.StoredFiles = results;
            CurrentProject.SetFiletypeList();
            SetType("All Types");
            UpdateFilter();

            Meta_1 = true;
            Meta_2 = true;
            Meta_3 = true;
            Meta_4 = true;
            Meta_5 = false;
            Meta_6 = false;
            Meta_7 = false;
            Meta_8 = false;
            Meta_9 = false;
            Meta_10 = true;
            Meta_11 = true;
            Meta_12 = true;
            Meta_13 = false;
            Meta_14 = false;
            Meta_15 = false;

        }
    }
}
