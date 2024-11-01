using Avalon.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System.Threading.Tasks;

namespace Avalon.Views;

public partial class PreWindow : Window
{
    public PreWindow()
    {
        InitializeComponent();

    }

    private bool dispose = false;
    private MainViewModel ctx;

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        e.Cancel = true;

        if (!dispose)
        {
            ctx = (MainViewModel)this.DataContext;
            WaitToClose();
        }
        else
        {
            e.Cancel = false;
        }
    }


    private async Task WaitToClose()
    {
        while (ctx.PreviewVM.FileWorkerBusy)
        {
            await Task.Delay(300);
        }

        dispose = true;
        this.Close();
    }

}