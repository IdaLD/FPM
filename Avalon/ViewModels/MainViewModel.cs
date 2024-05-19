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


namespace Avalon.ViewModels;


public class MainViewModel : ViewModelBase, INotifyPropertyChanged
{
    public MainViewModel()
    {
        Projects.Add("New Project");
        Status.Add("Ready");
    }

    public ObservableCollection<FileData> Drawings { get; } = new();
    public ObservableCollection<FileData> Documents { get; } = new();
    public ObservableCollection<string> Projects { get; } = new();
    public ObservableCollection<string> Properties { get; } = new();
    public ObservableCollection<string> Status { get; } = new();
    public ObservableCounter<int> Progress { get; set; } 



    public string user_tag { get; set; }

    public List<string[]> metastore = new List<string[]>();
    public List<(string, string)> PathStore = new List<(string, string)>();

    public string ProjectMessage { get; set; } = "";

    public IDocReader? docReader { get; set; } = null;
    public int pw_pagenr { get; set; } = 0;
    public int pw_pagenr_view { get; set; } = 1;
    public int pw_pagecount_view { get; set; } = 1;

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
        ImageFromBinding = null; OnPropertyChanged("ImageFromBinding");
    }

    public void next_preview_page()
    {
        if (pw_pagenr < docReader.GetPageCount()-1)
        {
            pw_pagenr++; OnPropertyChanged("pw_pagenr");
            preview_page(pw_pagenr);

            pw_pagenr_view = pw_pagenr + 1; OnPropertyChanged("pw_pagenr_view");
        }
    }

    public void previous_preview_page()
    {
        if (pw_pagenr > 0)
        {
            pw_pagenr--; OnPropertyChanged("pw_pagenr");
            preview_page(pw_pagenr);

            pw_pagenr_view = pw_pagenr + 1; OnPropertyChanged("pw_pagenr_view");
        }
    }

    public void selected_page(int pagenr)
    {
        if (pagenr != pw_pagenr)
        {
            pw_pagenr = pagenr; OnPropertyChanged("pw_pagenr");
            pw_pagenr_view = pw_pagenr + 1; OnPropertyChanged("pw_pagenr_view");

            preview_page(pw_pagenr);
        }

    }


    public void preview_page(int pagenr)
    {
        IPageReader page = docReader.GetPageReader(pagenr);

        byte[] rawBytes = page.GetImage();
        int width = page.GetPageWidth();
        int height = page.GetPageHeight();

        Avalonia.Vector dpi = new Avalonia.Vector(96, 96);

        ImageFromBinding = new WriteableBitmap(new PixelSize(width, height),dpi,Avalonia.Platform.PixelFormat.Bgra8888,AlphaFormat.Premul);

        using (var frameBuffer = ImageFromBinding.Lock())
        {
            Marshal.Copy(rawBytes, 0, frameBuffer.Address, rawBytes.Length);
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

            List<FileData> getFiles = JsonConvert.DeserializeObject<List<FileData>>(fileContent);
            List<string> getProjects = getFiles.Select(x => x.Uppdrag).Distinct().ToList();

            Globals.storedFiles = getFiles;
            Globals.projects = getProjects;

            var properties = typeof(FileData).GetProperties().ToList();
            foreach (var property in properties)
            {
                string val = property.Name;
                Properties.Add(val);
            }
            UpdateProjectList();

        }
    }

    public void read_savefile(string path)
    {

        using (StreamReader r = new StreamReader(path))
        {
            string json = r.ReadToEnd();
            List<FileData> getFiles = JsonConvert.DeserializeObject<List<FileData>>(json);
            List<string> getProjects = getFiles.Select(x => x.Uppdrag).Distinct().ToList();

            Globals.storedFiles = getFiles;
            Globals.projects = getProjects;
        }

        var properties = typeof(FileData).GetProperties().ToList();
        foreach (var property in properties)
        {
            string val = property.Name;
            Properties.Add(val);
        }
        UpdateProjectList();

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
            var data = JsonConvert.SerializeObject(Globals.storedFiles);
            await streamWriter.WriteLineAsync(data);
        }
    }

    public async Task SaveFileAuto(string path)
    {
        using (StreamWriter streamWriter = new StreamWriter(path))
        {
            var data = JsonConvert.SerializeObject(Globals.storedFiles);
            await streamWriter.WriteLineAsync(data);
        }
    }

    public async Task AddFile(string type, int selectedProject, Avalonia.Visual window)
    {
        if (Globals.projects.Count > 0)
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

                Globals.storedFiles.Add(new FileData
                {
                    Namn = System.IO.Path.GetFileNameWithoutExtension(path),
                    Uppdrag = Globals.projects[selectedProject],
                    Filtyp = type,
                    Sökväg = path
                });
            }
            UpdateLists(selectedProject);
        }
    }

    public void SelectFiles(bool singleMode, IList drawings, IList documents, string SelectedType)
    {
        metastore.Clear();
        PathStore.Clear();

        IList selectedDrawings = null;
        IList selectedDocuments = null;

        if (singleMode == true)
        {
            if (SelectedType == "Drawing")
            {
                selectedDrawings = drawings;
            }
            if (SelectedType == "Document")
            {
                selectedDocuments = documents;
            }
        }
        if (singleMode == false)
        {
            selectedDrawings = Drawings;
            selectedDocuments = Documents;
        }

        if (selectedDrawings != null)
        {
            foreach (FileData file in selectedDrawings) { PathStore.Add(("Drawing", file.Sökväg)); }
        }
        if (selectedDocuments != null)
        {
            foreach (FileData file in selectedDocuments) { PathStore.Add(("Document", file.Sökväg)); }
        }
    }

    public int GetNrSelectedFiles()
    {
        return PathStore.Count();
    }

    public void SetMetadata()
    {
        IList items = null;
        FileData file = null;

        int nSelected = PathStore.Count();
        for (int i = 0; i < nSelected; i++)
        {
            if (PathStore[i].Item1 == "Drawing")
            {
                items = Drawings;
                file = Drawings.First(x => x.Sökväg == PathStore[i].Item2);
            }
            if (PathStore[i].Item1 == "Document")
            {
                items = Documents;
                file = Documents.First(x => x.Sökväg == PathStore[i].Item2);
            }

            string path         = file.Sökväg;
            string[] md         = metastore[i];

            int index           = items.IndexOf(file);

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

            items[index] = null;
            items[index] = file;
        }
    }

    public void GetMetadata(int k)
    {
        string[] tags = ["Handlingstyp = ", "Granskningsstatus = ", "Datum = ", "Ritningstyp = ", "Beskrivning1 = ", "Beskrivning2 = ", "Beskrivning3 = ", "Beskrivning4 = ", "Revidering = "];
        int ntags = tags.Count();

        List<string[]> metadata = new List<string[]>();

        string path = PathStore[k].Item2;
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

    public void OpenFile(IList drawings, IList documents, string SelectedType, string openType)
    {
        string ending = "";
        if (openType == "MD") { ending = ".md"; }
        try
        {
            if (SelectedType == "Drawing")
            {
                foreach (FileData drawing in drawings)
                {
                    ProcessStartInfo psi = new ProcessStartInfo();
                    psi.FileName = drawing.Sökväg + ending;
                    psi.UseShellExecute = true;
                    Process.Start(psi);
                }
            }
            if (SelectedType == "Document")
            {
                foreach (FileData document in documents)
                {
                    ProcessStartInfo psi = new ProcessStartInfo();
                    psi.FileName = document.Sökväg + ending;
                    psi.UseShellExecute = true;
                    Process.Start(psi);
                }
            }
        }
        catch (Exception)
        { }
    }

    public void OpenPath(IList drawings, IList documents, string SelectedType)
    {
        IList items = null;
        if (SelectedType == "Drawing") { items = drawings; }
        if (SelectedType == "Document") { items = documents; }

        foreach (FileData item in items)
        {
            try
            {
                string folderpath = System.IO.Path.GetDirectoryName(item.Sökväg);
                Process process = Process.Start("explorer.exe", "\"" + folderpath + "\"");

            }
            catch(Exception e)
            { 
                Debug.WriteLine(e);
            }

        }
    }

    public void AddColor(string color, IList drawings, IList documents, string SelectedType)
    {
        List<FileData> filesToReplace = [];

        IList items = null;
        ObservableCollection<FileData> collection = null;
        if (SelectedType == "Drawing") { items = drawings; collection = Drawings; };
        if (SelectedType == "Document") { items = documents; collection = Documents; };
        
        foreach (FileData item in items) { filesToReplace.Add(item); }
        foreach (FileData file in filesToReplace) 
        {
            int index = collection.IndexOf(file);
            collection[index] = null;
            file.Färg = color;
            collection[index] = file;
        }
    }

    public void ClearAll(IList drawings, IList documents, string SelectedType)
    {
        List<FileData> filesToReplace = [];

        IList items = null;
        ObservableCollection<FileData> collection = null;
        if (SelectedType == "Drawing") { items = drawings; collection = Drawings; };
        if (SelectedType == "Document") { items = documents; collection = Documents; };

        foreach (FileData item in items) { filesToReplace.Add(item); }
        foreach (FileData file in filesToReplace)
        {
            int index = collection.IndexOf(file);
            collection[index] = null;
            file.Färg = "";
            file.Tagg = "";
            collection[index] = file;
        }
    }

    public void AddTag(bool tagmode, IList drawings, IList documents, string SelectedType)
    {

        if (SelectedType == "Drawing") { SetTag(tagmode, drawings, Drawings); };
        if (SelectedType == "Document") { SetTag(tagmode, documents, Documents); };

    }

    public void SetTag(bool tagmode, IList Items, ObservableCollection<FileData> Collection)
    {
        string Tag = "";
        if (tagmode == true) { Tag = user_tag; };
        List<FileData> filesToReplace = [];
        foreach (FileData item in Items) { filesToReplace.Add(item); }
        foreach (FileData file in filesToReplace)
        {
            int index = Collection.IndexOf(file);
            Collection[index] = null;
            file.Tagg = Tag;
            Collection[index] = file;
        }
    }

    public void RemoveFiles(IList files)
    {
        foreach (FileData file in files)
        {
            Globals.storedFiles.Remove(file);
            Drawings.Remove(file);
        }
    }

    public void new_project(string newName)
    {
        Globals.projects.Add(newName);
        UpdateProjectList();

    }

    public void remove_project(int projectIndex)
    {
        if (Globals.projects.Count > 1)
        {
            Globals.storedFiles.RemoveAll(x => x.Uppdrag == Globals.projects[projectIndex]);
            Globals.projects.RemoveAt(projectIndex);
            UpdateProjectList();
        }
    }

    public void rename_project(int projectindex, string newName)
    {
        for (int i = 0; i < Globals.storedFiles.Count; i++)
        {
            if (Globals.storedFiles[i].Uppdrag == Globals.projects[projectindex])
            {
                Globals.storedFiles[i].Uppdrag = newName;
            }
        }
        Globals.projects[projectindex] = newName;
        UpdateProjectList();
    }

    public void RemoveFiles(IList drawings, IList documents, string SelectedType)
    {
        List<FileData> filesToremove = [];
        if (SelectedType == "Drawing")
        {
            foreach (FileData drawing in drawings){filesToremove.Add(drawing);}
            foreach (FileData file in filesToremove)
            {
                Globals.storedFiles.Remove(file);
                Drawings.Remove(file);
            }
        }
        if (SelectedType == "Document")
        {
            foreach (FileData document in documents){filesToremove.Add(document);}
            foreach (FileData file in filesToremove)
            {
                Globals.storedFiles.Remove(file);
                Documents.Remove(file);
            }
        }
    }

    public void UpdateLists(int selectedProject)
    {
        Drawings.Clear();
        Documents.Clear();

        if (Globals.projects.Count() > 0)
        {
            string currentProject = Globals.projects[selectedProject];
            IEnumerable<FileData> filteredDraw = get_filtered_res(currentProject, "Drawing");
            IEnumerable<FileData> filteredDoc = get_filtered_res(currentProject, "Document");

            foreach (FileData file in filteredDraw)
            {
                Drawings.Add(file);
            }

            foreach (FileData file in filteredDoc)
            {
                Documents.Add(file);
            }

            int drawingcount = filteredDraw.Count();
            int documentcount = filteredDoc.Count();

            ProjectMessage = string.Format("Project {0}: {1} Drawings / {2} Documents", currentProject, drawingcount, documentcount);
            OnPropertyChanged("ProjectMessage");
        }

    }

    public void UpdateProjectList()
    {
        Projects.Clear();

        Globals.projects.Sort();

        foreach (string project in Globals.projects)
        {
            Projects.Add(project);
        }
    }

    public string GetCurrentProject(int selectedproject)
    {
        return Globals.projects[selectedproject];
    }

    private IEnumerable<FileData> get_filtered_res(string currentProject, string type)
    {
        IEnumerable<FileData> first = Globals.storedFiles.Where(x => x.Uppdrag == currentProject);
        IEnumerable<FileData> second = first.Where(x => x.Filtyp == type);

        return second.OrderBy(x => x.Namn);
    }

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

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }



}

