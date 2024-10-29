using Avalon.ViewModels;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Avalon.Dialog;

public partial class xNewDia : Window
{
    public xNewDia()
    {
        InitializeComponent();

        KeyDown += CloseKey;

    }

    private void OnNewProject(object sender, RoutedEventArgs e)
    {
        var Name = ProjectName.Text;
        if (Name != null)
        {
            MainViewModel ctx = (MainViewModel)this.DataContext;
            ctx.new_project(Name.ToString());
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