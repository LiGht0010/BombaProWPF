using FourniPro.Models;
using System.Windows;
using Wpf.Ui.Controls;

namespace FourniPro.Views.InfrastructurePages.Sections.Chauffeurs;

public partial class DetailChauffeurDialog : FluentWindow
{
    public bool ShouldEdit { get; private set; }

    public DetailChauffeurDialog(ChauffeurDto dto)
    {
        InitializeComponent();

        NomCompletText.Text = $"{dto.Prenom} {dto.Nom}".Trim();
        CINHeaderText.Text  = dto.CIN ?? string.Empty;

        NomText.Text       = dto.Nom;
        PrenomText.Text    = dto.Prenom ?? "—";
        CINText.Text       = dto.CIN ?? "—";
        TelephoneText.Text = dto.Telephone ?? "—";
        PermisText.Text    = dto.NumeroPermis ?? "—";

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
