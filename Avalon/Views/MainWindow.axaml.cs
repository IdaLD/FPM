using Avalon.ViewModels;
using Avalonia.Controls;
using Avalonia.Interactivity;
using DialogHostAvalonia;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Avalon.Views;

public partial class MainWindow : Window, INotifyPropertyChanged
{
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
            ConfirmLeave.IsVisible = true;
        }
        else
        {
            e.Cancel = false;
        }

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
    }


}