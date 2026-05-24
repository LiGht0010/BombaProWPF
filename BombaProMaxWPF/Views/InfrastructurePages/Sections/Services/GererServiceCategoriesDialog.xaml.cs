using BombaProMaxWPF.ViewModels;
using System.Windows;
using System.Windows.Input;
using Wpf.Ui.Controls;

namespace BombaProMaxWPF.Views.InfrastructurePages.Sections.Services;

public partial class GererServiceCategoriesDialog : FluentWindow
{
    public GererServiceCategoriesViewModel ViewModel { get; }

    public GererServiceCategoriesDialog()
    {
        InitializeComponent();
        ViewModel = new GererServiceCategoriesViewModel();
        DataContext = ViewModel;
        Loaded += async (_, _) => await ViewModel.LoadAsync();
    }

    private void OnCloseClick(object sender, RoutedEventArgs e) => Close();

    /// <summary>Enter in the "new category" TextBox triggers Add.</summary>
    private async void NewNomBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && ViewModel.AddCommand.CanExecute(null))
            await ViewModel.AddCommand.ExecuteAsync(null);
    }

    /// <summary>Enter commits, Escape cancels the inline edit.</summary>
    private async void EditBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (sender is System.Windows.Controls.TextBox tb &&
            tb.Tag is ServiceCategorieRowItem row)
        {
            if (e.Key == Key.Enter)
                await ViewModel.CommitEditCommand.ExecuteAsync(row);
            else if (e.Key == Key.Escape)
                ViewModel.CancelEditCommand.Execute(row);
        }
    }
}
