using Avalon.Model;
using Avalon.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;
using System.Drawing;
using System.IO;
using System.Xml;

namespace Avalon.Dialog;

public partial class xInfoDia : Window
{
    public xInfoDia()
    {
        InitializeComponent();

        Loaded += SetupInfo;
    }

    private void SetupInfo(object sender, RoutedEventArgs e)
    {

        MainViewModel ctx = (MainViewModel)this.DataContext;

        if(ctx.ProjectsVM.CurrentFile != null)
        {
            string path = ctx.ProjectsVM.CurrentFile.Sökväg;

            FileInfo fileInfo = new FileInfo(path);

            NameLabel.Content = fileInfo.Name;
            CreationLabel.Content = fileInfo.CreationTime;
            ReadLabel.Content = fileInfo.LastAccessTime;
            WriteLabel.Content = fileInfo.LastWriteTime;
            SizeLabel.Content = Math.Round((decimal)fileInfo.Length / 1000000, 2) + " Mb";
        }
    }


}