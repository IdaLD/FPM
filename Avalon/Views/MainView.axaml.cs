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
using Newtonsoft.Json.Bson;
using System.Threading;
using System.IO;
using System.Diagnostics;
using Avalonia.Controls.Generators;
using System.Collections.Generic;
using iText.Commons.Bouncycastle.Asn1.X509;
using System.Drawing;


namespace Avalon.Views;

public partial class MainView : UserControl, INotifyPropertyChanged
{
    public MainView() 
    {
        InitializeComponent();

        FileGrid.AddHandler(DataGrid.DoubleTappedEvent, on_open_file);

        FetchMetadata.AddHandler(Button.ClickEvent, on_fetch_full_meta);

        FileGrid.AddHandler(DataGrid.SelectionChangedEvent, set_preview_request);

        FileGrid.AddHandler(DataGrid.SelectionChangedEvent, select_file);

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
        

        init_columns();
        init_MetaWorker();
        init_PreviewWorker();
        setup_preview_transform();

        StatusLabel.Content = "Ready";
    }

    public int SelectedIndex = 0;

    public string currentProject = "";
    public string currentType = "";

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

    public string preview_request = "";
    public string preview_current = "";

    public bool darkmode = true;

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
    }

    public void on_theme_dark(object sender, RoutedEventArgs e)
    {
        darkmode = true;
        var MaterialThemeStyles = Application.Current!.LocateMaterialTheme<MaterialTheme>();
        MaterialThemeStyles.BaseTheme = Material.Styles.Themes.Base.BaseThemeMode.Dark;
        MaterialThemeStyles.PrimaryColor = Material.Colors.PrimaryColor.Grey;

        
        
        set_theme_colors();
        on_refresh_table();
    }

    public void on_theme_light(object sender, RoutedEventArgs e)
    {
        darkmode = false;
        var MaterialThemeStyles = Application.Current!.LocateMaterialTheme<MaterialTheme>();
        MaterialThemeStyles.BaseTheme = Material.Styles.Themes.Base.BaseThemeMode.Light;
        MaterialThemeStyles.PrimaryColor = Material.Colors.PrimaryColor.Blue;

        set_theme_colors();
        on_refresh_table();
    }

    public void set_theme_colors()
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
            on_project_refresh();
        }
        catch { }
    }

    private void on_toggle_preview(object sender, RoutedEventArgs e)
    {
        previewMode = !previewMode;

        float a = 1;
        float b = 0;
        float c = 0;


        if (previewMode == true) 
        {
            b = 5;
            c = 3.2f;

            set_preview_request(null, null); 
        }

        MainGrid.ColumnDefinitions.Clear();
        GridLength clmn1 = new GridLength(a, GridUnitType.Star);
        GridLength clmn2 = new GridLength(b, GridUnitType.Pixel);
        GridLength clmn3 = new GridLength(c, GridUnitType.Star);

        MainGrid.ColumnDefinitions.Add(new ColumnDefinition(clmn1));
        MainGrid.ColumnDefinitions.Add(new ColumnDefinition(clmn2));
        MainGrid.ColumnDefinitions.Add(new ColumnDefinition(clmn3));

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
        }
        if (Lockedstatus.IsChecked == false)
        {
            RemoveProjectMenu.IsEnabled = true;
            RemoveFileMenu.IsEnabled = true;
        }
    }

    private void select_file(object sender, RoutedEventArgs e)
    {
        if (FileGrid.SelectedItem != null)
        {
            FileData selectedFile = (FileData)FileGrid.SelectedItem;
            SelectedFileName.Text = selectedFile.Namn;
        }
    }

    private void on_copy_filename(object sender, RoutedEventArgs e)
    {
        IList files = FileGrid.SelectedItems;
        ctx.CopyFilenameToClipboard(this, files);
    }

    private void on_copy_listview(object sender, RoutedEventArgs e)
    {
        bool?[] checkstate = new bool?[7];

        IList files = FileGrid.SelectedItems;

        checkstate =
            [
            Column0.IsChecked,
            Column1.IsChecked,
            Column2.IsChecked,
            Column3.IsChecked,
            Column4.IsChecked,
            Column5.IsChecked,
            Column6.IsChecked,
            Column7.IsChecked,
            Column8.IsChecked,
            Column9.IsChecked,
            Column10.IsChecked,
            Column11.IsChecked,
            Column12.IsChecked,
            Column13.IsChecked
            ];

        ctx.CopyListviewToClipboard(this, files, checkstate);
    }

    private void init_columns()
    {
        int nval = FileGrid.Columns.Count();

        for (int i = 0; i < nval; i++)
        {
            FileGrid.Columns[i].IsVisible = false;
        }

        Column0.IsChecked = true;
        Column1.IsChecked = true;
        Column2.IsChecked = true;

        Column6.IsChecked = true;
        Column7.IsChecked = true;
        Column8.IsChecked = true;
        Column9.IsChecked = true;
        Column10.IsChecked = true;

    }

    private void ColumnCheck(object sender, RoutedEventArgs e)
    {
        var item = sender as CheckBox;
        int column = Int32.Parse(item.Tag.ToString());

        FileGrid.Columns[column].IsVisible = true;
    }

    private void ColumnUncheck(object sender, RoutedEventArgs e)
    {
        var item = sender as CheckBox;
        int column = Int32.Parse(item.Tag.ToString());

        FileGrid.Columns[column].IsVisible = false;
    }

    void OnMenuOpen(object sender, RoutedEventArgs e)
    {
        on_open_file(sender, e);
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
    

    public void on_project_selected(object sender, RoutedEventArgs e)
    {

        object selected = ProjectList.SelectedItem;
        if (selected != null) 
        {
            currentProject = (string)selected;
            SelectedProjectLabel.Content = currentProject;

            on_refresh_table();
        }
    }

    public void on_type_selected(object sender, RoutedEventArgs e)
    {
        object selected = TypeList.SelectedItem;

        if (selected != null)
        {
            currentType = (string)selected;
            SelectedTypeLabel.Content = currentType;

            on_refresh_table();
        }
    }

    public void on_refresh_table()
    {
        ctx.set_info_file(currentProject);
        ctx.UpdateLists(currentProject, currentType);
        on_update_columns();
    }

    void on_project_refresh()
    {

        currentProject = ctx.Projects.FirstOrDefault();
        currentType = ctx.Types.FirstOrDefault();

        SelectedProjectLabel.Content = currentProject;
        SelectedTypeLabel.Content = currentType;

        on_refresh_table();

    }

    public void EditColor(object sender, RoutedEventArgs e)
    {

        var menuItem = sender as MenuItem;
        string color = menuItem.Tag.ToString();

        IList items = FileGrid.SelectedItems;

        ctx.add_color(color, items);

        deselect_items();
        update_row_color();

    }

    public void EditType(object sender, RoutedEventArgs e)
    {

        var menuItem = sender as MenuItem;
        string type = menuItem.Tag.ToString();

        IList items = FileGrid.SelectedItems;

        ctx.add_type(type, items);
        ctx.UpdateTypes();

        on_refresh_table();

    }

    public void deselect_items()
    {
        FileGrid.SelectedItem = null;
    }

    public void on_clear_files(object sender, RoutedEventArgs e)
    {

        IList items = FileGrid.SelectedItems;

        ctx.clear_all(items);

        deselect_items();
        update_row_color();
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

        IList items = FileGrid.SelectedItems;

        ctx.add_tag(currentMode, items);

        deselect_items();
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
        ctx.remove_project(currentProject);
        on_project_refresh();
    }

    private void on_rename_project(object sender, RoutedEventArgs e)
    {
        string newName = NewProjectName.Text.ToString();

        ctx.rename_project(currentProject, newName);
        SelectedProjectLabel.Content = newName.ToString();
    }

    private void on_add_file(object sender, RoutedEventArgs e)
    {
        ctx.AddFile(currentProject, this);

        currentType = ctx.Types.FirstOrDefault();
        SelectedTypeLabel.Content = currentType;
        //on_project_refresh();
        //on_refresh_table();

    }

    private void on_fetch_single_meta(object sender, RoutedEventArgs e)
    {
        ProgressStatus.Content = "Fetching Metadata";

        IList files = FileGrid.SelectedItems;

        ctx.SelectFiles(true, files);
        MetaWorker.RunWorkerAsync();
    }

    private void on_clear_meta(object sender, RoutedEventArgs e)
    {
        IList files = FileGrid.SelectedItems;

        ctx.ClearMeta(files);
    }

    private void on_fetch_full_meta(object sender, RoutedEventArgs e)
    {
        ProgressStatus.Content = "Fetching Metadata";

        ctx.SelectFiles(false, null);
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
            IList files = FileGrid.SelectedItems;

            ctx.OpenPath(files);
            StatusLabel.Content = "Ready";
        }

    }

    private void on_open_file(object sender, EventArgs e)
    {
        if (StatusLabel.Content == "Ready")
        {
            StatusLabel.Content = "Opening file";
            IList files = FileGrid.SelectedItems;

            ctx.OpenFile(files, "PDF");
            StatusLabel.Content = "Ready";
        }
    }

    private void on_open_metafile(object sender, RoutedEventArgs e)
    {
        if (StatusLabel.Content == "Ready")
        {
            StatusLabel.Content = "Opening metafile";
            IList files = FileGrid.SelectedItems;

            ctx.OpenFile(files, "MD");
            StatusLabel.Content = "Ready";
        }
    }

    private void on_open_dwg(object sender, RoutedEventArgs e)
    {
        if (StatusLabel.Content == "Ready")
        {

            StatusLabel.Content = "Opening Drawing";
            FileData file = (FileData)FileGrid.SelectedItem;

            if (file.Filtyp == "Drawing")
            {
                ctx.OpenDwg(file);
            }
            
            StatusLabel.Content = "Ready";
            
        }
    }

    private async void on_load_file(object sender, RoutedEventArgs e)
    {
        StatusLabel.Content = "Loading file";

        await ctx.LoadFile(this);

        on_project_refresh();
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

    private void on_remove_files(object sender, RoutedEventArgs e)
    {
        IList items      = FileGrid.SelectedItems;

        ctx.remove_files(items);
        on_refresh_table();
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

}

