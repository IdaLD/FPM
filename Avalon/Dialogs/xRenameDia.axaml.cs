using Avalon.ViewModels;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Avalon.Dialog;

public partial class xRenameDia : Window
{
    public xRenameDia()
    {
        InitializeComponent();

        KeyDown += CloseKey;

    }

    private void OnRenameProject(object sender, RoutedEventArgs e)
    {
        if (NewProjectName.Text != null)
        {
            MainViewModel ctx = (MainViewModel)this.DataContext;
            ctx.rename_project(NewProjectName.Text.ToString());
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