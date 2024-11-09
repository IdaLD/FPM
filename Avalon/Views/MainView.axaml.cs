using Avalonia.Controls;
using System;
using Avalonia.Interactivity;
using System.Linq;
using Avalon.ViewModels;
using System.ComponentModel;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Input;
using System.Threading;
using System.Collections.Generic;
using Avalon.Model;
using System.IO;
using Avalonia.Data;
using Avalonia.Styling;
using System.Diagnostics;
using System.Net.Http.Headers;


namespace Avalon.Views;

public partial class MainView : UserControl, INotifyPropertyChanged
{
    public MainView() 
    {
        InitializeComponent();

        FileGrid.AddHandler(DataGrid.DoubleTappedEvent, on_open_file);
        TrayGrid.AddHandler(DataGrid.DoubleTappedEvent, on_open_file);

        FetchMetadata.AddHandler(Button.ClickEvent, on_fetch_full_meta);

        FileGrid.AddHandler(DataGrid.SelectionChangedEvent, set_preview_request_main);
        FileGrid.AddHandler(DataGrid.SelectionChangedEvent, select_files);
        FileGrid.AddHandler(DragDrop.DropEvent, on_drop);

        TrayGrid.AddHandler(DataGrid.SelectionChangedEvent, select_favorite);
        TrayGrid.AddHandler(DataGrid.SelectionChangedEvent, set_preview_request_tray);
        FavoriteGroups.AddHandler(ListBox.SelectionChangedEvent, OnFavoriteGroupChanged);

        PageGrid.AddHandler(DataGrid.SelectionChangedEvent, FavPageSelected);

        FileGrid.AddHandler(DataGrid.LoadedEvent, init_startup);
        PreviewToggle.AddHandler(ToggleSwitch.IsCheckedChangedEvent, on_toggle_preview);

        init_MetaWorker();

        StatusLabel.Content = "Ready";
    }

    public bool previewMode = false;
    public bool darkmode = true;
    public bool treeview = false;
    public bool trayview = false;

    private BackgroundWorker MetaWorker = new BackgroundWorker();

    private Thread taskThread = null;

    private CancellationTokenSource cts = new CancellationTokenSource();

    public MainViewModel ctx = null;
    public PreviewViewModel pwr = null;

    public List<DataGridRowEventArgs> Args = new List<DataGridRowEventArgs>();

    public ColumnDefinition a = new ColumnDefinition();
    public ColumnDefinition b = new ColumnDefinition();
    public ColumnDefinition c = new ColumnDefinition();
    public ColumnDefinition d = new ColumnDefinition();

    private bool PreviewTaskBusy = false;
    private bool PreviewReady = false;
    private double BitmapRes = 0.5;

    private void init_startup(object sender, RoutedEventArgs e)
    {
        get_datacontext();

        TreeStatus.IsChecked = true;

        try
        {
            string path = "C:\\FIlePathManager\\Projects.json";
            ctx.read_savefile(path);
        }
        catch
        { }
    }

    public void get_datacontext()
    {
        ctx = (MainViewModel)this.DataContext;
        pwr = ctx.PreviewVM;
        
        ctx.PropertyChanged += on_binding_ctx;
        pwr.PropertyChanged += on_binding_pwr;
    }


    public void on_binding_ctx(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "FilteredFiles") { on_update_columns(); }
        if (e.PropertyName == "UpdateColumns") { on_update_columns(); }
        if (e.PropertyName == "Color1") { update_row_color(); }
        if (e.PropertyName == "Color3") { update_row_color(); }
        if (e.PropertyName == "TreeViewUpdate") { SetupTreeview(null, null); }
    }

    private void on_binding_pwr(object sender, PropertyChangedEventArgs e)
    {

    }


    public void OnTogglePreviewWindow(object sender, RoutedEventArgs e)
    {
        
        if (!ctx.PreviewWindowOpen)
        {
            if (previewMode)
            {
                PreviewToggle.IsChecked = false;
            }

            var window = Window.GetTopLevel(this);

            ThemeVariant theme = window.RequestedThemeVariant;


            ctx.OpenPreviewWindow(theme);
        }

        else
        {
            ctx.PreviewWindow.Close();
        }

    }


    public void on_search(object sender, RoutedEventArgs e)
    {
        string searchtext = SearchText.Text;

        if (searchtext != null)
        {
            ctx.search(searchtext);
        }
        on_update_columns();
    }

    public void on_start_search(object sender, KeyEventArgs e)
    {
        if(e.Key == Key.Enter)
        {
            on_search(null, null);
        }
    }

    public void on_drop(object sender, DragEventArgs e)
    {
        var items = e.Data.GetFiles();

        foreach(var item in items)
        {
            if (item.Path.IsFile == true)
            {
                string path = item.Path.LocalPath;
                string type = System.IO.Path.GetExtension(path);

                if (type == ".pdf")
                {
                    ctx.AddFilesDrag(path);
                }
            }
        }
        SetupTreeview(null, null);
    }

    private void SetupTreeview(object sender, RoutedEventArgs e)
    {
        MainTree.Items.Clear();
        ctx.GetGroups();

        List<string> typeList = new List<string>() { "Archive", "Library", "Project" };


        foreach (string type in typeList)
        {
            List<TreeViewItem> items = new List<TreeViewItem>();

            IEnumerable<ProjectData> projects = ctx.ProjectsVM.StoredProjects.Where(x => x.Category == type);

            if (projects.Count() != 0)
            {
                foreach (ProjectData project in projects)
                {
                    if (project.Parent == null || project.Parent == "")
                    {
                        List<TreeViewItem> fileTypeTree = new List<TreeViewItem>();
                        foreach (string filetype in project.StoredFiles.Select(x => x.Filtyp).Distinct())
                        {
                            int nfiles = project.StoredFiles.Where(x => x.Filtyp == filetype).Count();
                            fileTypeTree.Add(new TreeViewItem() 
                            {
                                FontSize = 13,
                                FontWeight = FontWeight.Light,
                                Header = filetype + " (" + nfiles + ")", 
                                Tag = project.Namn 
                            });
                        }
                        items.Add(
                            new TreeViewItem()
                            {
                                FontSize = 15,
                                FontWeight = FontWeight.Normal,
                                FontStyle = FontStyle.Normal,
                                Header = project.Namn,
                                IsExpanded = (project == ctx.ProjectsVM.CurrentProject),
                                Tag = "All Types",
                                ItemsSource = fileTypeTree
                            }
                        );
                    }
                }

                if (type == "Project")
                {
                    foreach (string group in ctx.Groups)
                    {
                        IEnumerable<ProjectData> groupedProject = ctx.ProjectsVM.StoredProjects.Where(x => x.Parent == group);
                        List<TreeViewItem> groupedTree = new List<TreeViewItem>();

                        foreach (ProjectData project in groupedProject)
                        {
                            List<TreeViewItem> fileTypeTree = new List<TreeViewItem>();
                            foreach (string filetype in project.StoredFiles.Select(x => x.Filtyp).Distinct())
                            {
                                int nfiles = project.StoredFiles.Where(x => x.Filtyp == filetype).Count();
                                fileTypeTree.Add(new TreeViewItem() 
                                {
                                    FontSize = 13,
                                    FontWeight = FontWeight.Light,
                                    Header = filetype + " (" + nfiles + ")", 
                                    Tag = project.Namn 
                                });
                            }

                            groupedTree.Add(new TreeViewItem()
                            {
                                FontSize = 15,
                                FontWeight = FontWeight.Normal,
                                FontStyle = FontStyle.Normal,
                                Header = project.Namn,
                                IsExpanded = (project == ctx.ProjectsVM.CurrentProject),
                                Tag = "All Types",
                                ItemsSource = fileTypeTree
                            });
                        }

                        items.Add(
                            new TreeViewItem()
                            {
                                FontSize = 15,
                                FontWeight = FontWeight.Bold,
                                FontStyle = FontStyle.Normal,
                                Header = group,
                                IsExpanded = true,
                                Tag = "Group",
                                ItemsSource = groupedTree
                            }
                        );

                    }
                }

                MainTree.Items.Add(
                    new TreeViewItem()
                    {
                        FontSize = 16,
                        FontWeight = FontWeight.Bold,
                        FontStyle = FontStyle.Italic,
                        Header = type,
                        Tag = "Header",
                        IsExpanded = true,
                        ItemsSource = items
                    }
                );
            }

        }
    }

    public void toggle_treeview(object sender, RoutedEventArgs e)
    {
        treeview = !treeview;

        if (treeview)
        {
            SetupTreeview(null, null);
            MainGrid.ColumnDefinitions[0] = new ColumnDefinition(250, GridUnitType.Pixel);
        }
        else
        {
            MainGrid.ColumnDefinitions[0] = new ColumnDefinition(0, GridUnitType.Pixel);
        }
    }

    public void toggle_tray(object sender, RoutedEventArgs e)
    {
        trayview = !trayview;

        if (trayview)
        {
            ctx.ProjectsVM.UpdateFavorite();
            ctx.OnGetFavGroups();
            MainGrid.ColumnDefinitions[4] = new ColumnDefinition(300, GridUnitType.Pixel);
        }
        else
        {
            MainGrid.ColumnDefinitions[4] = new ColumnDefinition(0, GridUnitType.Pixel);
        }
    }

    public void on_treeview_selected(object sender, SelectionChangedEventArgs e)
    {
        object selected = MainTree.SelectedItem;


        if (selected != null)
        {
    
            TreeViewItem selectedTree = (TreeViewItem)selected;

            if(selectedTree.Tag == "Header" || selectedTree.Tag == "Group")
            {
                MainTree.SelectedItem = null;
                MainTree.ContextMenu.IsEnabled = false;
                return;
            }

            else
            {

                TreeViewItem parentTree = (TreeViewItem)selectedTree.Parent;
                

                MainTree.ContextMenu.IsEnabled = true;

                if (selectedTree.Tag == "All Types")
                {
                    ctx.select_type("All Types");
                    ctx.select_project(selectedTree.Header.ToString());
                }
                else
                {
                    ctx.select_type(selectedTree.Header.ToString().Split(" ")[0]);
                    ctx.select_project(selectedTree.Tag.ToString());
                }
            }
            
            on_update_columns();
        }
    }

    public void toggle_theme(object sender, RoutedEventArgs e)
    {
        darkmode = !darkmode;

        ModeDayIcon.IsVisible = !ModeDayIcon.IsVisible;
        ModeNightIcon.IsVisible = !ModeNightIcon.IsVisible;

        var window = Window.GetTopLevel(this);

        if (darkmode)
        {
            window.RequestedThemeVariant = ThemeVariant.Dark;
        }
        else
        {
            window.RequestedThemeVariant = ThemeVariant.Light;
        }

    }


    private void Border_PointerPressed(object sender, RoutedEventArgs args)
    {
        var ctl = sender as Control;
        if (ctl != null)
        {
            FlyoutBase.ShowAttachedFlyout(ctl);
        }
    }

    private void on_toggle_preview(object sender, RoutedEventArgs e)
    {
        if (ctx.PreviewWindowOpen)
        {
            OnTogglePreviewWindow(null, null);
        }

        previewMode = !previewMode;

        float val1 = 0f;
        float val2 = 0f;

        if (previewMode == true)
        {
            val1 = 5f;
            val2 = 3.2f;

            PreviewArea.IsVisible = true;

        }
        if (previewMode == false)
        {
            PreviewArea.IsVisible = false;
        }

        MainGrid.ColumnDefinitions[1] = new ColumnDefinition(1f, GridUnitType.Star);
        MainGrid.ColumnDefinitions[2] = new ColumnDefinition(val1, GridUnitType.Pixel);
        MainGrid.ColumnDefinitions[3] = new ColumnDefinition(val2, GridUnitType.Star);

        if (previewMode)
        {
            MainGrid.ColumnDefinitions[3].MinWidth = 300f;
            FileGrid.SelectedItems.Clear();

            EmbeddedPreview.UpdateLayout();
            EmbeddedPreview.IsVisible = true;

            EmbeddedPreview.SetRenderer();
        }

    }

    private void set_preview_request_main(object sender, RoutedEventArgs r)
    {
        FileData file = (FileData)FileGrid.SelectedItem;
        set_preview_request(file);

    }

    private void set_preview_request_tray(object sender, RoutedEventArgs r)
    {
        FileData file = (FileData)TrayGrid.SelectedItem;
        set_preview_request(file);
    }

    private void set_preview_request(FileData file)
    {
        if (previewMode || ctx.PreviewWindowOpen)
        {
            CheckStatusSingleFile();

            if (file != null && System.IO.Path.Exists(file.Sökväg)) 
            {
                int startPage = file.DefaultPage;

                pwr.SetupPage(startPage);
                pwr.RequestFile = file;

                pwr.SetFile();
            }
        }
    }

    private void on_copy_filepath(object sender, RoutedEventArgs e)
    {
        ctx.CopyFilepathToClipboard(this);
    }

    private void on_copy_filename(object sender, RoutedEventArgs e)
    {
        ctx.CopyFilenameToClipboard(this);
    }

    private void on_copy_listview(object sender, RoutedEventArgs e)
    {
        ctx.CopyListviewToClipboard(this);
    }

    private void edit_color(object sender, RoutedEventArgs e)
    {
        var menuItem = sender as MenuItem;
        string color = menuItem.Tag.ToString();

        if (color != "None")
        {
            ctx.add_color(color);
        }

        deselect_items();
        update_row_color();
    }

    private void edit_type(object sender, RoutedEventArgs e)
    {
        var menuItem = sender as MenuItem;

        MenuItem SelectedMenu = ctx.FileTypeSelection[menuItem.SelectedIndex];

        ctx.edit_type(SelectedMenu.Header.ToString());
        SetupTreeview(null, null);
    }

    private void deselect_items()
    {
        FileGrid.SelectedItem = null;
    }

    private void on_clear_files(object sender, RoutedEventArgs e)
    {
        ctx.clear_all();

        deselect_items();
       update_row_color();
    }

    private void OnCheckStatusSingleFile(object sender, RoutedEventArgs e)
    {
        CheckStatusSingleFile();
    }

    private void CheckStatusSingleFile()
    {
        ctx.CheckSingleFile();
        update_row_color();
    }



    private async void OnCheckProjectFiles(object sender, RoutedEventArgs e)
    {
        await ctx.CheckProjectFiles();
        deselect_items();
        update_row_color();
    }


    async void OnRemoveProject(object sender, RoutedEventArgs e)
    {

        Window window = (MainWindow)Window.GetTopLevel(this);
        await ctx.ConfirmDeleteDia(window);

        if (ctx.Confirmed)
        {
            ctx.remove_project();
            SetupTreeview(null, null);
        }
    }


    private void on_add_file(object sender, RoutedEventArgs e)
    {
        StatusLabel.Content = "Adding Files";
        ctx.AddFile(this);
        SetupTreeview(null, null);
        StatusLabel.Content = "Ready";
    }

    private void on_fetch_single_meta(object sender, RoutedEventArgs e)
    {
        ProgressStatus.Content = "Fetching Metadata";

        ctx.SelectFilesForMetaworker(true);
        MetaWorker.RunWorkerAsync();
    }

    private void on_fetch_full_meta(object sender, RoutedEventArgs e)
    {
        ProgressStatus.Content = "Fetching Metadata";

        ctx.SelectFilesForMetaworker(false);
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
        ctx.set_meta();
        ProgressStatus.Content = "";
        ProgressBar.Value = 0;
    }

    private void FavPageSelected(object sender, RoutedEventArgs e)
    {
        if (previewMode || ctx.PreviewWindowOpen)
        {
            PageData page = (PageData)PageGrid.SelectedItem;
            ctx.SetFavPage(page);
        }
    }

    private void OnAddFavPage(object sender, RoutedEventArgs e)
    {
        if (previewMode || ctx.PreviewWindowOpen)
        {
            ctx.AddFavPage(NewFavPageInput.Text);
            NewFavPageInput.Clear();
        }
    }

    private void OnRenameFavPage(object sender, RoutedEventArgs e)
    {
        ctx.RenameFavPage(NewFavPageInput.Text);
        NewFavPageInput.Clear();
    }

    private void OnRemoveFavPage(object sender, RoutedEventArgs e)
    {
        PageData page = (PageData)PageGrid.SelectedItem;
        ctx.RemoveFavPage(page);
    }

    private void select_favorite(object sender, RoutedEventArgs e)
    {
        IList<FileData> files = TrayGrid.SelectedItems.Cast<FileData>().ToList();
        
        deselect_items();
        ctx.select_files(files);
    }

    private void OnAddFavGroup(object sender, RoutedEventArgs e)
    {
        string text = NewFavGroupInput.Text;

        if(text == null || text.ToString().Length == 0)
        {
            return;
        }

        ctx.AddFavGroup(NewFavGroupInput.Text);
        NewFavGroupInput.Clear();
    }

    private void OnRenameFavGroup(object sender, RoutedEventArgs e)
    {
        string text = NewFavGroupInput.Text;

        if (text == null || text.ToString().Length == 0)
        {
            return;
        }

        ctx.RenameFavGroup(NewFavGroupInput.Text);
        NewFavGroupInput.Clear();
    }

    async void OnRemoveFavGroup(object sender, RoutedEventArgs e)
    {
        Window window = (MainWindow)Window.GetTopLevel(this);
        await ctx.ConfirmDeleteDia(window);

        if (ctx.Confirmed)
        {
            ctx.RemoveFavGroup();
        }
    }


    private void OnAddFavorite(object sender, RoutedEventArgs e)
    {
        MenuItem source = e.Source as MenuItem;

        ctx.ProjectsVM.AddFavorite(source.Header.ToString());
        ctx.CurrentFavorite = source.Header.ToString();
    }

    private void OnFavoriteGroupChanged(object sender, RoutedEventArgs e)
    {
        ListBox source = e.Source as ListBox;
        if (source.SelectedItem != null)
        {
            ctx.CurrentFavorite = source.SelectedItem.ToString();
        }
    }

    private void select_files(object sender, RoutedEventArgs e)
    {
        IList<FileData> files = FileGrid.SelectedItems.Cast<FileData>().ToList();
        TrayGrid.SelectedItem = null;

        ctx.select_files(files);
    }

    private void on_open_path(object sender, RoutedEventArgs e)
    {
        StatusLabel.Content = "Opening path";
        CheckStatusSingleFile();
        ctx.open_path();
        StatusLabel.Content = "Ready";
    }

    private void on_open_file(object sender, RoutedEventArgs e)
    {
        StatusLabel.Content = "Opening file";

        CheckStatusSingleFile();
        ctx.open_files();
        
        StatusLabel.Content = "Ready";   
    }

    private void on_open_metafile(object sender, RoutedEventArgs e)
    {
        StatusLabel.Content = "Opening metafile";
        ctx.open_meta();
        StatusLabel.Content = "Ready";   
    }

    private void on_open_dwg(object sender, RoutedEventArgs e)
    {
        StatusLabel.Content = "Opening Drawing";
        ctx.open_dwg();
        StatusLabel.Content = "Ready";       
    }

    private void on_open_doc(object sender, RoutedEventArgs e)
    {
        StatusLabel.Content = "Opening Document";
        ctx.open_doc();
        StatusLabel.Content = "Ready";
    }

    private async void on_load_file(object sender, RoutedEventArgs e)
    {
        await ctx.LoadFile(this);
        SetupTreeview(null, null);
    }

    private async void on_save_file(object sender, RoutedEventArgs e)
    {
        StatusLabel.Content = "Saving file";
        await ctx.SaveFile(this);
        StatusLabel.Content = "Ready";
    }

    private async void on_save_file_auto(object sender, RoutedEventArgs e)
    {
        StatusLabel.Content = "Saving file";

        if (!Directory.Exists("\\FIlePathManager"))
        {
            Directory.CreateDirectory("\\FIlePathManager");
        }

        string path = "C:\\FIlePathManager\\Projects.json";

        

        await ctx.SaveFileAuto(path);
        StatusLabel.Content = "Ready";
    }

    private void on_move_files(object sender, RoutedEventArgs e)
    {
        string projectname = MoveFileToProjectName.Text;
        ctx.move_files(projectname);
        ctx.ProjectsVM.UpdateFilter();
    }

    async void OnRemoveFiles(object sender, RoutedEventArgs e)
    {
        Window window = (MainWindow)Window.GetTopLevel(this);
        await ctx.ConfirmDeleteDia(window);

        if (ctx.Confirmed)
        {
            ctx.ProjectsVM.RemoveSelectedFiles();
            ctx.ProjectsVM.UpdateFilter();
            SetupTreeview(null, null);
        }
    }

    private void on_update_columns()
    {
        FileGrid.Columns[0].Width = new DataGridLength(1.0, DataGridLengthUnitType.SizeToCells);
        FileGrid.Columns[1].Width = new DataGridLength(1.0, DataGridLengthUnitType.SizeToCells);
        FileGrid.Columns[2].Width = new DataGridLength(1.0, DataGridLengthUnitType.SizeToCells);
        FileGrid.Columns[3].Width = new DataGridLength(1.0, DataGridLengthUnitType.SizeToCells);
        FileGrid.Columns[4].Width = new DataGridLength(1.0, DataGridLengthUnitType.SizeToCells);
        FileGrid.Columns[5].Width = new DataGridLength(1.0, DataGridLengthUnitType.SizeToCells);
        FileGrid.Columns[6].Width = new DataGridLength(1.0, DataGridLengthUnitType.SizeToCells);
        FileGrid.Columns[7].Width = new DataGridLength(1.0, DataGridLengthUnitType.SizeToCells);
        FileGrid.Columns[8].Width = new DataGridLength(1.0, DataGridLengthUnitType.SizeToCells);
        FileGrid.Columns[9].Width = new DataGridLength(1.0, DataGridLengthUnitType.SizeToCells);

        FileGrid.UpdateLayout();

    }

    private void DataGrid_OnLoadingRow(object? sender, DataGridRowEventArgs e)
    {
        Args.Add(e);

        var dataObject = e.Row.DataContext as FileData;
        e.Row.Classes.Clear();

        if (dataObject != null && dataObject.Färg == "") { e.Row.Classes.Clear(); }

        if (dataObject != null && dataObject.FileStatus == "Missing") { e.Row.Classes.Add("RedForeground"); }

        if (dataObject != null && dataObject.Färg == "Yellow") { e.Row.Classes.Add("Yellow"); }
        if (dataObject != null && dataObject.Färg == "Orange") { e.Row.Classes.Add("Orange"); }
        if (dataObject != null && dataObject.Färg == "Brown") { e.Row.Classes.Add("Brown"); }
        if (dataObject != null && dataObject.Färg == "Green") { e.Row.Classes.Add("Green"); }
        if (dataObject != null && dataObject.Färg == "Blue") { e.Row.Classes.Add("Blue"); }
        if (dataObject != null && dataObject.Färg == "Red") { e.Row.Classes.Add("Red"); }
        if (dataObject != null && dataObject.Färg == "Magenta") { e.Row.Classes.Add("Magenta"); }
    }

    private void update_row_color()
    {

        foreach (DataGridRowEventArgs e in Args)
        {
            var dataObject = e.Row.DataContext as FileData;

            e.Row.Classes.Clear();

            if (dataObject != null && dataObject.FileStatus == "Missing") { e.Row.Classes.Add("RedForeground"); }

            if (dataObject != null && dataObject.Färg == "Yellow") { e.Row.Classes.Add("Yellow"); }
            if (dataObject != null && dataObject.Färg == "Orange") { e.Row.Classes.Add("Orange"); }
            if (dataObject != null && dataObject.Färg == "Brown") { e.Row.Classes.Add("Brown"); }
            if (dataObject != null && dataObject.Färg == "Green") { e.Row.Classes.Add("Green"); }
            if (dataObject != null && dataObject.Färg == "Blue") { e.Row.Classes.Add("Blue"); }
            if (dataObject != null && dataObject.Färg == "Red") { e.Row.Classes.Add("Red"); }
            if (dataObject != null && dataObject.Färg == "Magenta") { e.Row.Classes.Add("Magenta"); }
        }
    }

    private void RaisePropertyChanged(string propName)
    {
        if (PropertyChanged != null)
            PropertyChanged(this, new PropertyChangedEventArgs(propName));
    }
    public event PropertyChangedEventHandler PropertyChanged;

}

