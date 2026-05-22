using BombaProMaxWPF.ViewModels;
using System.Windows;
using Wpf.Ui.Controls;

namespace BombaProMaxWPF.Views.InfrastructurePages.Sections.Reservoirs;

public partial class SaisirJaugeageGlobalDialog : FluentWindow
{
    public SaisirJaugeageGlobalViewModel ViewModel { get; }

    public SaisirJaugeageGlobalDialog()
    {
        InitializeComponent();
        ViewModel = new SaisirJaugeageGlobalViewModel();
        DataContext = ViewModel;
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.LoadAsync();
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private async void OnSaveClick(object sender, RoutedEventArgs e)
    {
        // Run the bound command then resolve DialogResult based on Result.
        if (ViewModel.SaveCommand.CanExecute(null))
        {
            await ViewModel.SaveCommand.ExecuteAsync(null);
        }
        if (ViewModel.Result is not null)
        {
            DialogResult = true;
            Close();
        }
    }
}
