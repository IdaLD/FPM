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
            OpenColorDia();
        }
        else
        {
            e.Cancel = false;
        }
    }

    public void OpenColorDia()
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