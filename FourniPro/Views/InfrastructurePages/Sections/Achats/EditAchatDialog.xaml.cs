using FourniPro.Models;
using FourniPro.ViewModels;
using System.Windows;
using Wpf.Ui.Controls;

namespace FourniPro.Views.InfrastructurePages.Sections.Achats;

public partial class EditAchatDialog : FluentWindow
{
    public EditAchatViewModel ViewModel { get; }

    public EditAchatDialog(AchatDto achat)
    {
        InitializeComponent();
        ViewModel   = new EditAchatViewModel(achat);
        DataContext = ViewModel;
    }

    private void OnCancelClick(object sender, RoutedEventArgs e) => Close();

    protected override void OnContentRendered(EventArgs e)
    {
        base.OnContentRendered(e);

        _ = ViewModel.LoadLookupsAsync();

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
