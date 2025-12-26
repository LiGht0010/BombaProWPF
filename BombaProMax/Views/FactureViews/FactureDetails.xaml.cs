using BombaProMax.Models;
using BombaProMax.Services;
using CommunityToolkit.Maui.Views;

namespace BombaProMax.Views.FactureViews;

public partial class FactureDetails : Popup
{
    private readonly FactureDto _facture;
    private readonly List<ElementsFactureDto> _elements;
    private readonly List<BonLivraisonDto> _linkedBLs;
    private readonly FactureService _factureService;

    public FactureDetails(FactureDto facture, List<ElementsFactureDto>? elements = null, List<BonLivraisonDto>? linkedBLs = null)
    {
        InitializeComponent();

        _facture = facture;
        _elements = elements ?? [];
        _linkedBLs = linkedBLs ?? [];
        _factureService = new FactureService();

        // If data not provided, load it
        if (_elements.Count == 0)
        {
            _ = LoadDataAsync();
        }
        else
        {
            PopulateUI();
        }
    }

    private async Task LoadDataAsync()
    {
        try
        {
            // Load elements
            var loadedElements = await _factureService.GetElementsByFactureAsync(_facture.ID);
            _elements.Clear();
            _elements.AddRange(loadedElements);

            // Load linked BLs
            var loadedBLs = await _factureService.GetFullLinkedBLsAsync(_facture.ID);
            _linkedBLs.Clear();
            _linkedBLs.AddRange(loadedBLs);

            PopulateUI();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading facture data: {ex.Message}");
            PopulateUI(); // Populate with empty data
        }
    }

    private void PopulateUI()
    {
        // Header info
        HeaderLabel.Text = "Facture";
        NumeroLabel.Text = _facture.NumeroFacture ?? "N/A";
        DateLabel.Text = _facture.DateFacture?.ToString("dd/MM/yyyy") ?? "-";
        ClientLabel.Text = $"Client: {_facture.ClientNom ?? "Non assigné"}";

        // Statut styling
        var isPaid = IsPaid(_facture.Statut);
        StatutLabel.Text = isPaid ? "? Payée" : "? Non Payée";
        StatutValueLabel.Text = _facture.Statut ?? "Non Payée";
        StatutValueLabel.TextColor = isPaid ? Color.FromArgb("#2E7D32") : Color.FromArgb("#E65100");

        // Client info
        ClientNomLabel.Text = _facture.ClientNom ?? "-";
        ClientNumeroLabel.Text = "-"; // Not in DTO
        ClientContactLabel.Text = "-"; // Not in DTO
        PaiementLabel.Text = _facture.MoyenPaiementNom ?? "-";

        // Financial summary
        NbElementsLabel.Text = _elements.Count.ToString();
        MontantTotalLabel.Text = $"{_facture.MontantTotal ?? 0:N2} MAD";

        // Date Paiement
        if (_facture.DatePaiement.HasValue)
        {
            DatePaiementStack.IsVisible = true;
            DatePaiementLabel.Text = _facture.DatePaiement.Value.ToString("dd/MM/yyyy");
        }

        // Build linked BLs
        BuildLinkedBLs();

        // Build elements rows
        BuildElementsRows();

        // Footer totals
        var totalQte = _elements.Sum(e => e.Quantite ?? 0);
        var totalMontant = _elements.Sum(e => (e.Quantite ?? 0) * (e.PrixUnitaire ?? 0));
        TotalQteFooter.Text = totalQte.ToString();
        TotalMontantFooter.Text = $"{totalMontant:N2} MAD";
    }

    private void BuildLinkedBLs()
    {
        BLsContainer.Children.Clear();

        if (_linkedBLs.Count == 0)
        {
            BLsSection.IsVisible = false;
            return;
        }

        BLsSection.IsVisible = true;

        foreach (var bl in _linkedBLs)
        {
            var row = new Grid
            {
                ColumnDefinitions =
                [
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto }
                ],
                ColumnSpacing = 12,
                Padding = new Thickness(0, 4)
            };

            var badge = new Border
            {
                BackgroundColor = Color.FromArgb("#E3F2FD"),
                Padding = new Thickness(8, 4),
                StrokeThickness = 0
            };
            badge.StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 4 };
            badge.Content = new Label { Text = bl.NumeroBL, FontSize = 11, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#4A8FBF") };

            var dateLabel = new Label { Text = bl.DateBL.ToString("dd/MM/yyyy"), FontSize = 11, TextColor = Color.FromArgb("#666"), VerticalOptions = LayoutOptions.Center };
            var montantLabel = new Label { Text = $"{bl.MontantTotal:N2} MAD", FontSize = 12, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#4A8FBF"), HorizontalOptions = LayoutOptions.End, VerticalOptions = LayoutOptions.Center };

            row.Add(badge, 0, 0);
            row.Add(dateLabel, 1, 0);
            row.Add(montantLabel, 2, 0);

            BLsContainer.Add(row);
        }
    }

    private void BuildElementsRows()
    {
        ElementsContainer.Children.Clear();

        var isAlternate = false;
        foreach (var element in _elements)
        {
            var bgColor = isAlternate ? Color.FromArgb("#FAFAFA") : Colors.White;
            isAlternate = !isAlternate;

            var row = new Grid
            {
                ColumnDefinitions =
                [
                    new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) },
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                    new ColumnDefinition { Width = new GridLength(1.5, GridUnitType.Star) }
                ],
                ColumnSpacing = 10,
                Padding = new Thickness(20, 10),
                BackgroundColor = bgColor
            };

            // Designation
            var displayName = !string.IsNullOrEmpty(element.ProduitNom) ? element.ProduitNom :
                              !string.IsNullOrEmpty(element.ServiceNom) ? element.ServiceNom :
                              "N/A";
            row.Add(new Label { Text = displayName, FontSize = 12, TextColor = Color.FromArgb("#333"), VerticalOptions = LayoutOptions.Center }, 0, 0);

            // Quantite
            row.Add(new Label { Text = (element.Quantite ?? 0).ToString(), FontSize = 12, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#333"), HorizontalTextAlignment = TextAlignment.Center, VerticalOptions = LayoutOptions.Center }, 1, 0);

            // Prix Unitaire
            row.Add(new Label { Text = $"{element.PrixUnitaire ?? 0:N2}", FontSize = 12, TextColor = Color.FromArgb("#666"), HorizontalTextAlignment = TextAlignment.End, VerticalOptions = LayoutOptions.Center }, 2, 0);

            // Montant
            var montant = (element.Quantite ?? 0) * (element.PrixUnitaire ?? 0);
            row.Add(new Label { Text = $"{montant:N2} MAD", FontSize = 12, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#5EAA8D"), HorizontalTextAlignment = TextAlignment.End, VerticalOptions = LayoutOptions.Center }, 3, 0);

            ElementsContainer.Add(row);
        }

        if (_elements.Count == 0)
        {
            ElementsContainer.Add(new Label { Text = "Aucun élément", TextColor = Color.FromArgb("#999"), FontSize = 12, Margin = new Thickness(20, 10) });
        }
    }

    private static bool IsPaid(string? statut)
    {
        if (string.IsNullOrWhiteSpace(statut)) return false;
        var normalized = statut.ToLower().Trim();
        return normalized == "payée" || normalized == "payee";
    }

    private void OnCloseClicked(object? sender, EventArgs e)
    {
        Close();
    }

    private async void OnEnregistrerPdfClicked(object? sender, EventArgs e)
    {
        try
        {
            PdfButton.IsEnabled = false;
            PdfButton.Text = "? Génération...";

            // Build PDF data
            var pdfData = BuildPdfData();

            // Generate PDF
            var pdfService = new PdfGeneratorService();
            var filePath = await pdfService.GenerateFactureReportAsync(pdfData);

            PdfButton.Text = "? Généré!";

            // Show success and offer to open
            var openFile = await Application.Current!.MainPage!.DisplayAlert(
                "PDF Généré",
                $"La facture a été enregistrée:\n{filePath}",
                "Ouvrir le fichier",
                "OK");

            if (openFile)
            {
                await Launcher.Default.OpenAsync(new OpenFileRequest
                {
                    File = new ReadOnlyFile(filePath)
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error generating PDF: {ex.Message}");
            await Application.Current!.MainPage!.DisplayAlert(
                "Erreur",
                $"Impossible de générer le PDF:\n{ex.Message}",
                "OK");
        }
        finally
        {
            PdfButton.IsEnabled = true;
            PdfButton.Text = "?? Enregistrer PDF";
        }
    }

    private FacturePdfData BuildPdfData()
    {
        var pdfData = new FacturePdfData
        {
            FactureID = _facture.ID,
            NumeroFacture = _facture.NumeroFacture ?? "",
            DateFacture = _facture.DateFacture ?? DateOnly.FromDateTime(DateTime.Now),
            ClientNom = _facture.ClientNom,
            Statut = _facture.Statut,
            DatePaiement = _facture.DatePaiement,
            MoyenPaiementNom = _facture.MoyenPaiementNom,
            MontantTotal = _facture.MontantTotal ?? 0
        };

        // Convert elements
        pdfData.Elements = _elements.Select(e => new FactureElementPdfData
        {
            Description = "",
            ProduitNom = e.ProduitNom,
            ServiceNom = e.ServiceNom,
            Quantite = e.Quantite ?? 0,
            PrixUnitaire = e.PrixUnitaire ?? 0
        }).ToList();

        // Convert linked BLs
        pdfData.BonsLivraisonLies = _linkedBLs.Select(bl => new FactureBLLinkPdfData
        {
            NumeroBL = bl.NumeroBL,
            DateBL = bl.DateBL,
            MontantBL = bl.MontantTotal
        }).ToList();

        return pdfData;
    }
}