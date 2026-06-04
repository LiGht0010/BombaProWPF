using FourniPro.Models;
using System.Windows;
using Wpf.Ui.Controls;

namespace FourniPro.Views.InfrastructurePages.Sections.Citernes;

public partial class DetailCiterneDialog : FluentWindow
{
    public bool ShouldEdit { get; private set; }

    public DetailCiterneDialog(CiterneDto dto)
    {
        InitializeComponent();

        MatriculeHeaderText.Text = dto.MatriculeCiterne ?? "—";
        CapaciteHeaderText.Text  = dto.Capacite.HasValue ? $"{dto.Capacite:F0} L" : string.Empty;

        MatriculeText.Text  = dto.MatriculeCiterne ?? "—";
        CapaciteText.Text   = dto.Capacite.HasValue   ? $"{dto.Capacite:F0} L" : "—";
        PartitionsText.Text = dto.PartitionsNumber.HasValue ? dto.PartitionsNumber.ToString() : "—";

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
