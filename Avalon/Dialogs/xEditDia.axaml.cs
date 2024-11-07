using Avalon.ViewModels;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Avalon.Dialog;

public partial class xEditDia : Window
{
    public xEditDia()
    {
        InitializeComponent();

        ProjectCategory.AddHandler(ComboBox.LoadedEvent, SetupCategory);

        KeyDown += CloseKey;

    }

    private void SetupCategory(object sender, RoutedEventArgs e)
    {
        MainViewModel ctx = (MainViewModel)this.DataContext;

        string cat = ctx.ProjectsVM.CurrentProject.Category;

        ComboBoxItem comboBoxItem = null;

        foreach( ComboBoxItem item in ProjectCategory.Items)
        {
            if(item.Content.ToString() == cat)
            {
                comboBoxItem = item;
            }
        }

        ProjectCategory.SelectedItem = comboBoxItem;

    }

    private void OnRenameProject(object sender, RoutedEventArgs e)
    {
        if (ProjectName.Text != null)
        {
            MainViewModel ctx = (MainViewModel)this.DataContext;
            ctx.rename_project(ProjectName.Text.ToString());
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