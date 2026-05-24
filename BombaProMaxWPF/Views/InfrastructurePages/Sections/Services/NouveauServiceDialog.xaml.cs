using BombaProMaxWPF.ViewModels;
using System.Windows;
using Wpf.Ui.Controls;

namespace BombaProMaxWPF.Views.InfrastructurePages.Sections.Services;

public partial class NouveauServiceDialog : FluentWindow
{
    public NouveauServiceViewModel ViewModel { get; }

    public NouveauServiceDialog()
    {
        InitializeComponent();
        ViewModel = new NouveauServiceViewModel();
        DataContext = ViewModel;
        Loaded += async (_, _) => await ViewModel.LoadCategoriesAsync();
    }

    private void OnCancelClick(object sender, RoutedEventArgs e) => Close();

    protected override async void OnContentRendered(EventArgs e)
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
