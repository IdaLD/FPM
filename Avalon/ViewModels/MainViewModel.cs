using Avalon.Views;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Microsoft.CodeAnalysis.CSharp;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;

namespace Avalon.ViewModels;


public class MainViewModel : ViewModelBase
{
    public MainViewModel()
    {

    }

    public ObservableCollection<File> Drawings { get; } = new();
    public ObservableCollection<File> Documents { get; } = new();
    public ObservableCollection<IList> Projects { get; } = new();

    public async void LoadFile(Avalonia.Visual window) 
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
            await using var stream      = await files[0].OpenReadAsync();
            using var streamReader      = new StreamReader(stream);
            string fileContent          = await streamReader.ReadToEndAsync();

            List<File> getFiles         = JsonConvert.DeserializeObject<List<File>>(fileContent);
            List<string> getProjects    = getFiles.Select(x => x.Project).Distinct().ToList();

            ViewModelBase.Globals.storedFiles         = getFiles;
            ViewModelBase.Globals.projects            = getProjects;

            UpdateLists();
            
        }
    }

    public void RemoveFiles(IList files)
    {
        foreach (File file in files)
        {
            Globals.storedFiles.Remove(file);
        }
        UpdateLists();
    }

    public void AddDrawings(IList drawings, IList documents, string SelectedType)
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

        UpdateLists();

    }

    private void SetCurrentType()
    {
        Debug.WriteLine("Selection changed");

    }

    private void UpdateLists()
    {
        Drawings.Clear();
        Documents.Clear();
        string currentProject = "VLUE10";
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

    private IEnumerable<File> get_filtered_res(string currentProject, string type)
    {
        IEnumerable<File> first = ViewModelBase.Globals.storedFiles.Where(x => x.Project == currentProject);
        IEnumerable<File> second = first.Where(x => x.Type == type);

        return second.OrderBy(x => x.Name);
    }
}

