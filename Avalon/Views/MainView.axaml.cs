using Avalonia.Controls;
using System.Collections.Generic;
using System;
using Newtonsoft.Json;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using System.Linq;
using System.IO;
using Avalon.ViewModels;
using System.Diagnostics;
using Avalonia.Controls.Primitives;
using System.Collections;
using Avalonia.Media;
using Avalonia.Collections;
using System.Data;
using System.Reflection;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Avalon.Views;

public partial class MainView : UserControl
{
    public MainView() 
    {
        InitializeComponent();

        

        ProjectSelection.AddHandler(ComboBox.LoadedEvent, on_combo_startup);
        DrawingGrid.AddHandler(DataGrid.DoubleTappedEvent, on_open_file);
        DocumentGrid.AddHandler(DataGrid.DoubleTappedEvent, on_open_file);

        AddProject.AddHandler(Button.ClickEvent, on_add_project);
        RemoveProject.AddHandler(Button.ClickEvent, on_remove_project);
        RenameProject.AddHandler(Button.ClickEvent, on_rename_project);

        AddDrawing.AddHandler(Button.ClickEvent, on_add_drawing);
        AddDocument.AddHandler(Button.ClickEvent, on_add_document);

        Removefiles.AddHandler(Button.ClickEvent, on_remove_files);
        OpenFile.AddHandler(Button.ClickEvent, on_open_file);

        LoadFile.AddHandler(Button.ClickEvent, on_load_file);
        SaveFile.AddHandler(Button.ClickEvent, on_save_file);
        

        DrawingGrid.AddHandler(DataGrid.SelectionChangedEvent, OnDrawingGridSelected);
        DocumentGrid.AddHandler(DataGrid.SelectionChangedEvent, OnDocumentGridSelected);

        ProjectSelection.AddHandler(ComboBox.SelectionChangedEvent, on_ProjectSelectionChange);

        White.AddHandler(Button.ClickEvent, EditColor);
        Yellow.AddHandler(Button.ClickEvent, EditColor);
        Green.AddHandler(Button.ClickEvent, EditColor);
        Blue.AddHandler(Button.ClickEvent, EditColor);
        Red.AddHandler(Button.ClickEvent, EditColor);
        Magenta.AddHandler(Button.ClickEvent, EditColor);




    }

    public string SelectedType = null;
    public string StatusMessage = "Ready";
    private void on_combo_startup(object sender,EventArgs e)
    {
        ProjectSelection.SelectedIndex = ProjectSelection.ItemCount - 1;
        StatusLabel.Content = "Ready";
    }
    private void DataGrid_OnLoadingRow(object? sender, DataGridRowEventArgs e)
    {
        var dataObject = e.Row.DataContext as FileData;
        e.Row.Classes.Clear();

        if (dataObject != null && dataObject.Color == "White") { e.Row.Classes.Clear(); }
        if (dataObject != null && dataObject.Color == "Yellow"){e.Row.Classes.Add("Yellow");}
        if (dataObject != null && dataObject.Color == "Green"){e.Row.Classes.Add("Green");}
        if (dataObject != null && dataObject.Color == "Blue") { e.Row.Classes.Add("Blue"); }
        if (dataObject != null && dataObject.Color == "Red") { e.Row.Classes.Add("Red"); }
        if (dataObject != null && dataObject.Color == "Magenta") { e.Row.Classes.Add("Magenta"); }

    }

    public void EditColor(object sender, EventArgs e)
    {
        var button = sender as Button;
        string color = button.Name.ToString();

        IList drawings = DrawingGrid.SelectedItems;
        IList documents = DocumentGrid.SelectedItems;

        var ctx = (MainViewModel)this.DataContext;
        ctx.AddColor(color, drawings, documents, SelectedType);
        DrawingGrid.SelectedItem = null;
    }

    private void on_add_project(object sender, EventArgs e)
    {
        string newName = ProjectName.Text.ToString();
        var ctx = (MainViewModel)this.DataContext;
        ctx.new_project(newName);
        ProjectName.Clear();
        ProjectSelection.SelectedIndex = ProjectSelection.ItemCount - 1;
    }

    private void on_remove_project(object sender, EventArgs e)
    {
        var ctx = (MainViewModel)this.DataContext;
        ctx.remove_project(ProjectSelection.SelectedIndex);
        ProjectSelection.SelectedIndex = ProjectSelection.ItemCount - 1;
    }

    private void on_rename_project(object sender, EventArgs a)
    {
        int currentProject = ProjectSelection.SelectedIndex;
        string newName = ProjectName.Text.ToString();
        var ctx = (MainViewModel)this.DataContext;
        ctx.rename_project(currentProject, newName);
        ProjectSelection.SelectedIndex = currentProject;
    }

    private void on_add_document(object sender, EventArgs e)
    {
        int currentProject = ProjectSelection.SelectedIndex;
        var ctx = (MainViewModel)this.DataContext;
        ctx.AddFile("Document", currentProject, this);
    }

    private void on_add_drawing(object sender, EventArgs e)
    {
        int currentProject = ProjectSelection.SelectedIndex;
        var ctx = (MainViewModel)this.DataContext;
        ctx.AddFile("Drawing", currentProject, this);
    }

    private void on_open_file(object sender, EventArgs e)
    {
        if (StatusLabel.Content == "Ready")
        {
            StatusLabel.Content = "Opening file";
            IList drawings = DrawingGrid.SelectedItems;
            IList documents = DocumentGrid.SelectedItems;

            var ctx = (MainViewModel)this.DataContext;
            ctx.OpenFile(drawings, documents, SelectedType);

            StatusLabel.Content = "Ready";
        }
        
    }
    private async void on_load_file(object sender, RoutedEventArgs e)
    {
        StatusLabel.Content = "Loading file";
        var ctx = (MainViewModel)this.DataContext;
        await ctx.LoadFile(this);
        ProjectSelection.SelectedIndex = ProjectSelection.Items.Count-1;
        StatusLabel.Content = "Ready";
    }

    private async void on_save_file(object sender, RoutedEventArgs e)
    {
        StatusLabel.Content = "Saving file";
        var ctx = (MainViewModel)this.DataContext;
        await ctx.SaveFile(this);
        StatusLabel.Content = "Ready";
    }

    public void update_projectList()
    {
        var ctx = (MainViewModel)this.DataContext;
        ctx.UpdateProjectList();
    }

    public void update_fileLists(int selectedProject)
    {   
        var ctx = (MainViewModel)this.DataContext;
        ctx.UpdateLists(selectedProject);
    }

    private void on_remove_files(object sender, EventArgs e)
    {
        IList drawings      = DrawingGrid.SelectedItems;
        IList documents     = DocumentGrid.SelectedItems;
        int projectindex    = ProjectSelection.SelectedIndex;

        var ctx = (MainViewModel)this.DataContext;
        ctx.RemoveFiles(drawings, documents, SelectedType);
    }


    private void OnDrawingGridSelected(object sender, EventArgs e)
    {
        SelectedType = "Drawing";
    }

    private void OnDocumentGridSelected(object sender, EventArgs e)
    {
        SelectedType = "Document";
    }

    private void on_ProjectSelectionChange(object sender, EventArgs e)
    {
        var ctx = (MainViewModel)this.DataContext;
        ctx.UpdateLists(ProjectSelection.SelectedIndex);
    }
}

