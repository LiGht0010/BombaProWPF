using FourniPro.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;

namespace FourniPro.Views.InfrastructurePages.Sections.Voyages;

public partial class VoyagesSection : UserControl
{
    public VoyagesSectionViewModel ViewModel { get; }

    public VoyagesSection()
    {
        InitializeComponent();
        ViewModel = new VoyagesSectionViewModel();
        ViewModel.OpenAddDialog = OpenAddVoyage;
        DataContext = ViewModel;
    }

    private void OpenAddVoyage()
    {
        try
        {
            var dlg = new NouveauVoyageDialog
            {
                Owner = Application.Current?.MainWindow
            };
            dlg.ShowDialog();
            if (dlg.ViewModel.Saved)
                _ = ViewModel.RefreshAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Error opening dialog:\n\n{ex.GetType().Name}: {ex.Message}\n\n{ex.StackTrace}",
                "Dialog Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}
