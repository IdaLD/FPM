using Avalonia.Controls;
using System.Collections.Generic;
using System;
using Newtonsoft.Json;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using System.Linq;
using System.IO;
using Avalon.ViewModels;
using System.Diagnostics;
using Avalonia.Controls.Primitives;
using System.Collections;

namespace Avalon.Views;

public partial class MainView : UserControl
{
    public MainView() 
    {
        InitializeComponent();

        Removefiles.AddHandler(Button.ClickEvent, OnButtonClick);

        DrawingGrid.AddHandler(DataGrid.SelectionChangedEvent, OnDrawingGridSelected);
        DocumentGrid.AddHandler(DataGrid.SelectionChangedEvent, OnDocumentGridSelected);

        ProjectSelection.AddHandler(ComboBox.SelectionChangedEvent, OnProjectSelectionChange);

    }

    public string SelectedType = null;

    private void OnButtonClick(object sender, EventArgs e)
    {
        IList drawings = DrawingGrid.SelectedItems;
        IList documents = DocumentGrid.SelectedItems;

        var ctx = (MainViewModel)this.DataContext;
        ctx.AddDrawings(drawings, documents, SelectedType);
    }

    private void OnDrawingGridSelected(object sender, EventArgs e)
    {
        SelectedType = "Drawing";
    }

    private void OnDocumentGridSelected(object sender, EventArgs e)
    {
        SelectedType = "Document";
    }

    private void OnProjectSelectionChange(object sender, EventArgs e)
    {

    }
}

