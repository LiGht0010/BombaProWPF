using FourniPro.Models;
using System.Windows;
using Wpf.Ui.Controls;

namespace FourniPro.Views.InfrastructurePages.Sections.Fournisseurs;

public partial class DetailFournisseurDialog : FluentWindow
{
    /// <summary>
    /// Set to <see langword="true"/> when the user clicks "Modifier".
    /// The caller opens <see cref="EditFournisseurDialog"/> immediately after this dialog closes.
    /// </summary>
    public bool ShouldEdit { get; private set; }

    public DetailFournisseurDialog(FournisseurDto dto)
    {
        InitializeComponent();
        DataContext = dto;
    }

    private void OnCloseClick(object sender, RoutedEventArgs e) => Close();

    private void OnEditClick(object sender, RoutedEventArgs e)
    {
        ShouldEdit = true;
        Close();
    }
}
