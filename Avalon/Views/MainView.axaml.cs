using Avalonia.Controls;
using System;
using Avalonia.Interactivity;
using System.Linq;
using Avalon.ViewModels;
using Avalonia;
using System.ComponentModel;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Input;
using Material.Styles.Themes;
using System.Threading;
using System.Collections.Generic;
using Avalon.Model;
using Avalonia.LogicalTree;
using System.IO;
using Avalonia.Data;
using iText.Kernel.Geom;


namespace Avalon.Views;

public partial class MainView : UserControl, INotifyPropertyChanged
{
    public MainView() 
    {
        InitializeComponent();

        FileGrid.AddHandler(DataGrid.DoubleTappedEvent, on_open_file);
        FetchMetadata.AddHandler(Button.ClickEvent, on_fetch_full_meta);

        FileGrid.AddHandler(DataGrid.SelectionChangedEvent, set_preview_request);
        FileGrid.AddHandler(DataGrid.SelectionChangedEvent, select_files);
        FileGrid.AddHandler(DragDrop.DropEvent, on_drop);

        Lockedstatus.AddHandler(ToggleSwitch.IsCheckedChangedEvent, on_lock);
        FileGrid.AddHandler(DataGrid.LoadedEvent, init_startup);
        PreviewToggle.AddHandler(ToggleSwitch.IsCheckedChangedEvent, on_toggle_preview);
        PreviewGrid.AddHandler(Grid.SizeChangedEvent, PreviewSizeChanged);

        init_MetaWorker();

        StatusLabel.Content = "Ready";
    }

    public bool previewMode = false;
    public bool darkmode = true;
    public bool treeview = false;

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
    private bool ZoomMode = false;

    private void init_startup(object sender, RoutedEventArgs e)
    {
        get_datacontext();
        pwr.GetRenderControl(MuPDFRenderer);
        
        Lockedstatus.IsChecked = true;
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
    }

    private void on_binding_pwr(object sender, PropertyChangedEventArgs e)
    {

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
    }

    public void on_set_category(object sender, RoutedEventArgs e)
    {
        MenuItem menuitem = sender as MenuItem;
        string category = menuitem.Header.ToString();

        ctx.set_category(category);
    }

    public void toggle_treeview(object sender, RoutedEventArgs e)
    {
        treeview = !treeview;

        TreeviewOn.IsVisible = !TreeviewOn.IsVisible;
        TreeviewOff.IsVisible = !TreeviewOff.IsVisible;

        if (treeview)
        {
            MainGrid.ColumnDefinitions[0] = new ColumnDefinition(200, GridUnitType.Pixel);
        }
        else
        {
            MainGrid.ColumnDefinitions[0] = new ColumnDefinition(0, GridUnitType.Pixel);
        }
    }

    public void on_treeview_selected(object sender, SelectionChangedEventArgs e)
    {  
        object selected = MainTree.SelectedItem;

        if (selected != null)
        {
            Type selectedtype = selected.GetType();

            if (selectedtype == typeof(ProjectData))
            {
                ProjectData project = (ProjectData)selected;
                ctx.select_type("All Types");
                ctx.select_project(project.Namn);
            }

            if (selectedtype == typeof(string))
            {

                string[] split = selected.ToString().Split("\t");

                ctx.select_type(split[0]);

                TreeViewItem item = (TreeViewItem)MainTree.TreeContainerFromItem(MainTree.SelectedItem);
                TreeViewItem parent = item.GetLogicalParent() as TreeViewItem;
                ProjectData project = MainTree.ItemFromContainer(parent) as ProjectData;

                ctx.select_project(project.Namn);

            }
            on_update_columns();
        }
    }

    public void toggle_theme(object sender, RoutedEventArgs e)
    {
        darkmode = !darkmode;

        ModeDayIcon.IsVisible = !ModeDayIcon.IsVisible;
        ModeNightIcon.IsVisible = !ModeNightIcon.IsVisible;

        if (darkmode)
        {
            var MaterialThemeStyles = Avalonia.Application.Current!.LocateMaterialTheme<MaterialTheme>();
            MaterialThemeStyles.BaseTheme = Material.Styles.Themes.Base.BaseThemeMode.Dark;
            MaterialThemeStyles.PrimaryColor = Material.Colors.PrimaryColor.Grey;
        }
        else
        {
            var MaterialThemeStyles = Avalonia.Application.Current!.LocateMaterialTheme<MaterialTheme>();
            MaterialThemeStyles.BaseTheme = Material.Styles.Themes.Base.BaseThemeMode.Light;
            MaterialThemeStyles.PrimaryColor = Material.Colors.PrimaryColor.Blue;
        }

        set_theme_colors();
        update_row_color();
    }


    private void Border_PointerPressed(object sender, RoutedEventArgs args)
    {
        var ctl = sender as Control;
        if (ctl != null)
        {
            FlyoutBase.ShowAttachedFlyout(ctl);
        }
    }

    private async void on_toggle_preview(object sender, RoutedEventArgs e)
    {
        if (previewMode)
        {
            if(ctx.FullScreenMode)
            {
                OnFullscreenMode(null, null);
            }
        }
        previewMode = !previewMode;

        EyeOnIcon.IsVisible = previewMode;
        EyeOffIcon.IsVisible = !previewMode;

        float val1 = 0f;
        float val2 = 0f;

        if (previewMode == true)
        {
            val1 = 5f;
            val2 = 3.2f;
        }
        if (previewMode == false)
        {
            await pwr.CloseRenderer();
        }


        MainGrid.ColumnDefinitions[1] = new ColumnDefinition(1f, GridUnitType.Star);
        MainGrid.ColumnDefinitions[2] = new ColumnDefinition(val1, GridUnitType.Pixel);
        MainGrid.ColumnDefinitions[3] = new ColumnDefinition(val2, GridUnitType.Star);

        if (previewMode)
        {
            MainGrid.ColumnDefinitions[3].MinWidth = 300f;
        }

    }

    private void set_preview_request(object sender, RoutedEventArgs r)
    {
        if (previewMode == true)
        {
            FileData file = (FileData)FileGrid.SelectedItem;

            if (file != null && System.IO.Path.Exists(file.Sökväg)) 
            {
                ScrollSlider.Value = 1;
                pwr.RequestFile = file;
            }
        }
    }


    private void ModifiedControlPointerWheelChanged(object sender, PointerWheelEventArgs e)
    {
        if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            ZoomMode = true;
        }
        else
        {
            ZoomMode = false;
        }

        MuPDFRenderer.ZoomEnabled = ZoomMode;

        if (!ZoomMode && pwr.Pagecount >  0)
        {
            if (!e.KeyModifiers.HasFlag(KeyModifiers.Control))
            {
                Avalonia.Vector mode = e.Delta;

                if (mode.Y > 0)
                {
                    pwr.PrevPage();
                }

                if (mode.Y < 0)
                {
                    pwr.NextPage();
                }
            }
        }
    }


    private void PreviewSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (previewMode)
        {
            ResetView(null, null);
        }
    }

    private void ResetView(object sender, RoutedEventArgs e)
    {
        MuPDFRenderer.Contain();
    }

    private void OnFullscreenMode(object sender, RoutedEventArgs e)
    {
        if (previewMode)
        {

            ctx.FullScreenMode = !ctx.FullScreenMode;
             
            if (ctx.FullScreenMode)
            {
                if (pwr.SearchMode)
                {
                    pwr.SearchMode = !pwr.SearchMode;
                    ToggleSearchMode(null, null);
                }

                a = MainGrid.ColumnDefinitions[0];
                b = MainGrid.ColumnDefinitions[1];
                c = MainGrid.ColumnDefinitions[2];
                d = MainGrid.ColumnDefinitions[3];

                MainGrid.RowDefinitions[0] = new RowDefinition(15f, GridUnitType.Pixel);
                MainGrid.RowDefinitions[1] = new RowDefinition(0f, GridUnitType.Pixel);
                MainGrid.RowDefinitions[2] = new RowDefinition(1f, GridUnitType.Star);

                MainGrid.ColumnDefinitions[0] = new ColumnDefinition(0f, GridUnitType.Pixel);
                MainGrid.ColumnDefinitions[1] = new ColumnDefinition(0f, GridUnitType.Pixel);
                MainGrid.ColumnDefinitions[2] = new ColumnDefinition(0f, GridUnitType.Pixel);
                MainGrid.ColumnDefinitions[3] = new ColumnDefinition(1f, GridUnitType.Star);

                ZoomMode = false;
                MuPDFRenderer.ZoomEnabled = ZoomMode;
            }

            else
            {
                MainGrid.RowDefinitions[0] = new RowDefinition(35f, GridUnitType.Pixel);
                MainGrid.RowDefinitions[1] = new RowDefinition(30f, GridUnitType.Pixel);
                MainGrid.RowDefinitions[2] = new RowDefinition(1f, GridUnitType.Star);

                MainGrid.ColumnDefinitions[0] = a;
                MainGrid.ColumnDefinitions[1] = b;
                MainGrid.ColumnDefinitions[2] = c;
                MainGrid.ColumnDefinitions[3] = d;

                ZoomMode = false;
                MuPDFRenderer.ZoomEnabled = ZoomMode;
            }
        }
    }

    private void ToggleSearchMode(object sender, RoutedEventArgs e)
    {
        if (pwr.SearchMode)
        {
            MainPreviewGrid.ColumnDefinitions[0] = new ColumnDefinition(1f, GridUnitType.Star);
            MainPreviewGrid.ColumnDefinitions[1] = new ColumnDefinition(200f, GridUnitType.Pixel);
            SearchRegex.Focus();
        }
        else
        {
            MainPreviewGrid.ColumnDefinitions[0] = new ColumnDefinition(1f, GridUnitType.Star);
            MainPreviewGrid.ColumnDefinitions[1] = new ColumnDefinition(0f, GridUnitType.Pixel);
        }
    }

    private void OnSeachRegex(object sender, RoutedEventArgs e)
    {
        string text = SearchRegex.Text;
        pwr.Search(text);
    }

    private void OnClearSearch(object sender, RoutedEventArgs e)
    {
        if(pwr.SearchBusy)
        {
            pwr.StopSearch();
        }
        else
        {
            pwr.ClearSearch();
            SearchRegex.Clear();
        }
    }

    private void OnStartSearhRegex(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            OnSeachRegex(null, null);
        }
    }


    private void on_lock(object sender, EventArgs e)
    {
        if (Lockedstatus.IsChecked == true)
        {
            RemoveProjectMenu.IsEnabled = false;
            RemoveFileMenu.IsEnabled = false;
            MoveFileMenu.IsEnabled = false;
            LockIcon.IsVisible = true;
            UnlockedIcon.IsVisible = false;
        }
        if (Lockedstatus.IsChecked == false)
        {
            RemoveProjectMenu.IsEnabled = true;
            RemoveFileMenu.IsEnabled = true;
            MoveFileMenu.IsEnabled = true;
            LockIcon.IsVisible = false;
            UnlockedIcon.IsVisible = true;
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

        ctx.add_color(color);

        deselect_items();
        update_row_color();
    }

    private void edit_type(object sender, RoutedEventArgs e)
    {
        var menuItem = sender as MenuItem;

        MenuItem SelectedMenu = ctx.FileTypeSelection[menuItem.SelectedIndex];

        ctx.edit_type(SelectedMenu.Header.ToString());
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

    private void on_add_tag(object sender, RoutedEventArgs e)
    {
        if (TagMenuInput.Text != null)
        {
            string tag = TagMenuInput.Text.ToString();
            ctx.add_tag(tag);
            ctx.add_tag(TagMenuInput.Text);
        }

        deselect_items();
    }

    private void on_clear_tag(object sender, RoutedEventArgs e)
    {
        ctx.clear_tag();
        deselect_items();
    }

    private void on_add_project(object sender, RoutedEventArgs e)
    {
        var Name = ProjectName.Text;
        if (Name != null)
        {
            ctx.new_project(Name.ToString());
        }
    }

    private void on_rename_project(object sender, RoutedEventArgs e)
    {
        if (NewProjectName.Text != null)
        {
            ctx.rename_project(NewProjectName.Text.ToString());
            NewProjectName.Text = null;
        }
    }

    private void on_add_file(object sender, RoutedEventArgs e)
    {
        StatusLabel.Content = "Adding Files";
        ctx.AddFile(this);
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

    private void select_files(object sender, RoutedEventArgs e)
    {
        IList<FileData> files = FileGrid.SelectedItems.Cast<FileData>().ToList();
        ctx.select_files(files);
    }

    private void on_open_path(object sender, RoutedEventArgs e)
    {
        StatusLabel.Content = "Opening path";
        ctx.open_path();
        StatusLabel.Content = "Ready";
    }

    private void on_open_file(object sender, RoutedEventArgs e)
    {
        StatusLabel.Content = "Opening file";
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

        if (darkmode == true)
        {
            if (dataObject != null && dataObject.Färg == "Yellow") { e.Row.Classes.Add("YellowDark"); }
            if (dataObject != null && dataObject.Färg == "Orange") { e.Row.Classes.Add("OrangeDark"); }
            if (dataObject != null && dataObject.Färg == "Brown") { e.Row.Classes.Add("BrownDark"); }
            if (dataObject != null && dataObject.Färg == "Green") { e.Row.Classes.Add("GreenDark"); }
            if (dataObject != null && dataObject.Färg == "Blue") { e.Row.Classes.Add("BlueDark"); }
            if (dataObject != null && dataObject.Färg == "Red") { e.Row.Classes.Add("RedDark"); }
            if (dataObject != null && dataObject.Färg == "Magenta") { e.Row.Classes.Add("MagentaDark"); }
        }
        else
        {
            if (dataObject != null && dataObject.Färg == "Yellow") { e.Row.Classes.Add("YellowLight"); }
            if (dataObject != null && dataObject.Färg == "Orange") { e.Row.Classes.Add("OrangeLight"); }
            if (dataObject != null && dataObject.Färg == "Brown") { e.Row.Classes.Add("BrownLight"); }
            if (dataObject != null && dataObject.Färg == "Green") { e.Row.Classes.Add("GreenLight"); }
            if (dataObject != null && dataObject.Färg == "Blue") { e.Row.Classes.Add("BlueLight"); }
            if (dataObject != null && dataObject.Färg == "Red") { e.Row.Classes.Add("RedLight"); }
            if (dataObject != null && dataObject.Färg == "Magenta") { e.Row.Classes.Add("MagentaLight"); }
        }
    }

    private void set_theme_colors()
    {
        if (darkmode == true)
        {

            YellowMenu.Foreground = (IBrush)new BrushConverter().ConvertFrom("#646424");
            OrangeMenu.Foreground = (IBrush)new BrushConverter().ConvertFrom("#643e24");
            BrownMenu.Foreground = (IBrush)new BrushConverter().ConvertFrom("#3e3124");
            GreenMenu.Foreground = (IBrush)new BrushConverter().ConvertFrom("#244a24");
            BlueMenu.Foreground = (IBrush)new BrushConverter().ConvertFrom("#243e64");
            RedMenu.Foreground = (IBrush)new BrushConverter().ConvertFrom("#642424");
            MagentaMenu.Foreground = (IBrush)new BrushConverter().ConvertFrom("#57244a");
        }

        if (darkmode == false)
        {
            YellowMenu.Foreground = (IBrush)new BrushConverter().ConvertFrom("#ffff99");
            OrangeMenu.Foreground = (IBrush)new BrushConverter().ConvertFrom("#ffd699");
            BrownMenu.Foreground = (IBrush)new BrushConverter().ConvertFrom("#c2ad99");
            GreenMenu.Foreground = (IBrush)new BrushConverter().ConvertFrom("#8cd1a3");
            BlueMenu.Foreground = (IBrush)new BrushConverter().ConvertFrom("#a3a3ff");
            RedMenu.Foreground = (IBrush)new BrushConverter().ConvertFrom("#ff8c8c");
            MagentaMenu.Foreground = (IBrush)new BrushConverter().ConvertFrom("#eb99eb");
        }
    }

    private void update_row_color()
    {
        foreach (DataGridRowEventArgs e in Args)
        {
            var dataObject = e.Row.DataContext as FileData;

            e.Row.Classes.Clear();

            if (darkmode == true)
            {
                if (dataObject != null && dataObject.Färg == "Yellow") { e.Row.Classes.Add("YellowDark"); }
                if (dataObject != null && dataObject.Färg == "Orange") { e.Row.Classes.Add("OrangeDark"); }
                if (dataObject != null && dataObject.Färg == "Brown") { e.Row.Classes.Add("BrownDark"); }
                if (dataObject != null && dataObject.Färg == "Green") { e.Row.Classes.Add("GreenDark"); }
                if (dataObject != null && dataObject.Färg == "Blue") { e.Row.Classes.Add("BlueDark"); }
                if (dataObject != null && dataObject.Färg == "Red") { e.Row.Classes.Add("RedDark"); }
                if (dataObject != null && dataObject.Färg == "Magenta") { e.Row.Classes.Add("MagentaDark"); }
            }
            else
            {
                if (dataObject != null && dataObject.Färg == "Yellow") { e.Row.Classes.Add("YellowLight"); }
                if (dataObject != null && dataObject.Färg == "Orange") { e.Row.Classes.Add("OrangeLight"); }
                if (dataObject != null && dataObject.Färg == "Brown") { e.Row.Classes.Add("BrownLight"); }
                if (dataObject != null && dataObject.Färg == "Green") { e.Row.Classes.Add("GreenLight"); }
                if (dataObject != null && dataObject.Färg == "Blue") { e.Row.Classes.Add("BlueLight"); }
                if (dataObject != null && dataObject.Färg == "Red") { e.Row.Classes.Add("RedLight"); }
                if (dataObject != null && dataObject.Färg == "Magenta") { e.Row.Classes.Add("MagentaLight"); }
            }
        }
    }

    private void RaisePropertyChanged(string propName)
    {
        if (PropertyChanged != null)
            PropertyChanged(this, new PropertyChangedEventArgs(propName));
    }
    public event PropertyChangedEventHandler PropertyChanged;

}

