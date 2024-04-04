using Avalonia.Controls;
using System.Collections.Generic;
using System;
using Newtonsoft.Json;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using System.Linq;
using System.IO;

namespace Avalon.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    public async void LoadFileButton_Clicked(object sender, RoutedEventArgs args)
    {
        var topLevel = TopLevel.GetTopLevel(this);

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


            List<Files> getFiles = JsonConvert.DeserializeObject<List<Files>>(fileContent);
            List<string> getProjects = getFiles.Select(x => x.Project).Distinct().ToList();
        }
    }
}

