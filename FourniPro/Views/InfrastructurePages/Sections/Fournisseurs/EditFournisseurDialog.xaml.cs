using FourniPro.Models;
using FourniPro.ViewModels;
using System.Windows;
using Wpf.Ui.Controls;

namespace FourniPro.Views.InfrastructurePages.Sections.Fournisseurs;

public partial class EditFournisseurDialog : FluentWindow
{
    public EditFournisseurViewModel ViewModel { get; }

    public EditFournisseurDialog(FournisseurDto dto)
    {
        InitializeComponent();
        ViewModel   = new EditFournisseurViewModel(dto);
        DataContext = ViewModel;
    }

    private void OnCancelClick(object sender, RoutedEventArgs e) => Close();

    protected override void OnContentRendered(EventArgs e)
    {
        base.OnContentRendered(e);

        ViewModel.SaveCommand.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(ViewModel.SaveCommand.IsRunning)
                && !ViewModel.SaveCommand.IsRunning
                && ViewModel.Saved)
            {
                Dispatcher.Invoke(Close);
            }
        };
    }
}
