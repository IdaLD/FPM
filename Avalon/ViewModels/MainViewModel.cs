using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System;
using System.ComponentModel;
using Avalon.Model;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using Avalon.Dialog;
using Avalonia.Interactivity;
using Avalon.Views;
using Avalonia;
using Org.BouncyCastle.Asn1.BC;


namespace Avalon.ViewModels
{
    public class MainViewModel : ViewModelBase, INotifyPropertyChanged
    {
        public MainViewModel() { }


        private ProjectViewModel projectsVM = new ProjectViewModel();
        public ProjectViewModel ProjectsVM
        {
            get { return projectsVM; }
            set { projectsVM = value; OnPropertyChanged("ProjectsVM"); }
        }

        private PreviewViewModel previewVM = new PreviewViewModel();
        public PreviewViewModel PreviewVM
        {
            get { return previewVM; }
            set { previewVM = value; OnPropertyChanged("PreviewVM"); }
        }

        public List<string[]> metastore = new List<string[]>();
        public List<string> PathStore = new List<string>();

        private ObservableCollection<string> favorites = new ObservableCollection<string>() { "Default" };
        public ObservableCollection<string> Favorites
        {
            get { return favorites; }
            set { favorites = value; OnPropertyChanged("Favorites"); }
        }

        private bool previewWindowOpen = false;
        public bool PreviewWindowOpen
        {
            get { return previewWindowOpen; }
            set { previewWindowOpen = value; OnPropertyChanged("PreviewWindowOpen"); }
        }


        public Window PreviewWindow;

        private string currentFavorite = string.Empty;
        public string CurrentFavorite
        {
            get { return currentFavorite; }
            set { currentFavorite = value; OnPropertyChanged("CurrentFavorite"); ProjectsVM.FilterFavorite(currentFavorite); }
        }

        private ObservableCollection<string> groups = new ObservableCollection<string>() {};
        public ObservableCollection<string> Groups
        {
            get { return groups; }
            set { groups = value; OnPropertyChanged("groups"); }
        }

        private PageData favPage;
        public PageData FavPage
        {
            get { return favPage; }
            set { favPage = value; OnPropertyChanged("FavPage"); TrySetPage(); }
        }

        public string ProjectMessage { get; set; } = "";

        private Color color1 = Color.Parse("#333333");
        public Color Color1
        {
            get { return color1; }
            set { color1 = value; OnPropertyChanged("Color1"); ColorChanged(); }
        }

        private Color color2 = Color.Parse("#444444");
        public Color Color2
        {
            get { return color2; }
            set { color2 = value; OnPropertyChanged("Color2"); ColorChanged(); }
        }

        private Color color3 = Color.Parse("#dfe6e9");
        public Color Color3
        {
            get { return color3; }
            set { color3 = value; OnPropertyChanged("Color3"); ColorChanged(); }
        }

        private Color color4 = Color.Parse("#999999");
        public Color Color4
        {
            get { return color4; }
            set { color4 = value; OnPropertyChanged("Color4"); ColorChanged(); }
        }


        private bool cornerRadiusVal = true;
        public bool CornerRadiusVal
        {
            get { return cornerRadiusVal; }
            set { cornerRadiusVal = value; OnPropertyChanged("CornerRadiusVal"); SetCornerRadius(); }
        }

        private CornerRadius cornerRadius = new CornerRadius(10);
        public CornerRadius CornerRadius
        {
            get { return cornerRadius; }
            set { cornerRadius = value; OnPropertyChanged("CornerRadius"); }
        }

        private bool borderVal = false;
        public bool BorderVal
        {
            get { return borderVal; }
            set { borderVal = value; OnPropertyChanged("BorderVal"); SetBorder(); }
        }

        private Thickness border = new Thickness(0);
        public Thickness Border
        {
            get { return border; }
            set { border = value; OnPropertyChanged("Border"); }
        }


        public bool Confirmed = false;

        public List<string> FileTypes { get; set; } = new List<string>();

        private List<MenuItem> fileTypeSelection = new List<MenuItem>()
        {
            new MenuItem() { Header = "Drawing", Icon = new Label() { Content = "○" } },
            new MenuItem() { Header = "Document", Icon = new Label() { Content = "○" } },
            new MenuItem() { Header = "Other", Icon = new Label() { Content = "○" } }
        };

        public List<MenuItem> FileTypeSelection
        {
            get { return fileTypeSelection; }
            set { fileTypeSelection = value; OnPropertyChanged("FileTypeSelection"); }
        }

        public void OpenPreviewWindow(ThemeVariant theme)
        {
            if (PreviewWindowOpen == true)
            {
                return;
            }

            PreviewWindow = new PreWindow()
            {
                DataContext = this
            };

            PreviewWindow.RequestedThemeVariant = theme;

            PreviewWindow.AddHandler(Window.WindowClosedEvent, PreviewWindowClosed);

            PreviewWindow.Show();

            PreviewWindowOpen = true;

        }

        public void PreviewWindowClosed(object sender, RoutedEventArgs e)
        {
            PreviewWindowOpen = false;
        }

        public void OpenInfoDia(Window mainWindow)
        {
            var window = new xProgDia()
            {
                DataContext = this
            };
            window.RequestedThemeVariant = mainWindow.ActualThemeVariant;
            window.ShowDialog(mainWindow);
        }


        public void OpenColorDia(Window mainWindow)
        {
            var window = new xColorDia()
            {
                DataContext = this
            };
            window.RequestedThemeVariant = mainWindow.ActualThemeVariant;
            window.ShowDialog(mainWindow);
        }

        public void OpenMetaEditDia(Window mainWindow)
        {
            var window = new xMetaDia()
            {
                DataContext = this
            };

            window.RequestedThemeVariant = mainWindow.ActualThemeVariant;
            window.ShowDialog(mainWindow);
        }

        public void OpenProjectEditDia(Window mainWindow)
        {
            var window = new xEditDia()
            {
                DataContext = this
            };

            window.RequestedThemeVariant = mainWindow.ActualThemeVariant;
            window.ShowDialog(mainWindow);
        }

        public void OpenProjectNewDia(Window mainWindow)
        {
            var window = new xNewDia()
            {
                DataContext = this
            };

            window.RequestedThemeVariant = mainWindow.ActualThemeVariant;
            window.ShowDialog(mainWindow);
            window.ProjectName.Focus();
        }

        public void OpenTagDia(Window mainWindow)
        {
            var window = new xTagDia()
            {
                DataContext = this
            };

            window.TagMenuInput.Text = ProjectsVM.CurrentFile.Tagg;
            window.TagMenuInput.CaretIndex = window.TagMenuInput.Text.Length;

            window.RequestedThemeVariant = mainWindow.ActualThemeVariant;
            window.ShowDialog(mainWindow);
            window.TagMenuInput.Focus();
        }

        public async Task ConfirmDeleteDia(Window mainWindow)
        {
            var window = new xDeleteDia()
            {
                DataContext = this
            };

            window.RequestedThemeVariant = mainWindow.ActualThemeVariant;
            await window.ShowDialog(mainWindow);
        }

        public void OnInfoDia(Window mainWindow)
        {
            var window = new xInfoDia()
            {
                DataContext = this
            };

            window.RequestedThemeVariant = mainWindow.ActualThemeVariant;
            window.ShowDialog(mainWindow);
        }

        private void SetCornerRadius()
        {
            if (CornerRadiusVal)
            {
                CornerRadius = new CornerRadius(10);
            }
            else
            {
                CornerRadius = new CornerRadius(0);
            }
        }

        private void SetBorder()
        {
            if (BorderVal)
            {
                Border = new Thickness(0.6);
            }
            else
            {
                Border = new Thickness(0);
            }
        }


        public void TrySetPage()
        {
            if(FavPage != null)
            {
                PreviewVM.RequestPage1 = FavPage.PageNr;
            }
        }

        public void SetFavPage(PageData page)
        {
            FavPage = page;
        }

        public void AddFavPage(string pageName)
        {
            int pageNr = PreviewVM.CurrentPage1;
            PageData page = new PageData() { PageNr = pageNr, PageName = pageName };

            if (PreviewVM.CurrentFile != null)
            {
                PreviewVM.CurrentFile.FavPages.Add(page);

                List<PageData> tempList = PreviewVM.CurrentFile.FavPages.OrderBy(x => x.PageNr).ToList();

                PreviewVM.CurrentFile.FavPages.Clear();

                PreviewVM.CurrentFile.FavPages = new ObservableCollection<PageData>(tempList);

            }
        }

        public void RenameFavPage(string pageName)
        {
            if (FavPage != null)
            {
                FavPage.PageName = pageName;
            }
        }

        public void RemoveFavPage(PageData page)
        {
            if (PreviewVM.CurrentFile != null)
            {
                PreviewVM.CurrentFile.FavPages.Remove(page);
            }
            //CurrentFavorite = Favorites.First();
        }


        public void AddFavGroup(string group)
        {
            Favorites.Add(group);
        }

        public void RenameFavGroup(string group)
        {
            int currentIndex = Favorites.IndexOf(CurrentFavorite);
            Favorites.Insert(currentIndex, group);

            ProjectsVM.RenameFavoriteGroup(CurrentFavorite, group);
            Favorites.Remove(CurrentFavorite);
            CurrentFavorite = group;

        }

        public void RemoveFavGroup()
        {
            ProjectsVM.RemoveFavoriteGroup(CurrentFavorite);
            Favorites.Remove(CurrentFavorite);

            CurrentFavorite = Favorites.First();
        }

        public void OnGetFavGroups()
        {
            Favorites.Clear();

            ProjectData FavProject = ProjectsVM.StoredProjects.FirstOrDefault(x => x.Namn == "Favorites");

            List<string> favList = new List<string>();

            if(FavProject == null)
            {
                favList.Add("Default");
            }

            if (FavProject != null)
            {
                favList = FavProject.StoredFiles.Select(x => x.Uppdrag).Distinct().ToList();
            }
            
            Favorites = new ObservableCollection<string>(favList);
        }

        public void OnAddFavorite()
        {
            ProjectsVM.AddFavorite(CurrentFavorite);
        }

        public void OnRemoveFavoriteFile()
        {
            ProjectsVM.RemoveFavorite();
            CurrentFavorite = CurrentFavorite;
        }


        public async Task LoadFile(Avalonia.Visual window)
        {
            var topLevel = TopLevel.GetTopLevel(window);

            var jsonformat = new FilePickerFileType("Json format") { Patterns = new[] { "*.json" } };
            List<FilePickerFileType> formatlist = new List<FilePickerFileType>();
            formatlist.Add(jsonformat);
            IReadOnlyList<FilePickerFileType> fileformat = formatlist;

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Load File",
                AllowMultiple = false,
                FileTypeFilter = fileformat
            });

            if (files.Count > 0)
            {
                await using var stream = await files[0].OpenReadAsync();
                using var streamReader = new StreamReader(stream);
                string fileContent = await streamReader.ReadToEndAsync();

                ProjectsVM = new ProjectViewModel();
                ProjectsVM.StoredProjects = JsonConvert.DeserializeObject<ObservableCollection<ProjectData>>(fileContent);
                ProjectsVM.SetProjectlist();
                ProjectsVM.SetDefaultSelection();
                SetCurrentColor();
                SetAllowedTypes();
                GetGroups();
                OnGetFavGroups();

            }
        }

        public void read_savefile(string path)
        {
            using (StreamReader r = new StreamReader(path))
            {
                string json = r.ReadToEnd();
                ProjectsVM = new ProjectViewModel();
                ProjectsVM.StoredProjects = JsonConvert.DeserializeObject<ObservableCollection<ProjectData>>(json);
                ProjectsVM.SetProjectlist();
                ProjectsVM.SetDefaultSelection();
                SetCurrentColor();
                SetAllowedTypes();
                GetGroups();
                OnGetFavGroups();
            }
        }

        public async Task SaveFile(Avalonia.Visual window)
        {
            ProjectsVM.SetProjectColor(Color1, Color2, Color3, Color4, CornerRadiusVal, BorderVal);

            var topLevel = TopLevel.GetTopLevel(window);

            var jsonformat = new FilePickerFileType("Json format") { Patterns = new[] { "*.json" } };
            List<FilePickerFileType> formatlist = new List<FilePickerFileType>();
            formatlist.Add(jsonformat);
            IReadOnlyList<FilePickerFileType> fileformat = formatlist;


            var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Save File",
                FileTypeChoices = fileformat
            });

            if (file is not null)
            {
                await using var stream = await file.OpenWriteAsync();
                using var streamWriter = new StreamWriter(stream);
                var data = JsonConvert.SerializeObject(ProjectsVM.StoredProjects);
                await streamWriter.WriteLineAsync(data);
            }
        }

        public async Task SaveFileAuto(string path)
        {
            ProjectsVM.SetProjectColor(Color1, Color2, Color3, Color4, CornerRadiusVal, BorderVal);

            using (StreamWriter streamWriter = new StreamWriter(path))
            {
                var data = JsonConvert.SerializeObject(ProjectsVM.StoredProjects);
                await streamWriter.WriteLineAsync(data);
            }

            Debug.WriteLine("Saved");
        }

        public async Task AddFile(Avalonia.Visual window)
        {
            if (ProjectsVM.CurrentProject != null)
            {
                var topLevel = TopLevel.GetTopLevel(window);
                var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    Title = "Add File",
                    FileTypeFilter = new[] { FilePickerFileTypes.Pdf },
                    AllowMultiple = true
                });

                foreach (var file in files)
                {
                    string path = file.Path.LocalPath;
                    ProjectsVM.CurrentProject.Newfile(path);
                    ProjectsVM.SetDefaultType();
                }
            }
        }

        public void SetCurrentColor()
        {
            string[] hexColors = ProjectsVM.GetDefaultProject().Colors;

            if (hexColors != null)
            {
                Color1 = Color.Parse(hexColors[0]);
                Color2 = Color.Parse(hexColors[1]);
                Color3 = Color.Parse(hexColors[2]);
                Color4 = Color.Parse(hexColors[3]);
            }

            bool[] borders = ProjectsVM.GetDefaultProject().Borders;

            if (borders != null)
            {
                CornerRadiusVal = borders[0];
                BorderVal = borders[1];
            }
        }

        public void ColorChanged()
        {           

            var theme = new FluentTheme()
            {
                Palettes =
                {
                    [ThemeVariant.Dark] = new ColorPaletteResources() {RegionColor = Color1, Accent = Color2},
                    [ThemeVariant.Light] = new ColorPaletteResources() {RegionColor = Color3, Accent = Color4 }
                }

            };

            
            App.Current.Resources = theme.Resources;   

        }

        public void AddFilesDrag(string path)
        {
            ProjectsVM.CurrentProject.Newfile(path);
            ProjectsVM.SetDefaultType();
        }

        public void SetCategory(string category)
        {
            ProjectsVM.SetProjecCategory(category);
            SetAllowedTypes();
        }

        public void SetAllowedTypes()
        {
            FileTypeSelection = ProjectsVM.GetAllowedTypes();
        }

        public void SetGroup(string group)
        {
            ProjectsVM.SetGroups(group);
            GetGroups();
        }

        public void GetGroups()
        {
            Groups.Clear();
            Groups = ProjectsVM.GetGroups();
        }

        public void CopyFilenameToClipboard(Avalonia.Visual window)
        {
            string store = string.Empty;

            foreach (FileData file in ProjectsVM.CurrentFiles)
            {
                store += file.Namn + Environment.NewLine;
            }

            TopLevel.GetTopLevel(window).Clipboard.SetTextAsync(store);

        }

        public void CopyFilepathToClipboard(Avalonia.Visual window)
        {
            string store = string.Empty;

            foreach (FileData file in ProjectsVM.CurrentFiles)
            {
                store += file.Sökväg + Environment.NewLine;
            }

            TopLevel.GetTopLevel(window).Clipboard.SetTextAsync(store);

        }

        public void CopyListviewToClipboard(Avalonia.Visual window)
        {
            string store = string.Empty;
            bool[] checkstate = ProjectsVM.GetMetaCheckState();


            foreach (FileData file in ProjectsVM.CurrentFiles)
            {
                if (checkstate[0] == true) { store += file.Namn + "\t"; };
                if (checkstate[1] == true) { store += file.Filtyp + "\t"; };
                if (checkstate[2] == true) { store += file.Uppdrag + "\t"; };
                if (checkstate[3] == true) { store += file.Tagg + "\t"; };
                if (checkstate[4] == true) { store += file.Färg + "\t"; };
                if (checkstate[5] == true) { store += file.Handling + "\t"; };
                if (checkstate[6] == true) { store += file.Status + "\t"; };
                if (checkstate[7] == true) { store += file.Datum + "\t"; };
                if (checkstate[8] == true) { store += file.Ritningstyp + "\t"; };
                if (checkstate[9] == true) { store += file.Beskrivning1 + "\t"; };
                if (checkstate[10] == true) { store += file.Beskrivning2 + "\t"; };
                if (checkstate[11] == true) { store += file.Beskrivning3 + "\t"; };
                if (checkstate[12] == true) { store += file.Beskrivning4 + "\t"; };
                if (checkstate[13] == true) { store += file.Revidering + "\t"; };
                if (checkstate[14] == true) { store += file.Sökväg + "\t"; };

                store += Environment.NewLine;
            }
            TopLevel.GetTopLevel(window).Clipboard.SetTextAsync(store);
        }

        public void SelectFilesForMetaworker(bool singleMode)
        {
            metastore.Clear();
            PathStore.Clear();

            if (singleMode == true)
            {
                foreach (FileData file in ProjectsVM.CurrentFiles) { PathStore.Add((file.Sökväg)); }
            }
            if (singleMode == false)
            {
                foreach (FileData file in ProjectsVM.FilteredFiles) { PathStore.Add((file.Sökväg)); }
            }
        }

        public int GetNrSelectedFiles()
        {
            return PathStore.Count();
        }

        public void set_meta()
        {
            int i = 0;
            foreach (string path in PathStore)
            {
                FileData file = ProjectsVM.FilteredFiles.FirstOrDefault(x => x.Sökväg == path);

                string[] md = metastore[i];

                file.Handling = md[0];
                file.Status = md[1];
                file.Datum = md[2];
                file.Ritningstyp = md[3];
                file.Beskrivning1 = md[4];
                file.Beskrivning2 = md[5];
                file.Beskrivning3 = md[6];
                file.Beskrivning4 = md[7];
                file.Revidering = md[8];
                file.Sökväg = path;

                i++;
            }
        }

        public void GetMetadata(int k)
        {
            string[] tags = ["Handlingstyp = ", "Granskningsstatus = ", "Datum = ", "Ritningstyp = ", "Beskrivning1 = ", "Beskrivning2 = ", "Beskrivning3 = ", "Beskrivning4 = ", "Revidering = "];
            int ntags = tags.Count();

            List<string[]> metadata = new List<string[]>();

            string path = PathStore[k];
            string[] description = new string[ntags];
            try
            {
                string[] lines = System.IO.File.ReadAllLines(path + ".md", Encoding.GetEncoding("ISO-8859-1"));

                int iter = 1;
                int start = 100;
                int end = 0;
                foreach (string line in lines)
                {
                    if (line == "[Metadata]") { start = iter; }
                    if (line.Trim().Length == 0 || iter > start) { end = iter; }
                    iter++;
                }

                for (int i = start; i < end; i++)
                {
                    string line = lines[i];
                    for (int j = 0; j < ntags; j++)
                    {
                        string tag = tags[j];
                        if (line.StartsWith(tag))
                        {
                            description[j] = line.Replace(tag, "");
                        }
                        if (line.StartsWith(tag.ToUpper()))
                        {
                            description[j] = line.Replace(tag.ToUpper(), "");
                        }

                    }
                }
                metastore.Add(description);
            }
            catch (Exception)
            {
                metastore.Add(["", "", "", "", "", "", "", "", ""]);
            }

            FileInfo file = new FileInfo("amit.txt");
            DateTime dt = file.CreationTime;

        }

        public void clear_meta()
        {
            ProjectsVM.ClearSelectedMetadata();
        }

        public void search(string searchtext)
        {
            ProjectsVM.SeachFiles(searchtext);
            OnPropertyChanged("UpdateColumns");
        }

        public void CheckSingleFile()
        {
            if (ProjectsVM.CurrentFile != null)
            {
                if (File.Exists(ProjectsVM.CurrentFile.Sökväg))
                {
                    ProjectsVM.CurrentFile.FileStatus = "OK";
                }
                else
                {
                    ProjectsVM.CurrentFile.FileStatus = "Missing";
                }
            }
        }

        public async Task CheckProjectFiles()
        {
            await Task.Run(() => CheckFileAsync());
        }

        public async Task CheckFileAsync()
        {
            ClearFileStatus();

            int n = ProjectsVM.CurrentProject.StoredFiles.Count();
            int i = 0;

            foreach (FileData file in ProjectsVM.CurrentProject.StoredFiles)
            {
                i++;

                if (File.Exists(file.Sökväg))
                {
                    file.FileStatus = "OK";
                }
                else
                {
                    file.FileStatus = "Missing";
                }

                PreviewVM.Progress = (int)(100 * ((float)i / (float)n));
            }
        }

        public void ClearFileStatus()
        {
            foreach (FileData file in ProjectsVM.CurrentProject.StoredFiles)
            {
                file.FileStatus = "";
            }
        }

        public void open_files()
        {
            try
            {
                foreach (FileData file in ProjectsVM.CurrentFiles)
                {
                    ProcessStartInfo psi = new ProcessStartInfo();
                    psi.FileName = file.Sökväg;
                    psi.UseShellExecute = true;
                    Process.Start(psi);
                }
            }
            catch (Exception e) { Debug.WriteLine(e); }
        }

        public void open_meta()
        {
            try
            {
                foreach (FileData file in ProjectsVM.CurrentFiles)
                {
                    ProcessStartInfo psi = new ProcessStartInfo();
                    psi.FileName = file.Sökväg + ".md";
                    psi.UseShellExecute = true;
                    Process.Start(psi);
                }
            }
            catch { }
        }

        public void open_dwg()
        {
            if (ProjectsVM.CurrentFile.Filtyp == "Drawing")
            {
                string dwgPathOld = ProjectsVM.CurrentFile.Sökväg.Replace("Ritning", "Ritdef").Replace("pdf", "dwg");
                string dwgPathNew = ProjectsVM.CurrentFile.Sökväg.Replace("Drawing", "Drawing Definition").Replace("pdf", "dwg");

                try
                {
                    ProcessStartInfo psi = new ProcessStartInfo();
                    psi.FileName = dwgPathOld;
                    psi.UseShellExecute = true;
                    Process.Start(psi);
                }
                catch (Exception)
                { }

                try
                {
                    ProcessStartInfo psi = new ProcessStartInfo();
                    psi.FileName = dwgPathNew;
                    psi.UseShellExecute = true;
                    Process.Start(psi);
                }
                catch (Exception)
                { }
            }
        }

        public void open_doc()
        {
            if (ProjectsVM.CurrentFile.Filtyp == "Document")
            {
                string docPath = ProjectsVM.CurrentFile.Sökväg.Replace("pdf", "docx");

                try
                {
                    ProcessStartInfo psi = new ProcessStartInfo();
                    psi.FileName = docPath;
                    psi.UseShellExecute = true;
                    Process.Start(psi);
                }
                catch (Exception)
                { }
            }

        }

        public void open_path()
        {
            try
            {
                string folderpath = System.IO.Path.GetDirectoryName(ProjectsVM.CurrentFile.Sökväg);
                Process process = Process.Start("explorer.exe", "\"" + folderpath + "\"");
            }

            catch (Exception e)
            { }
        }

        public void add_color(string color)
        {
            foreach (FileData file in ProjectsVM.CurrentFiles)
            {
                file.Färg = color;
            }
        }

        public void clear_all()
        {
            foreach (FileData file in ProjectsVM.CurrentFiles)
            {
                file.Färg = "";
                file.Tagg = "";
            }
        }

        public void add_tag(string tag)
        {
            foreach (FileData file in ProjectsVM.CurrentFiles)
            {
                file.Tagg = tag;
            }
        }

        public void clear_tag()
        {
            foreach (FileData file in ProjectsVM.CurrentFiles)
            {
                file.Tagg = "";
            }
        }

        public void edit_type(string type)
        {
            ProjectsVM.SetTypeSelected(type);
        }

        public void select_files(IList<FileData> files)
        {
            ProjectsVM.CurrentFiles = files;
        }

        public void select_type(string name)
        {
            string currentType = ProjectsVM.Type;

            if (currentType != name)
            {
                ProjectsVM.Type = name;
            }
            OnPropertyChanged("UpdateColumns");
        }

        public void select_project(string name)
        {
            string currentProjectName = ProjectsVM.CurrentProject.Namn;
            if (currentProjectName != name)
            {
                ProjectsVM.SetProject(name);
                SetAllowedTypes();
            }
            OnPropertyChanged("UpdateColumns");
        }

        public void ReselectProject()
        {
            ProjectsVM.SetProject(ProjectsVM.CurrentProject.Namn);
            SetAllowedTypes();
            OnPropertyChanged("UpdateColumns");
        }

        public void remove_project()
        {
            ProjectsVM.RemoveProject();
        }

        public void rename_project(string newProjectName)
        {
            ProjectsVM.RenameProject(newProjectName);
            ProjectsVM.SetProjectlist();
        }

        public void UpdateTreeview()
        {
            OnPropertyChanged("TreeViewUpdate");
        }



        public void move_files(string projectname)
        {
            ProjectsVM.TransferFiles(projectname);
        }

        public void UpdateLists(string selectedProject, string selectedType)
        {
            int fileCount = ProjectsVM.FilteredFiles.Count();
            ProjectMessage = string.Format("Project {0}/ Type {1}: {2} Files", selectedProject, selectedType, fileCount);
            OnPropertyChanged("ProjectMessage");

        }
    }
}

