using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Avalon.Dialog;

public partial class xColorDia : Window, INotifyPropertyChanged
{
    public xColorDia()
    {
        InitializeComponent();

        FontCombo.ItemsSource = FontManager.Current.SystemFonts.Select(x => x.Name).ToList();
        FontSizeCombo.ItemsSource = new List<int>() { 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24 };

        KeyDown += CloseKey;
    }

    public void ResetThemeColors(object sender, RoutedEventArgs e)
    {
        BackgroundColorPickerDark.Color = Color.Parse("#333333");
        AccentColorPickerDark.Color = Color.Parse("#444444");

        BackgroundColorPickerLight.Color = Color.Parse("#dfe6e9");
        AccentColorPickerLight.Color = Color.Parse("#999999");

        FontCombo.SelectedValue = "Default";
        FontSizeCombo.SelectedValue = 16;

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