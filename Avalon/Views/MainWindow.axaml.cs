using Avalon.ViewModels;
using Avalonia.Controls;
using Avalon.Dialog;
using System.ComponentModel;


namespace Avalon.Views;

public partial class MainWindow : Window, INotifyPropertyChanged
{

    public bool confirmLeave = true;

    public MainWindow()
    {
        InitializeComponent();
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if(confirmLeave)
        {
            e.Cancel = true;
            OpenClosingDia();
        }
        else
        {
            MainViewModel ctx = (MainViewModel)this.DataContext;

            //if (PreviewWindowOpen)
            //{
            //    ctx.PreviewWindow.Close();
            //}

            e.Cancel = false;
        }
    }

    public void OpenClosingDia()
    {

        var window = new xCloseDia()
        {
            DataContext = (MainViewModel)this.DataContext
        };

        window.SetMainWindow(this);

        window.RequestedThemeVariant = this.ActualThemeVariant;
        window.ShowDialog(this);
    }


}