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

using Avalonia.Controls;
using Avalonia.Themes.Fluent;
using Avalonia.Styling;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Newtonsoft.Json.Bson;
using System.Formats.Asn1;
using Avalonia.Data;
using System.Threading;
using Avalonia.Controls.Shapes;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Avalon.Views;

public partial class MainView : UserControl
{
    public MainView() 
    {
        InitializeComponent();
       
        //MainGrid.Margin = new Thickness(5, 5, 16, 5);
        MainGrid.Margin = new Thickness(5);


        DrawingGrid.AddHandler(DataGrid.DoubleTappedEvent, on_open_file);
        DocumentGrid.AddHandler(DataGrid.DoubleTappedEvent, on_open_file);

        AddProject.AddHandler(Button.ClickEvent, on_add_project);
        AddDrawing.AddHandler(Button.ClickEvent, on_add_drawing);
        AddDocument.AddHandler(Button.ClickEvent, on_add_document);
        FetchMetadata.AddHandler(Button.ClickEvent, on_fetch_full_meta);

        LoadFile.AddHandler(Button.ClickEvent, on_load_file);
        SaveFile.AddHandler(Button.ClickEvent, on_save_file);
        

        DrawingGrid.AddHandler(DataGrid.SelectionChangedEvent, OnDrawingGridSelected);
        DocumentGrid.AddHandler(DataGrid.SelectionChangedEvent, OnDocumentGridSelected);

        ToggleView.AddHandler(Button.ClickEvent, SetTables);
        ColorMode.AddHandler(Button.ClickEvent, ToggleColormode);

        ProjectList.AddHandler(ListBox.TappedEvent, on_project_selected);

        //SelectedProject.AddHandler(Button.PointerEnteredEvent, on_popup);
        //ProjectList.AddHandler(ListBox.PointerExitedEvent, on_popup);

        ColumnList.AddHandler(ListBox.PointerExitedEvent, on_drawingListPopup);
        //Columns.AddHandler(ListBox.PointerEnteredEvent, on_drawingListPopup);
        Columns.AddHandler(ListBox.TappedEvent, on_drawingListPopup);


        

        init_columns();
        
        StatusLabel.Content = "Ready";

        //dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishReadyToRun=true -p:PublishTrimmed=True -p:TrimMode=link --output ./MyTargetFolder Avalon.sln

    }

    public string SelectedType = null;
    public bool ViewMode = false;
    public bool DarkMode = false;
    public string StatusMessage = "Ready";
    public bool PopupStatus = true;
    public bool PopupColumnList_status = true;
    public string TagInput = "";

    private BackgroundWorker bw = new BackgroundWorker();



    private void init_columns()
    {
        int nval = DrawingGrid.Columns.Count();

        for (int i = 0; i < nval; i++)
        {
            DrawingGrid.Columns[i].IsVisible = false;
            DocumentGrid.Columns[i].IsVisible = false;
        }

        Column0.IsChecked = true;
        Column1.IsChecked = true;

        Column5.IsChecked = true;
        Column6.IsChecked = true;
        Column7.IsChecked = true;
        Column8.IsChecked = true;
        Column9.IsChecked = true;

    }

    private void ColumnCheck(object sender, RoutedEventArgs e)
    {
        var item = sender as CheckBox;
        int column = Int32.Parse(item.Tag.ToString());

        DrawingGrid.Columns[column].IsVisible = true;
        DocumentGrid.Columns[column].IsVisible = true;

    }

    private void ColumnUncheck(object sender, RoutedEventArgs e)
    {
        var item = sender as CheckBox;
        int column = Int32.Parse(item.Tag.ToString());

        DrawingGrid.Columns[column].IsVisible = false;
        DocumentGrid.Columns[column].IsVisible = false;

    }

    void on_project_refresh(object sender, EventArgs e)
    {
        ProjectList.SelectedIndex = ProjectList.ItemCount - 1;
        ProjectList.SelectedItem.ToString();
        SelectedProject.Content = ProjectList.SelectedItem.ToString();
        on_ProjectSelectionChange(sender, e);
    }


    void OnMenuOpen(object sender, RoutedEventArgs e)
    {
        on_open_file(sender, e);
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
    
    public void SetTables(object sender, EventArgs e)
    {

        MainGrid.RowDefinitions.Clear();
        GridLength row1 = new GridLength(40);
        GridLength row2 = new GridLength(40);
        GridLength row3 = new GridLength(8, GridUnitType.Star);
        GridLength row4 = new GridLength(5* Convert.ToInt32(ViewMode), GridUnitType.Star);
        MainGrid.RowDefinitions.Add(new RowDefinition(row1));
        MainGrid.RowDefinitions.Add(new RowDefinition(row2));
        MainGrid.RowDefinitions.Add(new RowDefinition(row3));
        MainGrid.RowDefinitions.Add(new RowDefinition(row4));
        
        ViewMode = !ViewMode;

    }

    private void ToggleColormode(object sender, EventArgs e)
    {
        var window = Window.GetTopLevel(this);
        if (DarkMode == false)
        {
            window.RequestedThemeVariant = ThemeVariant.Light;
        }
        if (DarkMode == true)
        {
            window.RequestedThemeVariant = ThemeVariant.Dark;
        }
        DarkMode = !DarkMode;
    }

    public void ToggleGridmode(object sender, RoutedEventArgs e)
    {
        var menuItem = sender as MenuItem;
        string gridselect = menuItem.Tag.ToString();
        DataGrid selectedGrid = null;

        if (SelectedType == "Drawing") { selectedGrid = DrawingGrid; }
        if (SelectedType == "Document") { selectedGrid = DocumentGrid; }

        if (gridselect == "None"){ selectedGrid.GridLinesVisibility = DataGridGridLinesVisibility.None;}
        if (gridselect == "Vertical") { selectedGrid.GridLinesVisibility = DataGridGridLinesVisibility.Vertical; }
        if (gridselect == "Horizontal") { selectedGrid.GridLinesVisibility = DataGridGridLinesVisibility.Horizontal; }
        if (gridselect == "All") { selectedGrid.GridLinesVisibility = DataGridGridLinesVisibility.All; }

    }


    public void on_popup(object sender, RoutedEventArgs e)
    {
        PopupList.IsOpen = PopupStatus;
        PopupStatus = !PopupStatus;
    }

    public void on_drawingListPopup(object sender, RoutedEventArgs e)
    {
        PopupColumnList.IsOpen = PopupColumnList_status;
        PopupColumnList_status = !PopupColumnList_status;
    }

    public void on_project_selected(object sender, RoutedEventArgs e)
    {
        var content = ProjectList.SelectedItem;
        if (content != null) 
        {
            on_popup(sender, e);
            SelectedProject.Content = content.ToString();
        }
        on_ProjectSelectionChange(sender, e);
    }

    public void EditColor(object sender, RoutedEventArgs e)
    {
        var menuItem = sender as MenuItem;
        string color = menuItem.Tag.ToString();

        IList drawings = DrawingGrid.SelectedItems;
        IList documents = DocumentGrid.SelectedItems;

        var ctx = (MainViewModel)this.DataContext;
        ctx.AddColor(color, drawings, documents, SelectedType);
        DrawingGrid.SelectedItem = null;
        DocumentGrid.SelectedItem = null;
    }

    public void on_clear_files(object sender, RoutedEventArgs e)
    {

        IList drawings = DrawingGrid.SelectedItems;
        IList documents = DocumentGrid.SelectedItems;

        var ctx = (MainViewModel)this.DataContext;
        ctx.ClearAll(drawings, documents, SelectedType);
        DrawingGrid.SelectedItem = null;
        DocumentGrid.SelectedItem = null;
    }

    private void on_add_tag(object sender, RoutedEventArgs e)
    {

        bool currentMode = false;
        var tagMode = sender as MenuItem;
        string mode = tagMode.Tag.ToString();
        var TagName = "";

        if (mode == "Add")
        {
            currentMode = true;
        }

        IList drawings = DrawingGrid.SelectedItems;
        IList documents = DocumentGrid.SelectedItems;

        var ctx = (MainViewModel)this.DataContext;
        ctx.AddTag(currentMode, drawings, documents, SelectedType);
        DrawingGrid.SelectedItem = null;
        DocumentGrid.SelectedItem = null;
        
    }

    private void on_add_project(object sender, EventArgs e)
    {
        var Name = ProjectName.Text;
        if (Name != null)
        {
            string newName = Name.ToString();
            var ctx = (MainViewModel)this.DataContext;
            ctx.new_project(newName);
            ProjectName.Clear();
            on_project_refresh(sender, e);
        }
    }

    private void on_remove_project(object sender, RoutedEventArgs e)
    {
        var ctx = (MainViewModel)this.DataContext;
        ctx.remove_project(ProjectList.SelectedIndex);
        on_project_refresh(sender, e);
    }

    private void on_rename_project(object sender, RoutedEventArgs e)
    {

        int currentProject = ProjectList.SelectedIndex;
        string newName = NewProjectName.Text.ToString();

        var ctx = (MainViewModel)this.DataContext;
        ctx.rename_project(currentProject, newName);
        if (currentProject == ProjectList.ItemCount - 1)
        {
            Debug.WriteLine("Last");
            SelectedProject.Content = newName;
        }
    }

    private void on_add_document(object sender, EventArgs e)
    {
        int currentProject = ProjectList.SelectedIndex;
        var ctx = (MainViewModel)this.DataContext;
        ctx.AddFile("Document", currentProject, this);
    }

    private void on_add_drawing(object sender, EventArgs e)
    {
        int currentProject = ProjectList.SelectedIndex;
        var ctx = (MainViewModel)this.DataContext;
        ctx.AddFile("Drawing", currentProject, this);
    }

    private void on_fetch_single_meta(object sender, RoutedEventArgs e)
    {
        ProgressStatus.Content = "Fetching Metadata";
        IList drawings = DrawingGrid.SelectedItems;
        IList documents = DocumentGrid.SelectedItems;

        var ctx = (MainViewModel)this.DataContext;
        ctx.SelectFiles(true, drawings, documents, SelectedType);
        on_fetch_metadata();

        Debug.WriteLine(ctx.GetNrSelectedFiles());
    }
    private void on_fetch_full_meta(object sender, RoutedEventArgs e)
    {
        ProgressStatus.Content = "Fetching Metadata";

        var ctx = (MainViewModel)this.DataContext;
        ctx.SelectFiles(false, null, null, null);
        
        on_fetch_metadata();
    }


    private void on_fetch_metadata()
    {
        //StatusLabel.Content = "Fetching metadata...";

        var ctx = (MainViewModel)this.DataContext;


        //BackgroundWorker bw = new BackgroundWorker();
        bw.DoWork += Bw_DoWork;
        //bw.ReportProgress += Bw_progress;
        bw.WorkerReportsProgress = true;
        bw.ProgressChanged += Bw_progress;
        bw.RunWorkerCompleted += Bw_RunWorkerCompleted;

        bw.RunWorkerAsync(ctx);
        
    }

    private void Bw_DoWork(object sender, DoWorkEventArgs e)
    {
        var ctx = e.Argument as MainViewModel;
        int nPaths = ctx.GetNrSelectedFiles();

        for (int k = 0; k < nPaths; k++)
        {
            Debug.WriteLine("iter: " + k);
            ctx.GetMetadata(k);

            int percentage = (k + 1) * 100 / nPaths;
            bw.ReportProgress(percentage);
        }
    }

    private void Bw_progress(object sender, ProgressChangedEventArgs e)
    {
        ProgressBar.Value = e.ProgressPercentage;
        //Debug.WriteLine(e.ProgressPercentage.ToString());
        
    }

    private void Bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
        var ctx = (MainViewModel)this.DataContext;
        ctx.SetMetadata();
        ProgressStatus.Content = "Fetching Complete";
        
    }





    private void on_open_path(object sender, RoutedEventArgs e)
    {
        if (StatusLabel.Content == "Ready")
        {
            StatusLabel.Content = "Opening path";
            IList drawings = DrawingGrid.SelectedItems;
            IList documents = DocumentGrid.SelectedItems;

            var ctx = (MainViewModel)this.DataContext;
            ctx.OpenPath(drawings, documents, SelectedType);

            StatusLabel.Content = "Ready";
        }

    }

    private void on_open_file(object sender, EventArgs e)
    {
        if (StatusLabel.Content == "Ready")
        {
            StatusLabel.Content = "Opening file";
            IList drawings = DrawingGrid.SelectedItems;
            IList documents = DocumentGrid.SelectedItems;

            var ctx = (MainViewModel)this.DataContext;
            ctx.OpenFile(drawings, documents, SelectedType,"PDF");

            StatusLabel.Content = "Ready";
        }
    }

    private void on_open_metafile(object sender, RoutedEventArgs e)
    {
        if (StatusLabel.Content == "Ready")
        {
            StatusLabel.Content = "Opening metafile";
            IList drawings = DrawingGrid.SelectedItems;
            IList documents = DocumentGrid.SelectedItems;

            var ctx = (MainViewModel)this.DataContext;
            ctx.OpenFile(drawings, documents, SelectedType,"MD");

            StatusLabel.Content = "Ready";
        }
    }

    private async void on_load_file(object sender, RoutedEventArgs e)
    {
        StatusLabel.Content = "Loading file";
        var ctx = (MainViewModel)this.DataContext;
        await ctx.LoadFile(this);
        on_project_refresh(sender, e);
        
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

    private void on_remove_files(object sender, RoutedEventArgs e)
    {
        IList drawings      = DrawingGrid.SelectedItems;
        IList documents     = DocumentGrid.SelectedItems;
        int projectindex    = ProjectList.SelectedIndex;

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
        ctx.UpdateLists(ProjectList.SelectedIndex);
    }
}

