using BombaProMaxWPF.Models;
using System.Collections.ObjectModel;
using System.Windows;
using Wpf.Ui.Controls;

namespace BombaProMaxWPF.Views.InfrastructurePages.Sections.Reservoirs;

public partial class CalibrationTableDialog : FluentWindow
{
    public ObservableCollection<ReservoirCalibrationDto> Rows { get; }
    public string ReservoirNumero { get; }

    public CalibrationTableDialog(
        string reservoirNumero,
        IEnumerable<ReservoirCalibrationDto> rows)
    {
        // DataContext must be set before InitializeComponent so XAML bindings resolve correctly
        ReservoirNumero = reservoirNumero;
        Rows = new ObservableCollection<ReservoirCalibrationDto>(rows);
        DataContext = this;
        InitializeComponent();
    }

    private void OnCloseClick(object sender, RoutedEventArgs e) => Close();
}
