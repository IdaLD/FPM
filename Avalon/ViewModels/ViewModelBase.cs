using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace Avalon.ViewModels;

public class ViewModelBase : ObservableObject
{
    public static class Globals
    {
        public static List<FileData> storedFiles = new List<FileData>();
        public static List<string> projects = new List<string>();
        public static string focusedList = new string("New Project");

    }
}
