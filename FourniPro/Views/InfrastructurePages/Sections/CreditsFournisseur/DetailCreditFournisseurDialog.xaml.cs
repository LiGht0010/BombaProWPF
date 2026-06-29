using FourniPro.Models;
using System.Windows;
using Wpf.Ui.Controls;

namespace FourniPro.Views.InfrastructurePages.Sections.CreditsFournisseur;

public partial class DetailCreditFournisseurDialog : FluentWindow
{
    public bool ShouldEdit { get; private set; }

    public DetailCreditFournisseurDialog(CreditFournisseurDto dto)
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
