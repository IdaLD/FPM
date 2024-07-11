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
using System.Diagnostics;
using Avalon.Model;
using Avalonia.LogicalTree;
using System.IO;


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

        ProjectList.AddHandler(ListBox.SelectionChangedEvent, on_project_selected);
        TypeList.AddHandler(ListBox.SelectionChangedEvent, on_type_selected);

        Lockedstatus.AddHandler(ToggleSwitch.IsCheckedChangedEvent, on_lock);
        FileGrid.AddHandler(DataGrid.LoadedEvent, init_startup);

        PreviewToggle.AddHandler(ToggleSwitch.IsCheckedChangedEvent, on_toggle_preview);


        Preview.AddHandler(Viewbox.PointerWheelChangedEvent, on_preview_zoom);
        Preview.AddHandler(Viewbox.PointerWheelChangedEvent, on_scroll_preview);

        Preview2.AddHandler(Viewbox.PointerWheelChangedEvent, on_preview_zoom);
        Preview2.AddHandler(Viewbox.PointerWheelChangedEvent, on_scroll_preview);

        Preview.AddHandler(Viewbox.PointerPressedEvent, on_pan_start);
        Preview.AddHandler(Viewbox.PointerMovedEvent, on_preview_pan);
        Preview.AddHandler(Viewbox.PointerReleasedEvent, on_pan_end);

        Preview2.AddHandler(Viewbox.PointerPressedEvent, on_pan_start);
        Preview2.AddHandler(Viewbox.PointerMovedEvent, on_preview_pan);
        Preview2.AddHandler(Viewbox.PointerReleasedEvent, on_pan_end);

        ScrollSlider.AddHandler(Slider.ValueChangedEvent, on_select_page);

        init_MetaWorker();
        init_PreviewWorker();
        setup_preview_transform();

        StatusLabel.Content = "Ready";
    }

    public string TagInput = "";

    public bool previewMode = false;
    public bool preview_pan = false;

    public double x_start = 0f;
    public double y_start = 0f;

    public double pw_scale = 1f;

    public string preview_request = "";
    public string preview_current = "";

    public bool darkmode = true;
    public bool treeview = false;

    private TransformGroup trGrp;
    private TranslateTransform trTns;
    private ScaleTransform trScl;
    public TransformGroup transform = new TransformGroup();

    private BackgroundWorker MetaWorker = new BackgroundWorker();
    private BackgroundWorker PreviewWorker = new BackgroundWorker();

    private Thread taskThread = null;

    private bool PreviewWorker_busy = false;

    public MainViewModel ctx = null;

    public List<DataGridRowEventArgs> Args = new List<DataGridRowEventArgs>();

    public void get_datacontext()
    {
        ctx = (MainViewModel)this.DataContext;
        ctx.PropertyChanged += on_binding_ctx;

    }

    public void on_binding_ctx(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "FilteredFiles") { on_update_columns(); }
        if (e.PropertyName == "UpdateColumns") { on_update_columns(); }
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

                string type = Path.GetExtension(path);

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

        if (treeview)
        {
            TreeviewOn.IsVisible = true;
            TreeviewOff.IsVisible = false;
            MainGrid.ColumnDefinitions[0] = new ColumnDefinition(200, GridUnitType.Pixel);
        }
        else
        {
            TreeviewOn.IsVisible = false;
            TreeviewOff.IsVisible = true;
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

    public void on_theme_dark(object sender, RoutedEventArgs e)
    {
        darkmode = true;
        var MaterialThemeStyles = Avalonia.Application.Current!.LocateMaterialTheme<MaterialTheme>();
        MaterialThemeStyles.BaseTheme = Material.Styles.Themes.Base.BaseThemeMode.Dark;
        MaterialThemeStyles.PrimaryColor = Material.Colors.PrimaryColor.Grey;

        ModeDayIcon.IsVisible = false;
        ModeNightIcon.IsVisible = true;
        
        set_theme_colors();
        update_row_color();
    }

    private void on_theme_light(object sender, RoutedEventArgs e)
    {
        darkmode = false;
        var MaterialThemeStyles = Avalonia.Application.Current!.LocateMaterialTheme<MaterialTheme>();
        MaterialThemeStyles.BaseTheme = Material.Styles.Themes.Base.BaseThemeMode.Light;
        MaterialThemeStyles.PrimaryColor = Material.Colors.PrimaryColor.Blue;

        ModeDayIcon.IsVisible = true;
        ModeNightIcon.IsVisible = false;

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

    private void init_startup(object sender, RoutedEventArgs e)
    {
        get_datacontext();
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

    private void on_toggle_preview(object sender, RoutedEventArgs e)
    {
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

        set_preview_request(null, null);

        MainGrid.ColumnDefinitions[1] = new ColumnDefinition(1f, GridUnitType.Star);
        MainGrid.ColumnDefinitions[2] = new ColumnDefinition(val1, GridUnitType.Pixel);
        MainGrid.ColumnDefinitions[3] = new ColumnDefinition(val2, GridUnitType.Star);


        if (previewMode == false)
        {
            ctx.clear_preview_file();
        }
    }

    private void set_preview_request(object sender, RoutedEventArgs r)
    {
        if (previewMode == true)
        {
            FileData file = (FileData)FileGrid.SelectedItem;

            if (file != null)
            {
                preview_request = file.Sökväg;

                int QFak = (int)PreviewQuality.Value;
                if (PreviewWorker_busy == false)
                {
                    try
                    {
                        PreviewWorker_busy = true;
                        PreviewWorker.RunWorkerAsync(QFak);
                    }
                    catch (Exception e) { }
                }
            }

        }
    }

    private void init_PreviewWorker()
    {
        PreviewWorker.DoWork += PreviewWorker_DoWork;
        PreviewWorker.RunWorkerCompleted += PreviewWorker_RunWorkerCompleted;
        PreviewWorker.WorkerSupportsCancellation = true;
    }

    private void PreviewWorker_DoWork(object sender, DoWorkEventArgs e)
    {
        ctx.clear_preview_file();

        int QFak = (int)e.Argument;
        string current_task = preview_request;
        ctx.create_preview_file(current_task, QFak);
        preview_current = current_task;
    }

    private void PreviewWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
        PreviewWorker_busy = false;

        if (preview_current != preview_request)
        {
            PreviewWorker_busy = true;
            PreviewWorker.RunWorkerAsync(4);
        }
        else
        {
            ctx.start_preview_page();
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

    private void on_scroll_preview(object sender, PointerWheelEventArgs args)
    {

        if (!args.KeyModifiers.HasFlag(KeyModifiers.Control))
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

        if (args.KeyModifiers.HasFlag(KeyModifiers.Control)) 

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

    private void on_toggle_dualmode(object sender, RoutedEventArgs e)
    {
        if (DualMode.IsChecked == true)
        {
            ScrollSlider.TickFrequency = 2;
        }
        if (DualMode.IsChecked == false)
        {
            ScrollSlider.TickFrequency = 1;
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

    private void on_project_selected(object sender, RoutedEventArgs e)
    {
        object selected = ProjectList.SelectedItem;

        if (selected != null) 
        {
            string name = selected.ToString();
            ctx.select_project(name);
        }

        on_update_columns();
    }

    private void on_type_selected(object sender, RoutedEventArgs e)
    {
        object type = TypeList.SelectedItem;

        if (type != null)
        {
            ctx.select_type(type.ToString());
        }

        on_update_columns();
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
        string type = menuItem.Tag.ToString();

        ctx.edit_type(type);
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
        if (StatusLabel.Content == "Ready")
        {
            StatusLabel.Content = "Opening path";
            ctx.open_path();
            StatusLabel.Content = "Ready";
        }

    }

    private void on_open_file(object sender, RoutedEventArgs e)
    {
        if (StatusLabel.Content == "Ready")
        {
            StatusLabel.Content = "Opening file";

            ctx.open_files();

            StatusLabel.Content = "Ready";
        }
    }

    private void on_open_metafile(object sender, RoutedEventArgs e)
    {
        if (StatusLabel.Content == "Ready")
        {
            StatusLabel.Content = "Opening metafile";

            ctx.open_meta();

            StatusLabel.Content = "Ready";
        }
    }

    private void on_open_dwg(object sender, RoutedEventArgs e)
    {
        if (StatusLabel.Content == "Ready")
        {

            StatusLabel.Content = "Opening Drawing";
            ctx.open_dwg();
            StatusLabel.Content = "Ready";
            
        }
    }

    private async void on_load_file(object sender, RoutedEventArgs e)
    {
        StatusLabel.Content = "Loading file";
        await ctx.LoadFile(this);
        StatusLabel.Content = "Ready";
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
        string path = "C:\\FIlePathManager\\Projects.json";
        await ctx.SaveFileAuto(path);
        StatusLabel.Content = "Ready";
    }

    private void on_move_files(object sender, RoutedEventArgs e)
    {
        string projectname = MoveFileToProjectName.Text;
        ctx.move_files(projectname);
    }

    private void on_check_toggle(object sender, RoutedEventArgs e)
    {
        //var checkbox = sender as CheckBox;
        //int nr = Int32.Parse(checkbox.Tag.ToString());

        //FileGrid.Columns[nr].IsVisible = !FileGrid.Columns[nr].IsVisible;

        Debug.WriteLine(FileGrid.Columns[0].IsVisible);

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

}

