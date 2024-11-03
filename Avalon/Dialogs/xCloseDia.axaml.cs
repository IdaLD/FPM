using Avalon.ViewModels;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalon.Views;

namespace Avalon.Dialog;

public partial class xCloseDia : Window
{
    private MainWindow mainWindow;

    public xCloseDia()
    {
        InitializeComponent();
    }

    public void SetMainWindow(MainWindow mainW)
    {
        mainWindow = mainW;
    }

    public async void SaveBeforeClose(object sender, RoutedEventArgs args)
    {

        MainViewModel ctx = (MainViewModel)this.DataContext;
        string path = "C:\\FIlePathManager\\Projects.json";

        await ctx.SaveFileAuto(path);

        OnLeave(null, null);

    }

    public void OnLeave(object sender, RoutedEventArgs args)
    {
        mainWindow.confirmLeave = false;
        mainWindow.Close();
    }


    public void OnCancel(object sender, RoutedEventArgs args)
    {
        Close();
    }


}