
using Avalon.Model;
using Avalon.ViewModels;
using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.IO;
using System.Reflection;

namespace Avalon.Dialog;

public partial class xProgDia : Window
{
    public xProgDia()
    {
        InitializeComponent();

        Loaded += SetupInfo;
    }

    private void SetupInfo(object sender, RoutedEventArgs e)
    {

        MainViewModel ctx = (MainViewModel)this.DataContext;

        CompiledDate.Content = File.GetLastWriteTime(Assembly.GetExecutingAssembly().CodeBase.Substring(8));

        LastSaved.Content = File.GetLastWriteTime("C:\\FIlePathManager\\Projects.json");


        NrProjects.Content = ctx.ProjectsVM.StoredProjects.Count;


        int nrFiles = 0;

        foreach (ProjectData project in ctx.ProjectsVM.StoredProjects)
        {
            nrFiles = nrFiles + project.StoredFiles.Count;
        }

        NrFiles.Content = nrFiles;


    }


}