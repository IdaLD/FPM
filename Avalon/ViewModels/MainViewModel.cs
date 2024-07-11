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

        public string ProjectMessage { get; set; } = "";

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
            }
        }

        public async Task SaveFile(Avalonia.Visual window)
        {
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
            using (StreamWriter streamWriter = new StreamWriter(path))
            {
                var data = JsonConvert.SerializeObject(ProjectsVM.StoredProjects);
                await streamWriter.WriteLineAsync(data);

            }
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

        public void AddFilesDrag(string path)
        {
            ProjectsVM.CurrentProject.Newfile(path);
            ProjectsVM.SetDefaultType();
        }

        public void set_category(string category)
        {
            ProjectsVM.SetProjecCategory(category);
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
                string dwgPath = ProjectsVM.CurrentFile.Sökväg.Replace("Ritning", "Ritdef").Replace("pdf", "dwg");

                try
                {
                    ProcessStartInfo psi = new ProcessStartInfo();
                    psi.FileName = dwgPath;
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
            }
            OnPropertyChanged("UpdateColumns");
        }

        public void new_project(string name)
        {
            ProjectsVM.NewProject(name);
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

