using Avalon.Views;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Text;
using System;
using Avalonia.Controls.Shapes;
using CommunityToolkit.Mvvm.ComponentModel;
using Avalonia.Media;
using Avalonia.Data.Converters;
using System.Globalization;
using System.ComponentModel;
using System.Collections.Frozen;
using System.Runtime.CompilerServices;
using Microsoft.VisualBasic;
using Newtonsoft.Json.Converters;
using static System.Net.WebRequestMethods;
using Avalonia.Threading;
using System.Threading;

namespace Avalon.ViewModels;


public class MainViewModel : ViewModelBase
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

    public string user_tag { get; set; }
    public List<string[]> metastore = new List<string[]>();

    public async Task LoadFile(Avalonia.Visual window)
    {
        Debug.WriteLine("Loading file");
        var topLevel = TopLevel.GetTopLevel(window);
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Load Save File",
            AllowMultiple = false
        });

        if (files.Count > 0)
        {
            await using var stream = await files[0].OpenReadAsync();
            using var streamReader = new StreamReader(stream);
            string fileContent = await streamReader.ReadToEndAsync();

            List<FileData> getFiles = JsonConvert.DeserializeObject<List<FileData>>(fileContent);
            List<string> getProjects = getFiles.Select(x => x.Project).Distinct().ToList();

            Globals.storedFiles = getFiles;
            Globals.projects = getProjects;

            var properties = typeof(FileData).GetProperties().ToList();
            foreach (var property in properties)
            {
                string val = property.Name;
                Debug.WriteLine(val);
                Properties.Add(val);
            }
            UpdateProjectList();
        }
    }

    public async Task SaveFile(Avalonia.Visual window)
    {
        var topLevel = TopLevel.GetTopLevel(window);

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save File",
        });

        if (file is not null)
        {
            await using var stream = await file.OpenWriteAsync();
            using var streamWriter = new StreamWriter(stream);
            var data = JsonConvert.SerializeObject(Globals.storedFiles);
            await streamWriter.WriteLineAsync(data);
        }
    }

    public async Task AddFile(string type, int selectedProject, Avalonia.Visual window)
    {
        var topLevel = TopLevel.GetTopLevel(window);
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Add File",
            FileTypeFilter = new[] {FilePickerFileTypes.Pdf},
            AllowMultiple = true
        });

        foreach (var file in files)
        {
            string path = file.Path.LocalPath;

            Globals.storedFiles.Add(new FileData
            {
                Name = System.IO.Path.GetFileNameWithoutExtension(path),
                Project = Globals.projects[selectedProject],
                Type = type,
                Path = path
            });
        }
        UpdateLists(selectedProject);
    }


    public void SetMetadata(int selectedProject)
    {
        IEnumerable<FileData> filteredDraw = get_filtered_res(Globals.projects[selectedProject], "Drawing");
        IEnumerable<FileData> filteredDoc = get_filtered_res(Globals.projects[selectedProject], "Document");

        int iter = 0;
        foreach (FileData file in filteredDraw)
        {
            string path = file.Path;
            Debug.WriteLine(iter);
            string[] md = metastore[iter];
            //string[] md = GetMetadata(path);

            int index = Drawings.IndexOf(file);

            file.Handling = md[0];
            file.Status = md[1];
            file.Date = md[2];
            file.DrawType = md[3];
            file.Descr1 = md[4];
            file.Descr2 = md[5];
            file.Descr3 = md[6];
            file.Descr4 = md[7];
            file.Rev = md[8];
            file.Path = path;

            Drawings[index] = null;
            Drawings[index] = file;

            iter++;
        }


        
    }

    public void GetMetadata(int selectedProject)
    {

        IEnumerable<FileData> filteredDraw = get_filtered_res(Globals.projects[selectedProject], "Drawing");
        IEnumerable<FileData> filteredDoc = get_filtered_res(Globals.projects[selectedProject], "Document");

        List<string> mdDrawingPath = new List<string>();
        List<string> mdDocumentPath = new List<string>();
       

        foreach (FileData file in filteredDraw)
        {
            mdDrawingPath.Add(file.Path.ToString());
        }
        foreach (FileData file in filteredDoc)
        {
            mdDocumentPath.Add(file.Path.ToString());
        }

        string[] tags = ["Handlingstyp = ", "Granskningsstatus = ", "Datum = ", "Ritningstyp = ", "Beskrivning1 = ", "Beskrivning2 = ", "Beskrivning3 = ", "Beskrivning4 = ", "Revidering = "];
        int ntags = tags.Count();

        List<string[]> metadata = new List<string[]>();

        foreach (string path in mdDrawingPath)
        {
            string[] description = new string[ntags];
            try
            {
                string[] lines = System.IO.File.ReadAllLines(path + ".md", Encoding.GetEncoding("ISO-8859-1"));

                int iter = 1;
                foreach (string line in lines)
                {
                    if (line == "[Metadata]")
                    {
                        break;
                    }
                    iter++;
                }


                int start = iter;

                for (int i = start; i < 50 + start; i++)
                {
                    string line = lines[i];

                    if (!line.StartsWith("TRVNR"))
                    {
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
                    else
                    {
                        break;
                    }

                }
                metastore.Add(description);
            }
            catch (Exception)
            {
                metastore.Add(["", "", "", "", "", "", "", "", ""]);
            }
            
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
                    psi.FileName = drawing.Path + ending;
                    psi.UseShellExecute = true;
                    Process.Start(psi);
                }
            }
            if (SelectedType == "Document")
            {
                foreach (FileData document in documents)
                {
                    ProcessStartInfo psi = new ProcessStartInfo();
                    psi.FileName = document.Path + ending;
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
            Debug.WriteLine(item.Path);
            Process.Start("explorer.exe", System.IO.Path.GetDirectoryName(item.Path));
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
            file.Color = color;
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
            file.Color = "";
            file.UserTag = "";
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
            file.UserTag = Tag;
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
        if (Globals.projects.Count > 0)
        {
            Globals.storedFiles.RemoveAll(x => x.Project == Globals.projects[projectIndex]);
            Globals.projects.RemoveAt(projectIndex);
            UpdateProjectList();
        }
    }

    public void rename_project(int projectindex, string newName)
    {
        for (int i = 0; i < Globals.storedFiles.Count; i++)
        {
            if (Globals.storedFiles[i].Project == Globals.projects[projectindex])
            {
                Globals.storedFiles[i].Project = newName;
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
        }
    }

    public void UpdateProjectList()
    {
        Projects.Clear();
        if (Globals.projects.Count() > 0)
        {
            foreach (string project in Globals.projects)
            {
                Projects.Add(project);
            }
        }
        else
        {
            Projects.Add("New Project");
        }
    }

    private IEnumerable<FileData> get_filtered_res(string currentProject, string type)
    {
        IEnumerable<FileData> first = Globals.storedFiles.Where(x => x.Project == currentProject);
        IEnumerable<FileData> second = first.Where(x => x.Type == type);

        return second.OrderBy(x => x.Name);
    }
}

