using Avalon.ViewModels;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Metadata;
using Org.BouncyCastle.Asn1.BC;
using Org.BouncyCastle.Crypto.Signers;

namespace Avalon.Dialog;

public partial class xRenameDia : Window
{
    public xRenameDia()
    {
        InitializeComponent();

        KeyDown += CloseKey;

    }

    public void SetCurrentName(string name)
    {
        NewNameInput.Text = name;
    }

    private void AcceptRename(object sender, RoutedEventArgs e)
    {
        if (NewNameInput.Text != null && NewNameInput.Text.Length > 0)
        {
            MainViewModel ctx = (MainViewModel)this.DataContext;
            ctx.RenameOriginal(NewNameInput.Text.ToString());
            ctx.SaveFileAuto();
        }

        this.Close();
    }

    private void OnClose(object sender, RoutedEventArgs e)
    {
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