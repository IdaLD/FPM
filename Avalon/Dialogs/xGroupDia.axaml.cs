using Avalon.ViewModels;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Avalon.Dialog;

public partial class xGroupDia : Window
{
    public xGroupDia()
    {
        InitializeComponent();

        KeyDown += CloseKey;

    }

    private void OnGroupProject(object sender, RoutedEventArgs e)
    {
        if (ProjectGroupInput.Text != null)
        {
            MainViewModel ctx = (MainViewModel)this.DataContext;
            ctx.SetGroup(ProjectGroupInput.Text.ToString());
        }

        this.Close();

    }


    private void CloseKey(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            this.Close();
        }
    }

}