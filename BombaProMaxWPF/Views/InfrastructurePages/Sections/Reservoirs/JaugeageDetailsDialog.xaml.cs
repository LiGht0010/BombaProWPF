using BombaProMaxWPF.ViewModels;
using System.Windows;
using Wpf.Ui.Controls;

namespace BombaProMaxWPF.Views.InfrastructurePages.Sections.Reservoirs;

public partial class JaugeageDetailsDialog : FluentWindow
{
    public JaugeageDetailsViewModel ViewModel { get; }

    public JaugeageDetailsDialog(int jaugeageId)
    {
        InitializeComponent();
        ViewModel = new JaugeageDetailsViewModel();
        DataContext = ViewModel;
        Loaded += async (_, _) => await ViewModel.LoadAsync(jaugeageId);
    }

    private void OnCloseClick(object sender, RoutedEventArgs e) => Close();
}
