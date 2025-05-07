using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalon.Model;
using Avalonia.Controls;
using Avalonia.Media;
using Org.BouncyCastle.Bcpg.OpenPgp;

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

        private string[] projectColor = null;
        public string[] ProjectColor
        {
            get { return projectColor; }
            set { projectColor = value; OnPropertyChanged("ProjectColor"); }
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

        private IEnumerable<FileData> filteredFav;
        public IEnumerable<FileData> FilteredFav
        {
            get { return filteredFav; }
            set { filteredFav = value; OnPropertyChanged("FilteredFav"); }
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

        public void NewProject(string name, string group = null, string category = "Project")
        {
            if (!StoredProjects.Any(x => x.Namn == name))
            {
                ProjectData newProject = new ProjectData { Namn = name, Parent = group, Category = category, Colors = ProjectColor };
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
            if (CurrentProject.Category != "Search" && CurrentProject.Category != "Favorites")
            {
                StoredProjects.Remove(CurrentProject);
                SetProjectlist();
                SetDefaultSelection();
                SortProjects();
            }
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
            if (CurrentProject.Category != "Search" && CurrentProject.Category != "Favorites")
            {
                CurrentProject.Namn = projectName;

                foreach (FileData file in CurrentProject.StoredFiles)
                {
                    file.Uppdrag = projectName;
                }
                CurrentProject.SetFiletypeList();
            }
        }

        public ObservableCollection<string> GetGroups()
        {
            List<string> list = StoredProjects.Select(x => x.Parent).Where(x => x != null).Distinct().ToList();

            list.Remove("");

            return new ObservableCollection<string>(list);
        }

        public ObservableCollection<string> GetFavGroups()
        {
            ProjectData FavProject = StoredProjects.FirstOrDefault(x => x.Namn == "Favorites");

            List<string> favList = new List<string>();

            if (FavProject == null)
            {
                favList.Add("Default");
            }

            if (FavProject != null)
            {
                favList = FavProject.StoredFiles.Select(x => x.Uppdrag).Distinct().ToList();
            }

            return new ObservableCollection<string>(favList);       
        }

        public void SetGroups(string group)
        {
            CurrentProject.Parent = group;
        }

        public List<MenuItem> GetAllowedTypes()
        {

            if (CurrentProject.Category == "Project")
            {
                return new List<MenuItem>()
                {
                    new MenuItem(){Header="Drawing", Icon=new Label(){Content="○" } },
                    new MenuItem(){Header="Document", Icon=new Label(){Content="○" } },
                    new MenuItem(){Header="Other", Icon=new Label(){Content="○" } }
                };
            }

            if (CurrentProject.Category == "Library")
            {

                return new List<MenuItem>()
                {
                    new MenuItem(){Header="General", Icon=new Label(){Content="○" } },
                    new MenuItem(){Header="Loads", Icon=new Label(){Content="○" } },
                    new MenuItem(){Header="Concrete", Icon=new Label(){Content="○" } },
                    new MenuItem(){Header="Steel", Icon=new Label(){Content="○" } },
                    new MenuItem(){Header="Timber", Icon=new Label(){Content="○" } },
                    new MenuItem(){Header="FEM", Icon=new Label(){Content="○" } },
                    new MenuItem(){Header="Mechanics", Icon=new Label(){Content="○" } },
                    new MenuItem(){Header="Dynamics", Icon=new Label(){Content="○" } },
                    new MenuItem(){Header="Geotechnics", Icon=new Label(){Content="○" } },
                    new MenuItem(){Header="Other", Icon=new Label(){Content="○" } }
                };
            }

            if (CurrentProject.Category == "Archive")
            {
                return new List<MenuItem>()
                {
                    new MenuItem(){Header="Portal Frame", Icon=new Label(){Content="○" } },
                    new MenuItem(){Header="Slab", Icon=new Label(){Content="○" } },
                    new MenuItem(){Header="Beam", Icon=new Label(){Content="○" } },
                    new MenuItem(){Header="Composite", Icon=new Label(){Content="○" } },
                    new MenuItem(){Header="Concrete deck", Icon=new Label(){Content="○" } },
                    new MenuItem(){Header="Integral", Icon=new Label(){Content="○" } },
                    new MenuItem(){Header="Steel", Icon=new Label(){Content="○" } },
                    new MenuItem(){Header="Post tension", Icon=new Label(){Content="○" } },
                    new MenuItem(){Header="Substructure", Icon=new Label(){Content="○" } },
                    new MenuItem(){Header="Other", Icon=new Label(){Content="○" } }
                };
            }

            else
            {
                return new List<MenuItem>()
                {
                    new MenuItem(){Header="" }
                };
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
            if (currentProject.Category != "Search" && currentProject.Category != "Favorites")
            {
                CurrentProject.Category = name;

                if (name != "Project")
                {
                    CurrentProject.Parent = null;
                }
                
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
            string defaultProject = StoredProjects.Where(x => x.Category != "Search" && x.Category != "Favorites").FirstOrDefault().Namn;
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
            return StoredProjects.Where(x => x.Category != "Search").Where(x=>x.Category != "Favorites").FirstOrDefault();
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

        public void AddFavorite(string group)
        {
            ProjectData FavProject = StoredProjects.FirstOrDefault(x => x.Namn == "Favorites");

            if (FavProject == null)
            {
                FavProject = new ProjectData { Namn = "Favorites", Category = "Favorites" };
                FavProject.MetaCheckStore = MetaCheckDefault;
                StoredProjects.Add(FavProject);
                
                SortProjects();
            }

            foreach(FileData file in CurrentFiles)
            {
                FileData currentFav = new FileData() { Namn = file.Namn, Sökväg = file.Sökväg, Filtyp = file.Filtyp, Uppdrag = group };
                FavProject.AddFile(currentFav);
            }

            FavProject.SetFiletypeList();
        }

        public void RemoveFavorite()
        {
            ProjectData FavProject = StoredProjects.FirstOrDefault(x => x.Namn == "Favorites");
            foreach(FileData file in CurrentFiles)
            {
                FavProject.RemoveFile(CurrentFile);
            }

            FavProject.SetFiletypeList();
        }

        public void RemoveFavoriteGroup(string group)
        {
            ProjectData FavProject = StoredProjects.FirstOrDefault(x => x.Namn == "Favorites");

            List<FileData> FavFiles = FavProject.StoredFiles.Where(x => x.Uppdrag == group).ToList();

            foreach (FileData file in FavFiles)
            {
                FavProject.RemoveFile(file);
            }

            FavProject.SetFiletypeList();
        }

        public void RenameFavoriteGroup(string oldGroup, string newGroup)
        {
            ProjectData FavProject = StoredProjects.FirstOrDefault(x => x.Namn == "Favorites");
            List<FileData> FavFiles = FavProject.StoredFiles.Where(x => x.Uppdrag == oldGroup).ToList();

            foreach (FileData file in FavFiles)
            {
                file.Uppdrag = newGroup;
            }

            FavProject.SetFiletypeList();
        }

        public void FilterFavorite(string group)
        {
            ProjectData FavProject = StoredProjects.FirstOrDefault(x => x.Namn == "Favorites");

            if(FavProject != null)
            {
                FilteredFav = FavProject.StoredFiles.Where(x => x.Uppdrag == group);
            }
            
        }

        public void UpdateFavorite()
        {
            ProjectData FavProject = StoredProjects.FirstOrDefault(x => x.Namn == "Favorites");

            if (FavProject != null)
            {
                TrayFiles = FavProject.StoredFiles;
            }

            if (FavProject != null)
            {
                FavProject.SetFiletypeList();
            }
        }

        public void AddAppendedFile(string filepath)
        {
            if (CurrentFile != null && CurrentFile.AppendedFiles.Where(x=>x.Sökväg == filepath).Count() == 0)
            {
                CurrentFile.AppendedFiles.Add(new FileData()
                {
                    Namn = System.IO.Path.GetFileNameWithoutExtension(filepath),
                    Sökväg = filepath
                });

                List<FileData> tempList = CurrentFile.AppendedFiles.OrderBy(x => x.Namn).ToList();
                CurrentFile.AppendedFiles.Clear();
                CurrentFile.AppendedFiles = new ObservableCollection<FileData>(tempList);

            }
        }

        public void SetProjectColor(Color color1, Color color2, Color color3, Color color4, bool cornerRadiusVal, bool borderVal)
        {
            ProjectColor = [color1.ToString(), color2.ToString(), color3.ToString(), color4.ToString()];

            foreach (ProjectData project in StoredProjects)
            {
                project.Colors = ProjectColor;

                project.Borders = [cornerRadiusVal, borderVal];
            }


        }

        public void SeachFiles(string searchtext)
        {
            RemoveProjects(StoredProjects.Where(x => x.Category == "Search").ToList());

            NewProject(searchtext);
            SetProject(searchtext);
            SetProjecCategory("Search");

            ObservableCollection<FileData> results = new ObservableCollection<FileData>();

            foreach (ProjectData project in StoredProjects.Where(x => x.Category != "Search").Where(x=>x.Category != "Favorites"))
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
                    if (b4 != null && !finished) { if (b4.ToLower().Contains(searchtext.ToLower())) { results.Add(file); finished = true; } }
                }
            }

            results.DistinctBy(x => x.Sökväg);

            CurrentProject.StoredFiles.Clear();
            CurrentProject.StoredFiles = results;
            CurrentProject.SetFiletypeList();
            SetType("All Types");
            UpdateFilter();

            Meta_1 = true;
            Meta_2 = false;
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

        public void RenameOriginal(string newName)
        {
            string oldName = CurrentFile.Namn;
            string oldPath = CurrentFile.Sökväg;

            if (oldName != newName && newName.Length > 0 && CurrentFile.IsLocal)
            {
                string newPath = CurrentFile.Sökväg.Replace(oldName, newName);
                try
                {
                    System.IO.File.Move(oldPath, newPath);
                }
                catch
                {
                    return;
                }


                CurrentFile.Sökväg = newPath;
                CurrentFile.Namn = newName;

            }
        }


        private void SetupDefaultFolders()
        {

        }
    }
}
