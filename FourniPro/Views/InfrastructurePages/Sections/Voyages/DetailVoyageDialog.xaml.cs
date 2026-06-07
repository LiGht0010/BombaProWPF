using FourniPro.Models;
using FourniPro.ViewModels;
using System.Windows;
using Wpf.Ui.Controls;

namespace FourniPro.Views.InfrastructurePages.Sections.Voyages;

public partial class DetailVoyageDialog : FluentWindow
{
    public DetailVoyageViewModel ViewModel { get; }

    /// <summary>
    /// Set to <see langword="true"/> when the user clicks "Modifier".
    /// The caller opens the edit dialog immediately after this dialog closes.
    /// </summary>
    public bool ShouldEdit { get; private set; }

    public DetailVoyageDialog(VoyageCardItem voyage)
    {
        InitializeComponent();
        ViewModel   = new DetailVoyageViewModel(voyage);
        DataContext = ViewModel;
    }

    protected override async void OnContentRendered(EventArgs e)
    {
        base.OnContentRendered(e);
        await ViewModel.LoadAsync();
    }

    private void OnCloseClick(object sender, RoutedEventArgs e) => Close();

    private void OnEditClick(object sender, RoutedEventArgs e)
    {
        ShouldEdit = true;
        Close();
    }
}

