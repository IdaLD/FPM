using Avalon.Views;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace Avalon.ViewModels;


public class MainViewModel : ViewModelBase
{
    public ObservableCollection<Files> FileStorage { get; }

    public MainViewModel()
    {
        var fileStorage = new List<Files>
        {
            //new Files("Neil", "Armstrong"),
        };
        FileStorage = new ObservableCollection<Files>(fileStorage);
    }

}

