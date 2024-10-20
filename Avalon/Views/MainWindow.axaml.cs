using Avalon.ViewModels;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using System.ComponentModel;

namespace Avalon.Views;

public partial class MainWindow : Window, INotifyPropertyChanged
{
    private KeyGesture KeyEnter;
    private KeyGesture KeyEscape;

    public MainWindow()
    {
        InitializeComponent();
    }

    bool confirmClose = false;

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (!confirmClose)
        {
            e.Cancel = true;
            OnOpenConfirm();
        }
        else
        {
            e.Cancel = false;
        }

    }

    public void OnOpenConfirm()
    {

        ConfirmLeave.IsVisible = true;

        HotKeyManager.SetHotKey(SaveYes, new KeyGesture(Key.Enter));
        HotKeyManager.SetHotKey(SaveCancel, new KeyGesture(Key.Escape));
    }

    public async void on_save_before_close(object sender, RoutedEventArgs args)
    {
        confirmClose = true;
        MainViewModel ctx = (MainViewModel)this.DataContext;
        string path = "C:\\FIlePathManager\\Projects.json";

        await ctx.SaveFileAuto(path);

        on_leave(null, null);

    }

    public void on_leave(object sender, RoutedEventArgs args)
    {
        confirmClose = true;
        
        Close();
    }


    public void on_cancel(object sender, RoutedEventArgs args)
    {
        ConfirmLeave.IsVisible = false;

        HotKeyManager.SetHotKey(SaveYes, null);
        HotKeyManager.SetHotKey(SaveCancel, null);
    }


}