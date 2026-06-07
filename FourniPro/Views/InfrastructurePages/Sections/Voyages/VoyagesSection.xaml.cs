using FourniPro.Models;
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
        ViewModel.OpenAddDialog    = OpenAddVoyage;
        ViewModel.OpenDetailDialog = OpenDetailVoyage;
        ViewModel.OpenEditDialog   = OpenEditVoyage;
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

    private void OpenDetailVoyage(VoyageCardItem voyage)
    {
        try
        {
            var dlg = new DetailVoyageDialog(voyage)
            {
                Owner = Application.Current?.MainWindow
            };
            dlg.ShowDialog();

            if (dlg.ShouldEdit)
                OpenEditVoyage(voyage);
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

    private void OpenEditVoyage(VoyageCardItem voyage)
    {
        try
        {
            var dlg = new EditVoyageDialog(voyage)
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
