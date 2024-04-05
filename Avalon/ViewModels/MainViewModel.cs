using Avalon.Views;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using Microsoft.CodeAnalysis.CSharp;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Avalon.ViewModels;


public class MainViewModel : ViewModelBase
{
    public MainViewModel()
    {

    }

    public ObservableCollection<File> Drawings { get; } = new();
    public ObservableCollection<File> Documents { get; } = new();
    public ObservableCollection<string> Projects { get; } = new();

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

            List<File> getFiles = JsonConvert.DeserializeObject<List<File>>(fileContent);
            List<string> getProjects = getFiles.Select(x => x.Project).Distinct().ToList();

            Globals.storedFiles = getFiles;
            Globals.projects = getProjects;

            UpdateProjectList();
        }

    }



    public void RemoveFiles(IList files)
    {
        foreach (File file in files)
        {
            Globals.storedFiles.Remove(file);
        }
        UpdateLists(0);
    }

    public void new_project(string newName)
    {
        Globals.projects.Add(newName);
        UpdateProjectList();

    }

    public void remove_project(int projectIndex)
    {
        Globals.storedFiles.RemoveAll(x => x.Project == Globals.projects[projectIndex]);
        Globals.projects = Globals.storedFiles.Select(x => x.Project).Distinct().ToList();
        UpdateProjectList(); 
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
        Globals.projects = Globals.storedFiles.Select(x => x.Project).Distinct().ToList();
        UpdateProjectList();
    }

    public void AddDrawings(IList drawings, IList documents, string SelectedType, int projectIndex)
    {
        if (SelectedType == "Drawing")
        {
            foreach (File drawing in drawings)
            {
                Globals.storedFiles.Remove(drawing);
            }
        }
        if (SelectedType == "Document")
        {
            foreach (File document in documents)
            {
                Globals.storedFiles.Remove(document);
            }
        }

        UpdateLists(projectIndex);

    }

    public void UpdateLists(int selectedProject)
    {
        Drawings.Clear();
        Documents.Clear();

        if (Globals.projects.Count() > 0)
        {
            string currentProject = Globals.projects[selectedProject];
            IEnumerable<File> filteredDraw = get_filtered_res(currentProject, "Drawing");
            IEnumerable<File> filteredDoc = get_filtered_res(currentProject, "Document");

            foreach (File file in filteredDraw)
            {
                Drawings.Add(file);
            }

            foreach (File file in filteredDoc)
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

    private IEnumerable<File> get_filtered_res(string currentProject, string type)
    {
        IEnumerable<File> first = Globals.storedFiles.Where(x => x.Project == currentProject);
        IEnumerable<File> second = first.Where(x => x.Type == type);

        return second.OrderBy(x => x.Name);
    }
}

