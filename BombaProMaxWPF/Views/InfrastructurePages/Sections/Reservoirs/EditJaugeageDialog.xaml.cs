using BombaProMaxWPF.ViewModels;
using System.Windows;
using Wpf.Ui.Controls;

namespace BombaProMaxWPF.Views.InfrastructurePages.Sections.Reservoirs;

public partial class EditJaugeageDialog : FluentWindow
{
    public EditJaugeageViewModel ViewModel { get; }

    public EditJaugeageDialog(int jaugeageId)
    {
        InitializeComponent();
        ViewModel = new EditJaugeageViewModel();
        DataContext = ViewModel;
        Loaded += async (_, _) => await ViewModel.LoadAsync(jaugeageId);
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private async void OnSaveClick(object sender, RoutedEventArgs e)
    {
        await ViewModel.SaveCommand.ExecuteAsync(null);
        if (ViewModel.ErrorMessage is null)
        {
            DialogResult = true;
            Close();
        }
    }
}
