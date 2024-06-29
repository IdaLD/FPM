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
using System.Drawing;
using System.Drawing.Imaging;
using System.ComponentModel; 

using Docnet.Core.Readers;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Avalonia.Media.Imaging;
using iText.Layout.Renderer;
using System.Numerics;
using Avalonia;
using Avalonia.Platform;
using Newtonsoft.Json.Bson;
using Avalonia.Media;
using Avalonia.Collections;
using DynamicData;


namespace Avalon.ViewModels;


public class MainViewModel : ViewModelBase, INotifyPropertyChanged
{
    public MainViewModel()
    {
        Projects.Add("New Project");
        Status.Add("Ready");
    }

    public ObservableCollection<FileData> StoredFiles = new ObservableCollection<FileData>();

    private IEnumerable<FileData> filteredFiles = null;
    public IEnumerable<FileData> FilteredFiles
    {
        get { return filteredFiles; }
        set { filteredFiles = value; OnPropertyChanged("FilteredFiles"); }
    }

    public List<string> Projects { get; set; } = new();

    private List<string> types = new List<string>();
    public List<string> Types
    {
        get { return types; }
        set { types = value; OnPropertyChanged("Types"); }
    }

    public ObservableCollection<string> Properties { get; } = new();
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

            docReader = DocLib.Instance.GetDocReader(filepath, new PageDimensions(fak * 1080/4, fak * 1920/4));
            pw_pagecount_view = docReader.GetPageCount(); OnPropertyChanged("pw_pagecount_view");
            pw_pagenr_view = 1; OnPropertyChanged("pw_pagenr_view");
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
    }

    public void next_preview_page()
    {
        if (pw_pagenr < docReader.GetPageCount()-1)
        {
            pw_pagenr++;

            preview_page(pw_pagenr, 0);
            pw_pagenr_view = pw_pagenr + 1;

            if (pw_dualmode == true)
            {
                pw_pagenr++;

                preview_page(pw_pagenr, 1);
                pw_pagenr_view = pw_pagenr + 1;
            }
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

                pw_pagenr_view = pw_pagenr + 1;
            }
        }
        if (pw_dualmode == true)
        {
            if (pw_pagenr > 1)
            {
                pw_pagenr = pw_pagenr - 2;
                preview_page(pw_pagenr, 0);

                pw_pagenr++;
                preview_page(pw_pagenr, 1);

                pw_pagenr--;

                pw_pagenr_view = pw_pagenr+1;
            }
        }

    }

    public void selected_page(int pagenr)
    {
        if (pagenr != pw_pagenr)
        {
            pw_pagenr = pagenr;
            pw_pagenr_view = pw_pagenr + 1;

            preview_page(pw_pagenr, 0);      

        }
    }

    public void toggle_pw_mode()
    {
        pw_dualmode = !pw_dualmode;
    }

    public void start_preview_page()
    {
        if (pw_dualmode == false)
        {
            preview_page(0, 0);
        }
        if (pw_dualmode == true)
        {
            preview_page(0, 0);
            preview_page(1, 1);
        }
    }

    public void preview_page(int pagenr, int mode)
    {
        Debug.WriteLine(pagenr);
        if (docReader != null && docReader.GetPageCount()-1 >= pagenr)
        {
            IPageReader page = docReader.GetPageReader(pagenr);

            byte[] rawBytes = page.GetImage();
            int width = page.GetPageWidth();
            int height = page.GetPageHeight();

            Avalonia.Vector dpi = new Avalonia.Vector(96, 96);

            if (mode == 0)
            {
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

            StoredFiles = JsonConvert.DeserializeObject<ObservableCollection<FileData>>(fileContent);
            Projects = StoredFiles.Select(x => x.Uppdrag).Distinct().ToList();
            Projects.Sort();

            UpdateTypes();

            var properties = typeof(FileData).GetProperties().ToList();
            foreach (var property in properties)
            {
                string val = property.Name;
                Properties.Add(val);
            }
        }
    }

    public void read_savefile(string path)
    {

        using (StreamReader r = new StreamReader(path))
        {
            string json = r.ReadToEnd();

            StoredFiles = JsonConvert.DeserializeObject<ObservableCollection<FileData>>(json);
            Projects = StoredFiles.Select(x => x.Uppdrag).Distinct().ToList();
            Projects.Sort();

            UpdateTypes();
            
        }

        var properties = typeof(FileData).GetProperties().ToList();
        foreach (var property in properties)
        {
            string val = property.Name;
            Properties.Add(val);
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
            var data = JsonConvert.SerializeObject(StoredFiles);
            await streamWriter.WriteLineAsync(data);
        }
    }

    public async Task SaveFileAuto(string path)
    {
        using (StreamWriter streamWriter = new StreamWriter(path))
        {
            var data = JsonConvert.SerializeObject(StoredFiles);
            await streamWriter.WriteLineAsync(data);
        }
    }

    public async Task AddFile(string selectedProject, Avalonia.Visual window)
    {
        if (Projects.Count > 0)
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

                if(!StoredFiles.Any(x => x.Sökväg == path))
                {
                    StoredFiles.Add(new FileData
                    {
                        Namn = System.IO.Path.GetFileNameWithoutExtension(path),
                        Uppdrag = selectedProject,
                        Filtyp = "",
                        Sökväg = path
                    });
                }


            }
        }
    }

    public void UpdateTypes()
    {
        List<string> newTypes = StoredFiles.Select(x => x.Filtyp).Distinct().ToList();

        newTypes.Remove("");
        newTypes.Sort();

        Types = new List<string>();
        Types.Add("All Files");
        Types.Add(newTypes);
        Types.Add("Empty");
    }

    public void CopyFilenameToClipboard(Avalonia.Visual window, IList files)
    {
        string store = string.Empty;

        foreach (FileData file in files)
        {
            store += file.Namn + Environment.NewLine;
        }

        TopLevel.GetTopLevel(window).Clipboard.SetTextAsync(store);

    }

    public void CopyListviewToClipboard(Avalonia.Visual window, IList files, bool?[] checkstate)
    {
        string store = string.Empty;
        
        foreach (FileData file in files)
        {
            if (checkstate[0] == true) { store += file.Namn + "\t"; };
            if (checkstate[1] == true) { store += file.Filtyp + "\t"; };
            if (checkstate[2] == true) { store += file.Tagg + "\t"; };
            if (checkstate[3] == true) { store += file.Färg + "\t"; };
            if (checkstate[4] == true) { store += file.Handling + "\t"; };
            if (checkstate[5] == true) { store += file.Status + "\t"; };
            if (checkstate[6] == true) { store += file.Datum + "\t"; };
            if (checkstate[7] == true) { store += file.Ritningstyp + "\t"; };
            if (checkstate[8] == true) { store += file.Beskrivning1 + "\t"; };
            if (checkstate[9] == true) { store += file.Beskrivning2 + "\t"; };
            if (checkstate[10] == true) { store += file.Beskrivning3 + "\t"; };
            if (checkstate[11] == true) { store += file.Beskrivning4 + "\t"; };
            if (checkstate[12] == true) { store += file.Revidering + "\t"; };
            if (checkstate[13] == true) { store += file.Sökväg + "\t"; };

            store += Environment.NewLine;

            
        }
        TopLevel.GetTopLevel(window).Clipboard.SetTextAsync(store);
    }

    public void SelectFiles(bool singleMode, IList files)
    {
        metastore.Clear();
        PathStore.Clear();

        if (singleMode == true)
        {
            foreach (FileData file in files) { PathStore.Add((file.Sökväg)); }
        }
        if (singleMode == false)
        {
            foreach (FileData file in FilteredFiles) { PathStore.Add((file.Sökväg)); }
        }

    }

    public int GetNrSelectedFiles()
    {
        return PathStore.Count();
    }

    public void SetMetadata()
    {

        int i = 0;
        foreach (string path in PathStore)
        {
            FileData file = FilteredFiles.FirstOrDefault(x => x.Sökväg == path);

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
    }

    public void OpenFile(IList files, string openType)
    {
        string ending = "";
        if (openType == "MD") { ending = ".md"; }
        try
        {
            foreach (FileData file in files)
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = file.Sökväg + ending;
                psi.UseShellExecute = true;
                Process.Start(psi);
            }
        }
        catch (Exception)
        { }
    }

    public void OpenDwg(FileData drawing)
    {
        string dwgPath = drawing.Sökväg.Replace("Ritning", "Ritdef").Replace("pdf","dwg");

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

    public void OpenPath(IList files)
    {

        foreach (FileData file in files)
        {
            try
            {
                string folderpath = System.IO.Path.GetDirectoryName(file.Sökväg);
                Process process = Process.Start("explorer.exe", "\"" + folderpath + "\"");

            }
            catch(Exception e)
            { 
                Debug.WriteLine(e);
            }

        }
    }

    public void add_color(string color, IList items)
    {
        foreach (FileData file in items)
        {
            file.Färg = color;
        }
    }


    public void clear_all(IList items)
    {
        foreach (FileData file in items)
        {
            file.Färg = "";
            file.Tagg = "";
        }
    }

    public void add_tag(bool tagmode, IList items)
    {
        string Tag = "";
        if (tagmode == true) { Tag = user_tag; };
        foreach (FileData file in items)
        {
            file.Tagg = Tag;
        }
    }

    public void add_type(string type, IList items)
    {
        foreach (FileData file in items)
        {
            file.Filtyp = type;
        }
    }

    public void remove_files(IList files)
    {
        foreach (FileData file in files)
        {
            StoredFiles.Remove(file);
        }
    }

    public void new_project(string newName)
    {
        Projects.Add(newName);
        Projects.Sort();

    }

    public void remove_project(string currentProject)
    {
        if (Projects.Count > 1)
        {
            StoredFiles.RemoveMany(StoredFiles.Where(x => x.Uppdrag == currentProject));
            Projects.Remove(currentProject);
        }
    }

    public void rename_project(string currentProject, string newProjectName)
    {
        for (int i = 0; i < StoredFiles.Count; i++)
        {
            if (StoredFiles[i].Uppdrag == currentProject)
            {
                StoredFiles[i].Uppdrag = newProjectName;
            }
        }
        Projects.Replace(currentProject, newProjectName);
        Projects.Sort();
    }


    public void UpdateLists(string selectedProject, string selectedType)
    {
        if (Projects.Count() > 0)
        {
            set_filtered_res(selectedProject, selectedType);

            int fileCount = FilteredFiles.Count();

            ProjectMessage = string.Format("Project {0}/ Type {1}: {2} Files", selectedProject, selectedType, fileCount);
            OnPropertyChanged("ProjectMessage");
        }

    }

    private void set_filtered_res(string currentProject, string type)
    {
        if (type == "All Files")
        {
            FilteredFiles = StoredFiles.Where(x => x.Uppdrag == currentProject).OrderBy(x=>x.Filtyp).OrderBy(x=>x.Namn);
        }

        if (type == "Empty")
        {
            FilteredFiles = StoredFiles.Where(x => x.Uppdrag == currentProject).Where(x => x.Filtyp == "").OrderBy(x => x.Namn);
        }
        
        if (type != "All Files" && type != "Empty")
        {
            FilteredFiles = StoredFiles.Where(x => x.Uppdrag == currentProject).Where(x => x.Filtyp == type).OrderBy(x => x.Namn);
        }
        
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }



}

