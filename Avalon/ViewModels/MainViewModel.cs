using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System;
using System.Diagnostics.Metrics;

using Docnet.Core;
using Docnet.Core.Models;
using System.ComponentModel;

using Docnet.Core.Readers;
using System.Runtime.InteropServices;
using Avalonia.Media.Imaging;
using Avalonia;
using Avalonia.Platform;
using Avalon.Model;
using Newtonsoft.Json.Bson;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Xml.Serialization;
using Avalonia.Collections;


namespace Avalon.ViewModels;


public class MainViewModel : ViewModelBase, INotifyPropertyChanged
{
    public MainViewModel()
    {
        Status.Add("Ready");
    }


    private Projects projectsModel = new Projects();
    public Projects ProjectsModel
    {
        get { return projectsModel; }
        set { projectsModel = value; OnPropertyChanged("ProjectsModel"); }
    }

    public ObservableCollection<string> Status { get; } = new();
    public ObservableCounter<int> Progress { get; set; }

    private Avalonia.Media.Imaging.Bitmap? previewFile;
    public Avalonia.Media.Imaging.Bitmap? PreviewFile
    {
        get { return previewFile; }
        set { previewFile = value; OnPropertyChanged("PreviewFile"); }
    }

    private WriteableBitmap? imageFromBinding = null;
    public WriteableBitmap? ImageFromBinding
    {
        get { return imageFromBinding; }
        set { imageFromBinding = value; OnPropertyChanged("ImageFromBinding"); }
    }

    private WriteableBitmap? imageFromBinding2 = null;
    public WriteableBitmap? ImageFromBinding2
    {
        get { return imageFromBinding2; }
        set { imageFromBinding2 = value; OnPropertyChanged("ImageFromBinding2"); }
    }

    private FileData currentInfoFile = new FileData();
    public FileData CurrentInfoFile
    {
        get { return currentInfoFile; }
        set { currentInfoFile = value; OnPropertyChanged("CurrentInfoFile"); }
    }

    public string user_tag { get; set; }

    public List<string[]> metastore = new List<string[]>();
    public List<string> PathStore = new List<string>();


    public string ProjectMessage { get; set; } = "";

    public IDocReader docReader { get; set; } = null;

    public int _pw_pagenr = 0;
    public int pw_pagenr
    {
        get { return _pw_pagenr; }
        set { _pw_pagenr = value; OnPropertyChanged("pw_pagenr"); }
    }
    public int _pw_pagenr_view = 1;
    public int pw_pagenr_view
    {
        get { return _pw_pagenr_view; }
        set { _pw_pagenr_view = value; OnPropertyChanged("pw_pagenr_view"); }
    }
    public int _pw_pagecount_view = 1;
    public int pw_pagecount_view
    {
        get { return _pw_pagecount_view; }
        set { _pw_pagecount_view = value; OnPropertyChanged("pw_pagecount_view"); }
    }

    public bool _pw_dualmode = false;
    public bool pw_dualmode
    {
        get { return _pw_dualmode; }
        set { _pw_dualmode = value; OnPropertyChanged("pw_dualmode"); }
    }

    public void create_preview_file(string filepath, int fak)
    {
        if (docReader != null)
        {
            docReader.Dispose();
        }

        try
        {
            pw_pagenr = 0;
            docReader = DocLib.Instance.GetDocReader(filepath, new PageDimensions(fak * 1080/2, fak * 1920/2));
            pw_pagecount_view = docReader.GetPageCount();
            pw_pagenr_view = 1;

        }
        catch
        {
            return;
        }
    }

    public void clear_preview_file()
    {
        docReader = null;
        ImageFromBinding = null;
        ImageFromBinding2 = null;
    }

    public void next_preview_page()
    {
        if (pw_pagenr < docReader.GetPageCount()-1)
        {
            if (pw_dualmode == false)
            {
                pw_pagenr++;
                preview_page(pw_pagenr, 0);
            }


            if (pw_dualmode == true)
            {
                pw_pagenr = pw_pagenr + 2;

                preview_page(pw_pagenr, 0);
                preview_page(pw_pagenr+1, 1);
            }

            pw_pagenr_view = pw_pagenr + 1;
        }
    }

    public void previous_preview_page()
    {
        if (pw_dualmode == false)
        {
            if (pw_pagenr > 0)
            {
                pw_pagenr--;
                preview_page(pw_pagenr, 0);
            }
        }

        if (pw_dualmode == true)
        {
            if (pw_pagenr > 1)
            {
                preview_page(pw_pagenr-2, 0);
                preview_page(pw_pagenr-1, 1);

                pw_pagenr = pw_pagenr - 2;
            }
        }

        pw_pagenr_view = pw_pagenr + 1;

    }

    public void selected_page(int pagenr)
    {
        if (pagenr != pw_pagenr)
        {
            if (pw_dualmode == false)
            {
                preview_page(pagenr, 0);
            }
            
            if (pw_dualmode == true)
            {
                preview_page(pagenr, 0);
                preview_page(pagenr+1, 1);
            }

            pw_pagenr = pagenr;
        }
    }

    public void toggle_pw_mode()
    {
        pw_dualmode = !pw_dualmode;
        start_preview_page();
    }

    public void start_preview_page()
    {
        imageFromBinding = null;
        imageFromBinding2 = null;

        if (pw_dualmode == false)
        {
            pw_pagenr = 0;
            preview_page(pw_pagenr, 0);
        }
        if (pw_dualmode == true)
        {
            pw_pagenr = 0;
            preview_page(pw_pagenr, 0);
            preview_page(pw_pagenr+1, 1);
        }

        pw_pagenr_view = pw_pagenr + 1;
    }

    public void preview_page(int pagenr, int mode)
    {
        if (docReader != null && docReader.GetPageCount()-1 >= pagenr)
        {

            IPageReader page = docReader.GetPageReader(pagenr);

            byte[] rawBytes = page.GetImage();
            int width = page.GetPageWidth();
            int height = page.GetPageHeight();

            Avalonia.Vector dpi = new Avalonia.Vector(96, 96);

            if (mode == 0)
            { // Bgra8888
                ImageFromBinding = new WriteableBitmap(new PixelSize(width, height), dpi, Avalonia.Platform.PixelFormat.Bgra8888, AlphaFormat.Premul);
                using (var frameBuffer = ImageFromBinding.Lock())
                {
                    Marshal.Copy(rawBytes, 0, frameBuffer.Address, rawBytes.Length);
                }
                ImageFromBinding2 = null;
            }

            if (mode == 1)
            {
                ImageFromBinding2 = new WriteableBitmap(new PixelSize(width, height), dpi, Avalonia.Platform.PixelFormat.Bgra8888, AlphaFormat.Premul);
                using (var frameBuffer = ImageFromBinding2.Lock())
                {
                    Marshal.Copy(rawBytes, 0, frameBuffer.Address, rawBytes.Length);
                }
            }

        }
    }

    public async Task LoadFileOld(Avalonia.Visual window)
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

            ObservableCollection<FileData> oldfiles = JsonConvert.DeserializeObject<ObservableCollection<FileData>>(fileContent);

            ProjectsModel.NewProject("Old files");

            ProjectsModel.SetProject("Old files");

            ProjectsModel.CurrentProject.AddFiles(oldfiles);

            ProjectsModel.SetDefaultSelection();

        }
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

            ProjectsModel = new Projects();
            ProjectsModel.StoredProjects = JsonConvert.DeserializeObject<ObservableCollection<ProjectData>>(fileContent);
            ProjectsModel.SetProjectlist();
            ProjectsModel.SetDefaultSelection();

        }
    }

    public void read_savefile(string path)
    {
        using (StreamReader r = new StreamReader(path))
        {
            string json = r.ReadToEnd();
            ProjectsModel = new Projects();
            ProjectsModel.StoredProjects = JsonConvert.DeserializeObject<ObservableCollection<ProjectData>>(json);
            ProjectsModel.SetProjectlist();
            ProjectsModel.SetDefaultSelection();

        }

    }

    public async Task SaveFile(Avalonia.Visual window)
    {
        var topLevel = TopLevel.GetTopLevel(window);

        var jsonformat = new FilePickerFileType("Json format"){Patterns = new[] { "*.json" }};
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
            var data = JsonConvert.SerializeObject(ProjectsModel.StoredProjects);
            await streamWriter.WriteLineAsync(data);
        }
    }

    public async Task SaveFileAuto(string path)
    {
        using (StreamWriter streamWriter = new StreamWriter(path))
        {
            var data = JsonConvert.SerializeObject(ProjectsModel.StoredProjects);
            await streamWriter.WriteLineAsync(data);

        }
    }

    public async Task AddFile(Avalonia.Visual window)
    {

        if (ProjectsModel.CurrentProject != null)
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
                ProjectsModel.CurrentProject.Newfile(path);
            }
        }
        ProjectsModel.SetDefaultType();
    }

    public void CopyFilenameToClipboard(Avalonia.Visual window)
    {
        string store = string.Empty;

        foreach (FileData file in ProjectsModel.CurrentFiles)
        {
            store += file.Namn + Environment.NewLine;
        }

        TopLevel.GetTopLevel(window).Clipboard.SetTextAsync(store);

    }

    public void CopyListviewToClipboard(Avalonia.Visual window)
    {
        string store = string.Empty;
        bool[] checkstate = ProjectsModel.GetMetaCheckState();


        foreach (FileData file in ProjectsModel.CurrentFiles)
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
            foreach (FileData file in ProjectsModel.CurrentFiles) { PathStore.Add((file.Sökväg)); }
        }
        if (singleMode == false)
        {
            foreach (FileData file in ProjectsModel.FilteredFiles) { PathStore.Add((file.Sökväg)); }
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
            FileData file = ProjectsModel.FilteredFiles.FirstOrDefault(x => x.Sökväg == path);

            string[] md         = metastore[i];

            file.Handling       = md[0];
            file.Status         = md[1];
            file.Datum          = md[2];
            file.Ritningstyp    = md[3];
            file.Beskrivning1   = md[4];
            file.Beskrivning2   = md[5];
            file.Beskrivning3   = md[6];
            file.Beskrivning4   = md[7];
            file.Revidering     = md[8];
            file.Sökväg         = path;

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
                if (line == "[Metadata]"){start = iter;}
                if (line.Trim().Length == 0 || iter > start ){end = iter; }
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
        ProjectsModel.ClearSelectedMetadata();
    }

    public void open_files()
    {
        try
        {
            foreach (FileData file in ProjectsModel.CurrentFiles)
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = file.Sökväg;
                psi.UseShellExecute = true;
                Process.Start(psi);
            }
        }
        catch { }
    }

    public void open_meta()
    {
        try
        {
            foreach (FileData file in ProjectsModel.CurrentFiles)
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
        if (ProjectsModel.CurrentFile.Filtyp == "Drawing")
        {
            string dwgPath = ProjectsModel.CurrentFile.Sökväg.Replace("Ritning", "Ritdef").Replace("pdf", "dwg");

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
            string folderpath = System.IO.Path.GetDirectoryName(ProjectsModel.CurrentFile.Sökväg);
            Process process = Process.Start("explorer.exe", "\"" + folderpath + "\"");
        }

        catch(Exception e)
        { }
    }

    public void add_color(string color)
    {
        foreach (FileData file in ProjectsModel.CurrentFiles)
        {
            file.Färg = color;
        }
    }

    public void clear_all()
    {
        foreach (FileData file in ProjectsModel.CurrentFiles)
        {
            file.Färg = "";
            file.Tagg = "";
        }
    }

    public void add_tag()
    {
        foreach (FileData file in ProjectsModel.CurrentFiles)
        {
            file.Tagg = user_tag;
        }
    }

    public void clear_tag()
    {
        foreach (FileData file in ProjectsModel.CurrentFiles)
        {
            file.Tagg = "";
        }
    }

    public void edit_type(string type)
    {
        ProjectsModel.SetTypeSelected(type);
    }

    public void select_files(IList<FileData> files)
    {
        ProjectsModel.CurrentFiles = files;
    }

    public void remove_duplicate_files()
    {
        //ProjectsModel.CurrentProject.RemoveDuplicates();
    }

    public void treeview_update(object item)
    {
        try
        {
            ProjectData project = (ProjectData)item;
            select_project(project.Namn);
        }
        catch
        {
            select_type(item.ToString());
        }
        
    }

    public void select_type(string name)
    {
        ProjectsModel.Type = name;
    }

    public void select_project(string name)
    {
        ProjectsModel.SetProject(name);
    }

    public void new_project(string name)
    {
        ProjectsModel.NewProject(name);
    }

    public void remove_project()
    {
        ProjectsModel.RemoveProject();
    }

    public void rename_project(string newProjectName)
    {
        ProjectsModel.CurrentProject.RenameProject(newProjectName);
        ProjectsModel.SetProjectlist();
    }

    public void move_files(string projectname)
    {
        ProjectsModel.TransferFiles(projectname);
    }

    public void UpdateLists(string selectedProject, string selectedType)
    {
        int fileCount = ProjectsModel.FilteredFiles.Count();
        ProjectMessage = string.Format("Project {0}/ Type {1}: {2} Files", selectedProject, selectedType, fileCount);
        OnPropertyChanged("ProjectMessage");

    }


    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }



}

