using FourniPro.Models;
using System.Windows;
using Wpf.Ui.Controls;

namespace FourniPro.Views.InfrastructurePages.Sections.PaiementsFournisseur;

public partial class DetailPaiementFournisseurDialog : FluentWindow
{
    public bool ShouldEdit { get; private set; }

    public DetailPaiementFournisseurDialog(PaiementFournisseurDto dto)
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
