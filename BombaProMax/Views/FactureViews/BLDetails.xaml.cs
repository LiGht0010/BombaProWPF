using BombaProMax.Models;
using BombaProMax.Services;
using CommunityToolkit.Maui.Views;

namespace BombaProMax.Views.FactureViews;

public partial class BLDetails : Popup
{
    private readonly BonLivraisonDto _bl;
    private readonly List<BonLivraisonDetailsDto> _details;
    private readonly BonLivraisonService _blService;

    public BLDetails(BonLivraisonDto bl, List<BonLivraisonDetailsDto>? details = null)
    {
        InitializeComponent();

        _bl = bl;
        _details = details ?? [];
        _blService = new BonLivraisonService();

        // If details not provided, load them
        if (_details.Count == 0)
        {
            _ = LoadDetailsAsync();
        }
        else
        {
            PopulateUI();
        }
    }

    private async Task LoadDetailsAsync()
    {
        try
        {
            var loadedDetails = await _blService.GetDetailsByBLAsync(_bl.ID);
            _details.Clear();
            _details.AddRange(loadedDetails);
            PopulateUI();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading BL details: {ex.Message}");
            PopulateUI(); // Populate with empty details
        }
    }

    private void PopulateUI()
    {
        // Header info
        HeaderLabel.Text = $"Bon de Livraison";
        NumeroLabel.Text = _bl.NumeroBL;
        DateLabel.Text = _bl.DateBL.ToString("dd/MM/yyyy");
        ClientLabel.Text = $"Client: {_bl.ClientNom ?? "Non assigné"}";
        StatutLabel.Text = _bl.EstFacture ? "? Facturé" : "? Non Facturé";

        // Client info
        ClientNomLabel.Text = _bl.ClientNom ?? "-";
        ClientNumeroLabel.Text = _bl.ClientNumero ?? "-";
        ClientContactLabel.Text = "-"; // Not available in BL DTO

        // Financial summary
        var totalQte = _details.Sum(d => d.Quantite);
        NbLignesLabel.Text = _details.Count.ToString();
        QteToTaleLabel.Text = totalQte.ToString();
        MontantTotalLabel.Text = $"{_bl.MontantTotal:N2} MAD";

        // Build product totals
        BuildProductTotals();

        // Build details rows
        BuildDetailsRows();

        // Footer totals (only quantity, no montant in simplified view)
        TotalQteFooter.Text = totalQte.ToString();

        // Notes
        if (!string.IsNullOrWhiteSpace(_bl.Notes))
        {
            NotesSection.IsVisible = true;
            NotesLabel.Text = _bl.Notes;
        }
    }

    private void BuildProductTotals()
    {
        ProduitsContainer.Children.Clear();

        var grouped = _details
            .GroupBy(d => d.DisplayName)
            .Select(g => new
            {
                Nom = g.Key,
                Qte = g.Sum(x => x.Quantite),
                Montant = g.Sum(x => x.MontantLigne)
            })
            .OrderByDescending(x => x.Montant)
            .ToList();

        var colors = new[] { "#4CAF50", "#2196F3", "#FF9800", "#9C27B0", "#795548" };
        var colorIndex = 0;

        foreach (var produit in grouped)
        {
            var color = Color.FromArgb(colors[colorIndex % colors.Length]);
            colorIndex++;

            var row = new Grid
            {
                ColumnDefinitions =
                [
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Auto }
                ],
                ColumnSpacing = 10
            };

            var colorBar = new BoxView { Color = color, WidthRequest = 4, HeightRequest = 20, CornerRadius = 2 };
            var nameLabel = new Label { Text = produit.Nom, FontSize = 13, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#333"), VerticalOptions = LayoutOptions.Center };
            var qteLabel = new Label { Text = $"x{produit.Qte}", FontSize = 12, TextColor = Color.FromArgb("#666"), VerticalOptions = LayoutOptions.Center };
            var montantLabel = new Label { Text = $"{produit.Montant:N2} MAD", FontSize = 13, FontAttributes = FontAttributes.Bold, TextColor = color, HorizontalOptions = LayoutOptions.End, VerticalOptions = LayoutOptions.Center };

            row.Add(colorBar, 0, 0);
            row.Add(nameLabel, 1, 0);
            row.Add(qteLabel, 2, 0);
            row.Add(montantLabel, 3, 0);

            ProduitsContainer.Add(row);
        }

        if (grouped.Count == 0)
        {
            ProduitsContainer.Add(new Label { Text = "Aucune donnée", TextColor = Color.FromArgb("#999"), FontSize = 12 });
        }
    }

    private void BuildDetailsRows()
    {
        DetailsContainer.Children.Clear();

        var isAlternate = false;
        foreach (var detail in _details)
        {
            var bgColor = isAlternate ? Color.FromArgb("#FAFAFA") : Colors.White;
            isAlternate = !isAlternate;

            var row = new Grid
            {
                ColumnDefinitions =
                [
                    new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) },
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
                ],
                ColumnSpacing = 10,
                Padding = new Thickness(20, 10),
                BackgroundColor = bgColor
            };

            // Produit/Designation
            row.Add(new Label { Text = detail.DisplayName, FontSize = 12, TextColor = Color.FromArgb("#333"), VerticalOptions = LayoutOptions.Center }, 0, 0);

            // Quantite
            row.Add(new Label { Text = detail.Quantite.ToString(), FontSize = 12, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#333"), HorizontalTextAlignment = TextAlignment.Center, VerticalOptions = LayoutOptions.Center }, 1, 0);

            DetailsContainer.Add(row);
        }

        if (_details.Count == 0)
        {
            DetailsContainer.Add(new Label { Text = "Aucun détail", TextColor = Color.FromArgb("#999"), FontSize = 12, Margin = new Thickness(20, 10) });
        }
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

            // Build printing data
            var printingData = await BuildPrintingDataAsync();

            // Generate PDF
            var pdfService = new PdfGeneratorService();
            var filePath = await pdfService.GenerateBLPrintAsync(printingData);

            PdfButton.Text = "? Généré!";

            // Show success and offer to open
            var openFile = await Application.Current!.MainPage!.DisplayAlert(
                "PDF Généré",
                $"Le bon de livraison a été enregistré:\n{filePath}",
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

    private async Task<BlPrintingData> BuildPrintingDataAsync()
    {
        // Get station info
        var stationService = new StationInfoService();
        var stationInfo = await stationService.GetStationInfoAsync();

        var printingData = new BlPrintingData
        {
            BonLivraisonID = _bl.ID,
            NumeroBL = _bl.NumeroBL,
            DateBL = _bl.DateBL,
            ClientNom = _bl.ClientNom,
            ClientNumero = _bl.ClientNumero,
            EstFacture = _bl.EstFacture,
            Notes = _bl.Notes,
            TotalQuantite = _details.Sum(d => d.Quantite),
            NombreElements = _details.Count,
            StationInfo = stationInfo
        };

        // Convert details (simplified - only Produit and Quantite)
        printingData.Elements = _details.Select(d => new BlPrintingElement
        {
            Description = d.Description,
            ProduitNom = d.ProduitNom,
            ServiceNom = d.ServiceNom,
            Quantite = d.Quantite
        }).ToList();

        // Build product totals (simplified - only Produit and Quantite)
        printingData.TotauxParProduit = _details
            .GroupBy(d => d.DisplayName)
            .Select(g => new BlPrintingProduitTotal
            {
                ProduitNom = g.Key,
                QuantiteTotale = g.Sum(x => x.Quantite)
            })
            .OrderByDescending(x => x.QuantiteTotale)
            .ToList();

        return printingData;
    }
}