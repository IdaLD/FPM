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

namespace Avalon.Views;

public partial class MainView : UserControl
{
    public MainView() 
    {
        InitializeComponent();

        Removefiles.AddHandler(Button.ClickEvent, on_remove_files);

        AddProject.AddHandler(Button.ClickEvent, on_add_project);
        RemoveProject.AddHandler(Button.ClickEvent, on_remove_project);
        RenameProject.AddHandler(Button.ClickEvent, on_rename_project);

        LoadFile.AddHandler(Button.ClickEvent, on_load_files);
        

        DrawingGrid.AddHandler(DataGrid.SelectionChangedEvent, OnDrawingGridSelected);
        DocumentGrid.AddHandler(DataGrid.SelectionChangedEvent, OnDocumentGridSelected);

        ProjectSelection.AddHandler(ComboBox.SelectionChangedEvent, on_ProjectSelectionChange);

    }

    public string SelectedType = null;


    private async void on_load_files(object sender, RoutedEventArgs e)
    {
        var ctx = (MainViewModel)this.DataContext;
        await ctx.LoadFile(this);
        ProjectSelection.SelectedIndex = ProjectSelection.Items.Count-1;
    }

    public void update_projectList()
    {
        var ctx = (MainViewModel)this.DataContext;
        ctx.UpdateProjectList();
    }

    public void update_fileLists(int selectedProject)
    {   
        var ctx = (MainViewModel)this.DataContext;
        ctx.UpdateLists(0);
    }

    private void on_remove_files(object sender, EventArgs e)
    {
        IList drawings      = DrawingGrid.SelectedItems;
        IList documents     = DocumentGrid.SelectedItems;
        int projectindex    = ProjectSelection.SelectedIndex;

        var ctx = (MainViewModel)this.DataContext;
        ctx.AddDrawings(drawings, documents, SelectedType, projectindex);
    }

    private void on_add_project(object sender, EventArgs e)
    {
        string newName = ProjectName.Text.ToString();
        var ctx = (MainViewModel)this.DataContext;
        ctx.new_project(newName);
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

