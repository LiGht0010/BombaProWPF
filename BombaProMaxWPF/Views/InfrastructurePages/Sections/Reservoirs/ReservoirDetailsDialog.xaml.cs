using BombaProMaxWPF.Models;
using BombaProMaxWPF.ViewModels;
using System.Windows;
using Wpf.Ui.Controls;

namespace BombaProMaxWPF.Views.InfrastructurePages.Sections.Reservoirs;

public partial class ReservoirDetailsDialog : FluentWindow
{
    public ReservoirDetailsViewModel ViewModel { get; }

    public ReservoirDetailsDialog(ReservoirDto reservoir)
    {
        InitializeComponent();
        ViewModel = new ReservoirDetailsViewModel(reservoir);
        DataContext = ViewModel;
        Loaded += async (_, _) => await ViewModel.EnsureLoadedAsync();
    }

    private void OnCloseClick(object sender, RoutedEventArgs e) => Close();

    private void OnShowCalibrationTableClick(object sender, RoutedEventArgs e)
    {
        var dialog = new CalibrationTableDialog(
            ViewModel.Reservoir.Numero,
            ViewModel.CalibrationRows)
        {
            Owner = this
        };
        dialog.ShowDialog();
    }
}
