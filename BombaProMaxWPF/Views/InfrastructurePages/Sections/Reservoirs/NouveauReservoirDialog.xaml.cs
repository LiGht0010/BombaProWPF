using BombaProMaxWPF.ViewModels;
using System.Windows;
using Wpf.Ui.Controls;

namespace BombaProMaxWPF.Views.InfrastructurePages.Sections.Reservoirs;

public partial class NouveauReservoirDialog : FluentWindow
{
    public NouveauReservoirViewModel ViewModel { get; }

    public NouveauReservoirDialog()
    {
        InitializeComponent();
        ViewModel = new NouveauReservoirViewModel();
        DataContext = ViewModel;
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private async void OnSaveClick(object sender, RoutedEventArgs e)
    {
        if (ViewModel.SaveCommand.CanExecute(null))
            await ViewModel.SaveCommand.ExecuteAsync(null);

        if (ViewModel.Result is not null)
        {
            DialogResult = true;
            Close();
        }
    }
}
