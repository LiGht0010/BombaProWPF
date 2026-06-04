using FourniPro.Models;
using System.Windows;
using Wpf.Ui.Controls;

namespace FourniPro.Views.InfrastructurePages.Sections.Camions;

public partial class DetailCamionDialog : FluentWindow
{
    public bool ShouldEdit { get; private set; }

    public DetailCamionDialog(CamionDto dto)
    {
        InitializeComponent();

        MatriculeHeaderText.Text = dto.Matricule ?? "—";
        MarqueHeaderText.Text    = dto.Marque ?? string.Empty;

        MatriculeText.Text    = dto.Matricule ?? "—";
        MarqueText.Text       = dto.Marque ?? "—";
        ConsommationText.Text = dto.Consommation.HasValue ? $"{dto.Consommation:F1} L/100 km" : "—";
        KilometrageText.Text  = dto.Kilometrage.HasValue  ? $"{dto.Kilometrage:N0} km" : "—";

        CreatedByText.Text = dto.AjouteParNom ?? "—";
        CreatedAtText.Text = dto.DateCreation?.ToLocalTime().ToString("dd/MM/yyyy HH:mm") ?? "—";
        EditedByText.Text  = dto.ModifieParNom ?? "—";
        EditedAtText.Text  = dto.DateModification?.ToLocalTime().ToString("dd/MM/yyyy HH:mm") ?? "—";
    }

    private void OnCloseClick(object sender, RoutedEventArgs e) => Close();

    private void OnEditClick(object sender, RoutedEventArgs e)
    {
        ShouldEdit = true;
        Close();
    }
}
