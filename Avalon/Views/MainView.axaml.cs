using Avalonia.Controls;
using System;
using Avalonia.Interactivity;
using System.Linq;
using Avalon.ViewModels;
using System.Collections;
using Avalonia;
using System.ComponentModel;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Input;
using Material.Styles.Themes;
using System.Runtime.CompilerServices;


namespace Avalon.Views;

public partial class MainView : UserControl
{
    public MainView() 
    {
        InitializeComponent();

        DrawingGrid.AddHandler(DataGrid.DoubleTappedEvent, on_open_file);
        DocumentGrid.AddHandler(DataGrid.DoubleTappedEvent, on_open_file);

        FetchMetadata.AddHandler(Button.ClickEvent, on_fetch_full_meta);

        DrawingGrid.AddHandler(DataGrid.SelectionChangedEvent, OnDrawingGridSelected);
        DocumentGrid.AddHandler(DataGrid.SelectionChangedEvent, OnDocumentGridSelected);

        DrawingGrid.AddHandler(DataGrid.SelectionChangedEvent, on_preview);
        DocumentGrid.AddHandler(DataGrid.SelectionChangedEvent, on_preview);


        ProjectList.AddHandler(ListBox.SelectionChangedEvent, on_project_selected);
        Lockedstatus.AddHandler(ToggleSwitch.IsCheckedChangedEvent, on_lock);
        DrawingGrid.AddHandler(DataGrid.LoadedEvent, init_startup);

        PreviewToggle.AddHandler(ToggleSwitch.IsCheckedChangedEvent, on_toggle_preview);

        Preview.AddHandler(Viewbox.PointerWheelChangedEvent, on_preview_zoom);
        Preview.AddHandler(Viewbox.PointerWheelChangedEvent, on_scroll_preview);

        Preview.AddHandler(Viewbox.PointerPressedEvent, on_pan_start);
        Preview.AddHandler(Viewbox.PointerMovedEvent, on_preview_pan);
        Preview.AddHandler(Viewbox.PointerReleasedEvent, on_pan_end);

        ScrollSlider.AddHandler(Slider.ValueChangedEvent, on_select_page);
        

        init_columns();
        init_MetaWorker();
        init_PreviewWorker();
        setup_preview_transform();

        StatusLabel.Content = "Ready";

    }



    public string SelectedType = null;
    public int SelectedIndex = 0;

    public string StatusMessage = "Ready";
    public bool PopupStatus = true;
    public bool PopupColumnList_status = true;
    public string TagInput = "";

    public bool previewMode = false;
    public bool preview_pan = false;

    public double x_start = 0f;
    public double y_start = 0f;

    public double pw_scale = 1f;
    public string pw_mode = "Scroll";

    private TransformGroup trGrp;
    private TranslateTransform trTns;
    private ScaleTransform trScl;
    public TransformGroup transform = new TransformGroup();

    private BackgroundWorker MetaWorker = new BackgroundWorker();
    private BackgroundWorker PreviewWorker = new BackgroundWorker();

    private bool PreviewWorker_busy = false;

    public MainViewModel ctx = null;

    public void get_datacontext()
    {
        ctx = (MainViewModel)this.DataContext;
    }

    public void on_theme_dark(object sender, RoutedEventArgs e)
    {
        var MaterialThemeStyles = Application.Current!.LocateMaterialTheme<MaterialTheme>();
        MaterialThemeStyles.BaseTheme = Material.Styles.Themes.Base.BaseThemeMode.Dark;
    }

    public void on_theme_light(object sender, RoutedEventArgs e)
    {
        var MaterialThemeStyles = Application.Current!.LocateMaterialTheme<MaterialTheme>();
        MaterialThemeStyles.BaseTheme = Material.Styles.Themes.Base.BaseThemeMode.Light;
    }

    public void Border_PointerPressed(object sender, RoutedEventArgs args)
    {
        var ctl = sender as Control;
        if (ctl != null)
        {
            FlyoutBase.ShowAttachedFlyout(ctl);
        }
    }

    private void init_startup(object sender, RoutedEventArgs e)
    {
        get_datacontext();
        Lockedstatus.IsChecked = true;

        try
        {
            string path = "C:\\FIlePathManager\\Projects.json";
            ctx.read_savefile(path);
            on_project_refresh(sender, e);
        }
        catch { }
    }

    private void on_toggle_preview(object sender, RoutedEventArgs e)
    {
        previewMode = !previewMode;

        float a = 1;
        float b = 0;

        if (previewMode == true) 
        {
            b = 1.5f;
        }

        MainGrid.ColumnDefinitions.Clear();
        GridLength clmn1 = new GridLength(a, GridUnitType.Star);
        GridLength clmn2 = new GridLength(b, GridUnitType.Star);

        MainGrid.ColumnDefinitions.Add(new ColumnDefinition(clmn1));
        MainGrid.ColumnDefinitions.Add(new ColumnDefinition(clmn2));

        if (previewMode == false)
        {
            ctx.clear_preview_file();
        }

    }

    private string set_preview_file()
    {
        FileData drawing = (FileData)DrawingGrid.SelectedItem;
        FileData document = (FileData)DocumentGrid.SelectedItem;

        string filepath = "";
        if (SelectedType == "Drawing") { filepath = drawing.Sökväg; }
        if (SelectedType == "Document") {filepath = document.Sökväg; } 

        return filepath;
    }
    private void init_PreviewWorker()
    {
        PreviewWorker.DoWork += PreviewWorker_DoWork;
        PreviewWorker.RunWorkerCompleted += PreviewWorker_RunWorkerCompleted;
    }

    private void PreviewWorker_DoWork(object sender, DoWorkEventArgs e)
    {
        int QFak = (int)e.Argument;
        string filepath = set_preview_file();

        ctx.create_preview_file(filepath, QFak);
        ctx.preview_page(0);
    }

    private void PreviewWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
        
        PreviewWorker_busy = false;
    }

    private void on_preview(object sender, RoutedEventArgs e)
    {
        if (previewMode == true)
        {
            int QFak = (int)PreviewQuality.Value;

            if (PreviewWorker_busy != true)
            {
                PreviewWorker_busy = true;
                PreviewWorker.RunWorkerAsync(QFak);

            }
        }
    }

    private void setup_preview_transform()
    {
        trTns = new TranslateTransform(0, 0);
        trScl = new ScaleTransform(1, 1);

        trGrp = new TransformGroup();
        trGrp.Children.Add(trTns);
        trGrp.Children.Add(trScl);
    }

    private void reset_preview_transform(object sender, RoutedEventArgs e)
    {
        trScl.ScaleX = 1;
        trScl.ScaleY = 1;
        trTns.X = 0;
        trTns.Y = 0;
        pw_scale = 1f;

        Previewer.RenderTransform = trGrp;
    }

    private void on_scroll_mode(object sender, RoutedEventArgs e)
    {
        pw_mode = "Scroll";
        ScrollMode.Background = Avalonia.Media.Brushes.Orange;
        ZoomMode.Background = Avalonia.Media.Brushes.DarkGray;
    }

    private void on_zoom_mode(object sender, RoutedEventArgs e)
    {
        pw_mode = "Zoom";
        ScrollMode.Background = Avalonia.Media.Brushes.DarkGray;
        ZoomMode.Background = Avalonia.Media.Brushes.Orange;
    }

    private void on_scroll_preview(object sender, PointerWheelEventArgs args)
    {
    
        if (pw_mode == "Scroll")
        {
            Vector mode = args.Delta;

            if (mode.Y > 0)
            {
                ctx.previous_preview_page();
            }
            if (mode.Y < 0)
            {
                ctx.next_preview_page();
            }
        }
    }

    private void on_select_page(object sender, RoutedEventArgs e)
    {
        if (ScrollSlider.IsPointerOver == true)
        {
            ctx.selected_page((int)ScrollSlider.Value - 1);
        }
    }

    private void on_pan_start(object sender, PointerEventArgs args)
    {
        preview_pan = true;

        double dx = 0;
        double dy = 0;

        if (Previewer.RenderTransform != null)
        {
            dx = trTns.X;
            dy = trTns.Y;
        }

        x_start = args.GetPosition(null).X - dx;
        y_start = args.GetPosition(null).Y - dy;

    }

    private void on_pan_end(object sender, PointerEventArgs args)
    {
        preview_pan = false;
    }

    private void on_preview_pan(object sender, PointerEventArgs args)
    {
        if (preview_pan == true)
        {
            trTns.X = args.GetPosition(null).X - x_start;
            trTns.Y = args.GetPosition(null).Y - y_start;

            Previewer.RenderTransform = trGrp;
        }
    }

    private void on_preview_zoom(object sender, PointerWheelEventArgs args)
    {

        if (pw_mode == "Zoom")
        {

            Vector mode = args.Delta;

            if (mode.Y > 0)
            {
                pw_scale = pw_scale * 1.05;
                trScl.ScaleX = trScl.ScaleY = pw_scale;
                Previewer.RenderTransform = trGrp;
            }

            else if (mode.Y < 0 && pw_scale > 1)
            {
                pw_scale = pw_scale * 0.95;
                trScl.ScaleX = trScl.ScaleY = pw_scale;
                Previewer.RenderTransform = trGrp;
            }
        }
    }



    private void on_lock(object sender, EventArgs e)
    {
        if (Lockedstatus.IsChecked == true)
        {
            RemoveProjectMenu.IsEnabled = false;

            ContextMenu Menu = this.Resources["Menu"] as ContextMenu;
            MenuItem removeMenu = Menu.Items[3] as MenuItem;
            removeMenu.IsEnabled = false;
        }
        if (Lockedstatus.IsChecked == false)
        {
            RemoveProjectMenu.IsEnabled = true;

            ContextMenu Menu = this.Resources["Menu"] as ContextMenu;
            MenuItem removeMenu = Menu.Items[3] as MenuItem;
            removeMenu.IsEnabled = true;
        }
    }

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
        ProjectList.SelectedIndex = SelectedIndex = 0;
        SelectedProject.Content = ctx.GetCurrentProject(SelectedIndex);
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

        if (dataObject != null && dataObject.Färg == "") { e.Row.Classes.Clear(); }
        if (dataObject != null && dataObject.Färg == "Yellow"){e.Row.Classes.Add("Yellow");}
        if (dataObject != null && dataObject.Färg == "Orange") { e.Row.Classes.Add("Orange"); }
        if (dataObject != null && dataObject.Färg == "Brown") { e.Row.Classes.Add("Brown"); }
        if (dataObject != null && dataObject.Färg == "Green"){e.Row.Classes.Add("Green");}
        if (dataObject != null && dataObject.Färg == "Blue") { e.Row.Classes.Add("Blue"); }
        if (dataObject != null && dataObject.Färg == "Red") { e.Row.Classes.Add("Red"); }
        if (dataObject != null && dataObject.Färg == "Magenta") { e.Row.Classes.Add("Magenta"); }

    }
    
    public void toggle_table(object sender, RoutedEventArgs e)
    {
        var menuItem = sender as MenuItem;
        string mode = menuItem.Tag.ToString();

        int a = 1;
        int b = 1;

        if (mode == "Both") { a = 5; b = 3;}
        if (mode == "Drawings") { a = 1; b = 0; }
        if (mode == "Documents") {  a = 0; b = 1; }


        TableGrid.RowDefinitions.Clear();
        GridLength row3 = new GridLength(a, GridUnitType.Star);
        GridLength row4 = new GridLength(b, GridUnitType.Star);
        TableGrid.RowDefinitions.Add(new RowDefinition(row3));
        TableGrid.RowDefinitions.Add(new RowDefinition(row4));
        
    }

    public void on_project_selected(object sender, RoutedEventArgs e)
    {
        var content = ProjectList.SelectedItem;
        if (content != null) 
        {
            PreviewToggle.IsChecked = false;

            SelectedIndex = ProjectList.SelectedIndex;
            SelectedProject.Content = content.ToString();
            NewProjectName.Text = content.ToString();

            on_ProjectSelectionChange(sender, e);
            on_update_columns(sender, e);
        }
    }

    public void EditColor(object sender, RoutedEventArgs e)
    {
        var menuItem = sender as MenuItem;
        string color = menuItem.Tag.ToString();

        IList drawings = DrawingGrid.SelectedItems;
        IList documents = DocumentGrid.SelectedItems;

        ctx.AddColor(color, drawings, documents, SelectedType);
        DrawingGrid.SelectedItem = null;
        DocumentGrid.SelectedItem = null;
    }

    public void on_clear_files(object sender, RoutedEventArgs e)
    {
        IList drawings = DrawingGrid.SelectedItems;
        IList documents = DocumentGrid.SelectedItems;

        ctx.ClearAll(drawings, documents, SelectedType);
        DrawingGrid.SelectedItem = null;
        DocumentGrid.SelectedItem = null;
    }

    private void on_add_tag(object sender, RoutedEventArgs e)
    {

        bool currentMode = false;
        var tagMode = sender as MenuItem;
        string mode = tagMode.Tag.ToString();

        if (mode == "Add")
        {
            currentMode = true;
        }

        IList drawings = DrawingGrid.SelectedItems;
        IList documents = DocumentGrid.SelectedItems;

        ctx.AddTag(currentMode, drawings, documents, SelectedType);
        DrawingGrid.SelectedItem = null;
        DocumentGrid.SelectedItem = null;
    }

    private void on_add_project(object sender, RoutedEventArgs e)
    {
        var Name = ProjectName.Text;
        if (Name != null)
        {
            string newName = Name.ToString();
            ctx.new_project(newName);
            ProjectName.Clear();
        }
    }

    private void on_remove_project(object sender, RoutedEventArgs e)
    {
        ctx.remove_project(SelectedIndex);
        on_project_refresh(sender, e);
    }

    private void on_rename_project(object sender, RoutedEventArgs e)
    {
        int currentProject = SelectedIndex;
        string newName = NewProjectName.Text.ToString();

        ctx.rename_project(currentProject, newName);
        SelectedProject.Content = newName.ToString();
    }

    private void on_add_document(object sender, RoutedEventArgs e)
    {
        ctx.AddFile("Document", SelectedIndex, this);
    }

    private void on_add_drawing(object sender, RoutedEventArgs e)
    {
        ctx.AddFile("Drawing", SelectedIndex, this);
    }

    private void on_fetch_single_meta(object sender, RoutedEventArgs e)
    {
        ProgressStatus.Content = "Fetching Metadata";
        IList drawings = DrawingGrid.SelectedItems;
        IList documents = DocumentGrid.SelectedItems;

        ctx.SelectFiles(true, drawings, documents, SelectedType);
        MetaWorker.RunWorkerAsync();
    }

    private void on_fetch_full_meta(object sender, RoutedEventArgs e)
    {
        ProgressStatus.Content = "Fetching Metadata";

        ctx.SelectFiles(false, null, null, null);
        MetaWorker.RunWorkerAsync();
    }

    private void init_MetaWorker()
    {
        MetaWorker.DoWork += MetaWorker_DoWork;
        MetaWorker.WorkerReportsProgress = true;
        MetaWorker.ProgressChanged += MetaWorker_progress;
        MetaWorker.RunWorkerCompleted += MetaWorker_RunWorkerCompleted;
    }

    private void MetaWorker_DoWork(object sender, DoWorkEventArgs e)
    {
        int nPaths = ctx.GetNrSelectedFiles();

        for (int k = 0; k < nPaths; k++)
        {
            ctx.GetMetadata(k);

            int percentage = (k + 1) * 100 / nPaths;
            MetaWorker.ReportProgress(percentage);
        }
    }

    private void MetaWorker_progress(object sender, ProgressChangedEventArgs e)
    {
        ProgressBar.Value = e.ProgressPercentage;
    }

    private void MetaWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
        ctx.SetMetadata();
        ProgressStatus.Content = "";
        ProgressBar.Value = 0;
    }

    private void on_open_path(object sender, RoutedEventArgs e)
    {
        if (StatusLabel.Content == "Ready")
        {
            StatusLabel.Content = "Opening path";
            IList drawings = DrawingGrid.SelectedItems;
            IList documents = DocumentGrid.SelectedItems;

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

            ctx.OpenFile(drawings, documents, SelectedType,"MD");
            StatusLabel.Content = "Ready";
        }
    }

    private async void on_load_file(object sender, RoutedEventArgs e)
    {
        StatusLabel.Content = "Loading file";

        await ctx.LoadFile(this);

        on_project_refresh(sender, e);
        StatusLabel.Content = "Ready";
    }

    private async void on_save_file(object sender, RoutedEventArgs e)
    {
        StatusLabel.Content = "Saving file";
        await ctx.SaveFile(this);
        StatusLabel.Content = "Ready";
    }

    public void update_projectList()
    {
        ctx.UpdateProjectList();
    }

    public void update_fileLists(int selectedProject)
    {   
        ctx.UpdateLists(selectedProject);
    }

    private void on_remove_files(object sender, RoutedEventArgs e)
    {
        IList drawings      = DrawingGrid.SelectedItems;
        IList documents     = DocumentGrid.SelectedItems;

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
        ctx.UpdateLists(SelectedIndex);
    }

    private void on_update_columns(object sender, EventArgs e)
    {
        DrawingGrid.Columns[0].Width = new DataGridLength(1.0, DataGridLengthUnitType.SizeToCells);
        DrawingGrid.Columns[1].Width = new DataGridLength(1.0, DataGridLengthUnitType.SizeToCells);
        DrawingGrid.Columns[2].Width = new DataGridLength(1.0, DataGridLengthUnitType.SizeToCells);
        DrawingGrid.Columns[3].Width = new DataGridLength(1.0, DataGridLengthUnitType.SizeToCells);
        DrawingGrid.Columns[4].Width = new DataGridLength(1.0, DataGridLengthUnitType.SizeToCells);
        DrawingGrid.Columns[5].Width = new DataGridLength(1.0, DataGridLengthUnitType.SizeToCells);
        DrawingGrid.Columns[6].Width = new DataGridLength(1.0, DataGridLengthUnitType.SizeToCells);
        DrawingGrid.Columns[7].Width = new DataGridLength(1.0, DataGridLengthUnitType.SizeToCells);
        DrawingGrid.Columns[8].Width = new DataGridLength(1.0, DataGridLengthUnitType.SizeToCells);
        DrawingGrid.Columns[9].Width = new DataGridLength(1.0, DataGridLengthUnitType.SizeToCells);

        DocumentGrid.Columns[0].Width = new DataGridLength(1.0, DataGridLengthUnitType.SizeToCells);
        DocumentGrid.Columns[1].Width = new DataGridLength(1.0, DataGridLengthUnitType.SizeToCells);
        DocumentGrid.Columns[2].Width = new DataGridLength(1.0, DataGridLengthUnitType.SizeToCells);
        DocumentGrid.Columns[3].Width = new DataGridLength(1.0, DataGridLengthUnitType.SizeToCells);
        DocumentGrid.Columns[4].Width = new DataGridLength(1.0, DataGridLengthUnitType.SizeToCells);
        DocumentGrid.Columns[5].Width = new DataGridLength(1.0, DataGridLengthUnitType.SizeToCells);
        DocumentGrid.Columns[6].Width = new DataGridLength(1.0, DataGridLengthUnitType.SizeToCells);
        DocumentGrid.Columns[7].Width = new DataGridLength(1.0, DataGridLengthUnitType.SizeToCells);
        DocumentGrid.Columns[8].Width = new DataGridLength(1.0, DataGridLengthUnitType.SizeToCells);
        DocumentGrid.Columns[9].Width = new DataGridLength(1.0, DataGridLengthUnitType.SizeToCells);

        DrawingGrid.UpdateLayout();
    }

}

