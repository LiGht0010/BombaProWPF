using BombaProMax.Models;
using BombaProMax.Services;
using CommunityToolkit.Maui.Views;

namespace BombaProMax.Views.PeriodeViews;

public partial class PeriodeViewPopup : Popup
{
    private readonly PeriodeAnalyticsModel _analytics;
    private readonly int _periodeId;
    private readonly PeriodeService _periodeService;
    private readonly CreditTransactionService _creditTransactionService;
    private readonly PeriodeDto _periode;
    private readonly List<PeriodeDetailsDto> _details;
    private List<CreditTransactionDto> _creditTransactions = [];

    public PeriodeViewPopup(PeriodeDto periode, List<PeriodeDetailsDto> details)
    {
        InitializeComponent();
        
        _periodeId = periode.PeriodeID;
        _periode = periode;
        _details = details;
        _periodeService = new PeriodeService();
        _creditTransactionService = new CreditTransactionService();
        _analytics = BuildAnalytics(periode, details);
        
        PopulateUI();
        
        // Load marge and credit data asynchronously
        _ = LoadAdditionalDataAsync();
    }

    private async Task LoadAdditionalDataAsync()
    {
        await Task.WhenAll(
            LoadMargeDataAsync(),
            LoadCreditTransactionsAsync()
        );
    }

    private async Task LoadCreditTransactionsAsync()
    {
        try
        {
            _creditTransactions = await _creditTransactionService.GetByPeriodeIdAsync(_periodeId);
            
            MainThread.BeginInvokeOnMainThread(() =>
            {
                BuildCreditTransactionsSection();
                UpdateFinancialMetrics();
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading credit transactions: {ex.Message}");
        }
    }

    private async Task LoadMargeDataAsync()
    {
        try
        {
            var margeData = await _periodeService.GetPeriodeMargeAnalysisAsync(_periodeId);
            
            if (margeData != null && margeData.Consommations.Count > 0)
            {
                _analytics.ConsommationsStock = margeData.Consommations
                    .Select(c => new StockConsumptionModel
                    {
                        StockLotID = c.StockLotID,
                        ProduitNom = c.ProduitNom,
                        ReservoirNumero = c.ReservoirNumero,
                        PrixAchat = c.PrixAchat,
                        PrixVente = c.PrixVente,
                        QuantiteConsommee = c.QuantiteConsommee
                    })
                    .ToList();
                
                _analytics.TotalCoutAchat = margeData.TotalCoutAchat;
                
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    BuildMargeSection();
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading marge data: {ex.Message}");
        }
    }

    private void BuildCreditTransactionsSection()
    {
        CreditTransactionsContainer.Children.Clear();
        
        var totalCredite = _creditTransactions.Sum(ct => ct.MontantTotal);
        CreditCountBadge.Text = $"({_creditTransactions.Count})";
        CreditTotalLabel.Text = $"{totalCredite:N2} MAD";
        TotalCrediteLabel.Text = $"{totalCredite:N2} MAD";

        if (_creditTransactions.Count == 0)
        {
            var emptyLabel = new Label
            {
                Text = "Aucun crédit carburant lié à cette période",
                FontSize = 12,
                TextColor = Color.FromArgb("#999"),
                HorizontalTextAlignment = TextAlignment.Center,
                Padding = new Thickness(15, 20)
            };
            CreditTransactionsContainer.Add(emptyLabel);
            return;
        }

        var isAlternate = false;
        foreach (var ct in _creditTransactions)
        {
            var bgColor = isAlternate ? Color.FromArgb("#FAFAFA") : Colors.White;
            isAlternate = !isAlternate;

            var row = new Grid
            {
                ColumnDefinitions = [
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Auto }
                ],
                ColumnSpacing = 10,
                Padding = new Thickness(15, 8),
                BackgroundColor = bgColor
            };

            var nameStack = new VerticalStackLayout { Spacing = 2 };
            nameStack.Add(new Label
            {
                Text = ct.ClientNom ?? "N/A",
                FontSize = 12,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#333"),
                LineBreakMode = LineBreakMode.TailTruncation
            });
            nameStack.Add(new Label
            {
                Text = $"{ct.ProduitNom} • {ct.DateCredit:dd/MM HH:mm}",
                FontSize = 10,
                TextColor = Color.FromArgb("#999"),
                LineBreakMode = LineBreakMode.TailTruncation
            });

            var qtyLabel = new Label
            {
                Text = $"{ct.Quantite}L",
                FontSize = 11,
                TextColor = Color.FromArgb("#666"),
                VerticalOptions = LayoutOptions.Center
            };

            var amountLabel = new Label
            {
                Text = $"{ct.MontantTotal:N2}",
                FontSize = 12,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#E65100"),
                VerticalOptions = LayoutOptions.Center
            };

            row.Add(nameStack, 0, 0);
            row.Add(qtyLabel, 1, 0);
            row.Add(amountLabel, 2, 0);

            CreditTransactionsContainer.Add(row);
        }
    }

    private void UpdateFinancialMetrics()
    {
        var totalCredite = _creditTransactions.Sum(ct => ct.MontantTotal);
        var recette = _analytics.Recette;
        var tpe = _analytics.TPE;
        var especes = _analytics.Especes;
        
        // Espèces Attendues = Recette - TPE - Crédité
        var especesAttendues = recette - tpe - totalCredite;
        EspecesAttenduesLabel.Text = $"{especesAttendues:N2} MAD";
        
        // Manque = Espèces Attendues - Espèces Déclarées
        var manque = especesAttendues - especes;
        
        // Update ecart display with new formula
        if (Math.Abs(manque) < 0.01m)
        {
            EcartLabel.Text = "0.00 MAD";
            EcartLabel.TextColor = Color.FromArgb("#2E7D32");
            EcartPercentLabel.Text = "(équilibré)";
        }
        else if (manque > 0)
        {
            EcartLabel.Text = $"{manque:N2} MAD";
            EcartLabel.TextColor = Color.FromArgb("#C62828");
            var percent = recette > 0 ? Math.Round((manque / recette) * 100, 1) : 0;
            EcartPercentLabel.Text = $"({percent}%)";
        }
        else
        {
            EcartLabel.Text = $"{manque:N2} MAD";
            EcartLabel.TextColor = Color.FromArgb("#1976D2");
            var percent = recette > 0 ? Math.Round((Math.Abs(manque) / recette) * 100, 1) : 0;
            EcartPercentLabel.Text = $"(excédent {percent}%)";
        }
    }

    private static PeriodeAnalyticsModel BuildAnalytics(PeriodeDto periode, List<PeriodeDetailsDto> details)
    {
        var analytics = new PeriodeAnalyticsModel
        {
            PeriodeID = periode.PeriodeID,
            DateDebut = periode.DateDebut,
            DateFin = periode.DateFin,
            EmployeNom = periode.EmployeNom,
            TPE = periode.TPE,
            Especes = periode.Especes,
            Recette = details.Sum(d => d.PrixTotal),
            TotalQuantite = details.Sum(d => d.QuantiteVendue),
            TotalEcartCompteurs = details.Sum(d => d.DifferenceQuantite)
        };

        analytics.TotauxParProduit = details
            .Where(d => d.ProduitID.HasValue)
            .GroupBy(d => new { d.ProduitID, d.ProduitNom })
            .Select(g => new ProductTotalModel
            {
                ProduitID = g.Key.ProduitID,
                ProduitNom = g.Key.ProduitNom ?? "N/A",
                Quantite = g.Sum(d => d.QuantiteVendue),
                Montant = g.Sum(d => d.PrixTotal),
                NombrePompes = g.Count()
            })
            .OrderByDescending(p => p.Montant)
            .ToList();

        analytics.TotauxParReservoir = details
            .Where(d => d.ReservoirID.HasValue)
            .GroupBy(d => new { d.ReservoirID, d.ReservoirNumero, d.ProduitNom })
            .Select(g => new ReservoirTotalModel
            {
                ReservoirID = g.Key.ReservoirID,
                ReservoirNumero = g.Key.ReservoirNumero ?? "N/A",
                ProduitNom = g.Key.ProduitNom,
                QuantiteConsommee = g.Sum(d => d.QuantiteVendue),
                Montant = g.Sum(d => d.PrixTotal),
                NombrePompes = g.Count()
            })
            .OrderBy(r => r.ReservoirNumero)
            .ToList();

        analytics.DetailsParPompe = details
            .Select(d => new PompeTotalModel
            {
                PompeID = d.PompeID,
                PompeNumero = d.PompeNumero ?? "N/A",
                ReservoirNumero = d.ReservoirNumero,
                ProduitNom = d.ProduitNom,
                CompteurElecDebut = d.CompteurElectroniqueDebut,
                CompteurElecFin = d.CompteurElectroniqueFinal,
                CompteurMecaDebut = d.CompteurMecaniqueDebut,
                CompteurMecaFin = d.CompteurMecaniqueFinal,
                QuantiteVendue = d.QuantiteVendue,
                PrixUnitaire = d.PrixCarburant,
                Montant = d.PrixTotal
            })
            .OrderBy(p => p.PompeNumero)
            .ToList();

        analytics.ConsommationsStock = [];

        return analytics;
    }

    private void PopulateUI()
    {
        DateRangeLabel.Text = $"{_analytics.DateDebut:dd/MM/yyyy HH:mm} - {_analytics.DateFin:HH:mm}";
        EmployeLabel.Text = $"Employe: {_analytics.EmployeNom ?? "Non assigne"}";
        
        var duree = _analytics.DateFin - _analytics.DateDebut;
        DureeLabel.Text = $"Duree: {(int)duree.TotalHours}h {duree.Minutes:D2}min";

        TotalQuantiteLabel.Text = $"{_analytics.TotalQuantite:N2} L";

        TPELabel.Text = $"{_analytics.TPE:N2} MAD";
        EspecesLabel.Text = $"{_analytics.Especes:N2} MAD";
        RecetteLabel.Text = $"{_analytics.Recette:N2} MAD";
        
        // Initial ecart (will be updated when credits load)
        TotalCrediteLabel.Text = "0.00 MAD";
        EspecesAttenduesLabel.Text = $"{_analytics.Recette - _analytics.TPE:N2} MAD";
        
        var initialManque = (_analytics.Recette - _analytics.TPE) - _analytics.Especes;
        if (Math.Abs(initialManque) < 0.01m)
        {
            EcartLabel.Text = "0.00 MAD";
            EcartLabel.TextColor = Color.FromArgb("#2E7D32");
            EcartPercentLabel.Text = "(équilibré)";
        }
        else if (initialManque > 0)
        {
            EcartLabel.Text = $"{initialManque:N2} MAD";
            EcartLabel.TextColor = Color.FromArgb("#C62828");
            EcartPercentLabel.Text = "";
        }
        else
        {
            EcartLabel.Text = $"{initialManque:N2} MAD";
            EcartLabel.TextColor = Color.FromArgb("#1976D2");
            EcartPercentLabel.Text = "(excédent)";
        }

        BuildReservoirRows();
        BuildProductRows();
        BuildMargeSection();
        BuildPumpTableRows();

        TotalQteFooter.Text = $"{_analytics.TotalQuantite:N2} L";
        TotalMontantFooter.Text = $"{_analytics.Recette:N2} MAD";
    }

    private void BuildReservoirRows()
    {
        ReservoirsContainer.Children.Clear();

        foreach (var reservoir in _analytics.TotauxParReservoir)
        {
            var row = new Grid
            {
                ColumnDefinitions = [
                    new ColumnDefinition { Width = 60 },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto }
                ],
                ColumnSpacing = 10
            };

            var nameStack = new VerticalStackLayout { Spacing = 2 };
            nameStack.Add(new Label { Text = reservoir.ReservoirNumero, FontSize = 14, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#333") });
            nameStack.Add(new Label { Text = reservoir.ProduitNom ?? "", FontSize = 11, TextColor = Color.FromArgb("#999") });
            
            var qtyLabel = new Label 
            { 
                Text = $"-{reservoir.QuantiteConsommee:N2} L", 
                FontSize = 13, 
                TextColor = Color.FromArgb("#D32F2F"),
                VerticalOptions = LayoutOptions.Center 
            };
            
            var amountLabel = new Label 
            { 
                Text = $"{reservoir.Montant:N2} MAD", 
                FontSize = 14, 
                FontAttributes = FontAttributes.Bold, 
                TextColor = Color.FromArgb("#333"),
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.Center 
            };

            row.Add(nameStack, 0, 0);
            row.Add(qtyLabel, 1, 0);
            row.Add(amountLabel, 2, 0);

            ReservoirsContainer.Add(row);
        }

        if (_analytics.TotauxParReservoir.Count == 0)
        {
            ReservoirsContainer.Add(new Label { Text = "Aucune donnee", TextColor = Color.FromArgb("#999"), FontSize = 12 });
        }
    }

    private void BuildProductRows()
    {
        ProduitsContainer.Children.Clear();

        var colors = new[] { "#4CAF50", "#2196F3", "#FF9800", "#9C27B0", "#795548" };
        var colorIndex = 0;

        foreach (var produit in _analytics.TotauxParProduit)
        {
            var color = Color.FromArgb(colors[colorIndex % colors.Length]);
            colorIndex++;

            var row = new Grid
            {
                ColumnDefinitions = [
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto }
                ],
                ColumnSpacing = 10
            };

            var nameStack = new HorizontalStackLayout { Spacing = 8, VerticalOptions = LayoutOptions.Center };
            nameStack.Add(new BoxView { Color = color, WidthRequest = 4, HeightRequest = 20, CornerRadius = 2 });
            nameStack.Add(new Label { Text = produit.ProduitNom, FontSize = 14, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#333"), VerticalOptions = LayoutOptions.Center });
            
            var qtyLabel = new Label 
            { 
                Text = $"{produit.Quantite:N2} L", 
                FontSize = 13, 
                TextColor = Color.FromArgb("#666"),
                VerticalOptions = LayoutOptions.Center 
            };
            
            var amountLabel = new Label 
            { 
                Text = $"{produit.Montant:N2} MAD", 
                FontSize = 14, 
                FontAttributes = FontAttributes.Bold, 
                TextColor = color,
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.Center 
            };

            row.Add(nameStack, 0, 0);
            row.Add(qtyLabel, 1, 0);
            row.Add(amountLabel, 2, 0);

            ProduitsContainer.Add(row);
        }

        if (_analytics.TotauxParProduit.Count == 0)
        {
            ProduitsContainer.Add(new Label { Text = "Aucune donnee", TextColor = Color.FromArgb("#999"), FontSize = 12 });
        }
    }

    private void BuildMargeSection()
    {
        MargeContainer.Children.Clear();

        if (_analytics.ConsommationsStock.Count > 0)
        {
            var groupedByProduit = _analytics.ConsommationsStock
                .GroupBy(c => c.ProduitNom ?? "N/A")
                .ToList();

            foreach (var group in groupedByProduit)
            {
                var totalQte = group.Sum(c => c.QuantiteConsommee);
                var totalCout = group.Sum(c => c.CoutAchat);
                var totalVente = group.Sum(c => c.Vente);
                var totalMarge = totalVente - totalCout;
                var prixAchatMoyen = totalQte > 0 ? totalCout / totalQte : 0;

                var row = new Grid
                {
                    ColumnDefinitions = [
                        new ColumnDefinition { Width = new GridLength(1.5, GridUnitType.Star) },
                        new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                        new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                        new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                        new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                        new ColumnDefinition { Width = new GridLength(1.2, GridUnitType.Star) }
                    ],
                    ColumnSpacing = 15,
                    Padding = new Thickness(0, 6)
                };

                // Product name
                row.Add(new Label { Text = group.Key, FontSize = 12, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#333"), VerticalOptions = LayoutOptions.Center }, 0, 0);
                
                // Quantity
                row.Add(new Label { Text = $"{totalQte:N2} L", FontSize = 12, TextColor = Color.FromArgb("#666"), VerticalOptions = LayoutOptions.Center }, 1, 0);
                
                // Prix Achat Moyen
                row.Add(new Label { Text = $"@ {prixAchatMoyen:N2}", FontSize = 11, TextColor = Color.FromArgb("#999"), VerticalOptions = LayoutOptions.Center }, 2, 0);
                
                // Cout Achat
                row.Add(new Label { Text = $"{totalCout:N2}", FontSize = 12, TextColor = Color.FromArgb("#666"), HorizontalTextAlignment = TextAlignment.End, VerticalOptions = LayoutOptions.Center }, 3, 0);
                
                // Vente
                row.Add(new Label { Text = $"{totalVente:N2}", FontSize = 12, TextColor = Color.FromArgb("#666"), HorizontalTextAlignment = TextAlignment.End, VerticalOptions = LayoutOptions.Center }, 4, 0);
                
                // Marge
                var margeColor = totalMarge >= 0 ? "#2E7D32" : "#C62828";
                var margePercent = totalVente > 0 ? Math.Round((totalMarge / totalVente) * 100, 1) : 0;
                row.Add(new Label { Text = $"{totalMarge:N2} ({margePercent}%)", FontSize = 12, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb(margeColor), HorizontalTextAlignment = TextAlignment.End, VerticalOptions = LayoutOptions.Center }, 5, 0);

                MargeContainer.Add(row);
            }

            // Update totals
            TotalCoutAchatLabel.Text = $"Cout: {_analytics.TotalCoutAchat:N2} MAD";
            TotalVenteLabel.Text = $"Vente: {_analytics.TotalVente:N2} MAD";
            TotalMargeLabel.Text = $"{_analytics.TotalMarge:N2} MAD ({_analytics.MargePercent}%)";
            TotalMargeLabel.TextColor = _analytics.TotalMarge >= 0 ? Color.FromArgb("#2E7D32") : Color.FromArgb("#C62828");
        }
        else
        {
            // Show no data message - marge data may not be available if no stock consumption exists
            var infoLabel = new Label
            {
                Text = "Aucune donnée de marge disponible (pas de consommation stock enregistrée)",
                FontSize = 11,
                TextColor = Color.FromArgb("#999"),
                HorizontalTextAlignment = TextAlignment.Center
            };
            MargeContainer.Add(infoLabel);

            // Still show totals based on Recette
            TotalCoutAchatLabel.Text = "Cout: N/A";
            TotalVenteLabel.Text = $"Vente: {_analytics.Recette:N2} MAD";
            TotalMargeLabel.Text = "N/A";
            TotalMargeLabel.TextColor = Color.FromArgb("#999");
        }
    }

    private void BuildPumpTableRows()
    {
        PompesContainer.Children.Clear();

        var isAlternate = false;
        foreach (var pompe in _analytics.DetailsParPompe)
        {
            var bgColor = isAlternate ? Color.FromArgb("#FAFAFA") : Colors.White;
            isAlternate = !isAlternate;

            // 9 columns with proportional widths to match header: 1.2*,1.2*,1.2*,*,*,*,*,1.2*,1.5*
            var row = new Grid
            {
                ColumnDefinitions = [
                    new ColumnDefinition { Width = new GridLength(1.2, GridUnitType.Star) },  // POMPE
                    new ColumnDefinition { Width = new GridLength(1.2, GridUnitType.Star) },  // PRODUIT
                    new ColumnDefinition { Width = new GridLength(1.2, GridUnitType.Star) },  // RESERVOIR
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },    // ELEC D
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },    // ELEC F
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },    // MECA D
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },    // MECA F
                    new ColumnDefinition { Width = new GridLength(1.2, GridUnitType.Star) },  // QUANTITE
                    new ColumnDefinition { Width = new GridLength(1.5, GridUnitType.Star) }   // MONTANT
                ],
                ColumnSpacing = 10,
                Padding = new Thickness(20, 10),
                BackgroundColor = bgColor
            };

            // POMPE
            row.Add(new Label { Text = pompe.PompeNumero, FontSize = 12, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#333"), VerticalOptions = LayoutOptions.Center }, 0, 0);
            
            // PRODUIT
            row.Add(new Label { Text = pompe.ProduitNom ?? "N/A", FontSize = 11, TextColor = Color.FromArgb("#666"), VerticalOptions = LayoutOptions.Center }, 1, 0);
            
            // RESERVOIR
            row.Add(new Label { Text = pompe.ReservoirNumero ?? "N/A", FontSize = 11, TextColor = Color.FromArgb("#666"), VerticalOptions = LayoutOptions.Center }, 2, 0);
            
            // ELEC D - Blue Bold
            row.Add(new Label { Text = $"{pompe.CompteurElecDebut:N0}", FontSize = 11, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#1976D2"), HorizontalTextAlignment = TextAlignment.Center, VerticalOptions = LayoutOptions.Center }, 3, 0);
            
            // ELEC F - Blue Bold
            row.Add(new Label { Text = $"{pompe.CompteurElecFin:N0}", FontSize = 11, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#1976D2"), HorizontalTextAlignment = TextAlignment.Center, VerticalOptions = LayoutOptions.Center }, 4, 0);
            
            // MECA D - Brown Bold
            row.Add(new Label { Text = $"{pompe.CompteurMecaDebut:N0}", FontSize = 11, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#795548"), HorizontalTextAlignment = TextAlignment.Center, VerticalOptions = LayoutOptions.Center }, 5, 0);
            
            // MECA F - Brown Bold (red if ecart significatif)
            var mecaFColor = pompe.HasEcartSignificatif ? "#C62828" : "#795548";
            row.Add(new Label { Text = $"{pompe.CompteurMecaFin:N0}", FontSize = 11, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb(mecaFColor), HorizontalTextAlignment = TextAlignment.Center, VerticalOptions = LayoutOptions.Center }, 6, 0);
            
            // QUANTITE
            var qteStack = new VerticalStackLayout { HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
            qteStack.Add(new Label { Text = $"{pompe.QuantiteVendue:N2} L", FontSize = 12, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#333"), HorizontalTextAlignment = TextAlignment.Center });
            if (pompe.HasEcartSignificatif)
            {
                qteStack.Add(new Label { Text = $"ecart: {pompe.EcartCompteurs:N1}L", FontSize = 9, TextColor = Color.FromArgb("#C62828"), HorizontalTextAlignment = TextAlignment.Center });
            }
            row.Add(qteStack, 7, 0);
            
            // MONTANT
            var montantStack = new VerticalStackLayout { HorizontalOptions = LayoutOptions.End, VerticalOptions = LayoutOptions.Center };
            montantStack.Add(new Label { Text = $"{pompe.Montant:N2} MAD", FontSize = 12, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#1976D2"), HorizontalTextAlignment = TextAlignment.End });
            montantStack.Add(new Label { Text = $"@ {pompe.PrixUnitaire:N2}", FontSize = 9, TextColor = Color.FromArgb("#999"), HorizontalTextAlignment = TextAlignment.End });
            row.Add(montantStack, 8, 0);

            PompesContainer.Add(row);
        }

        if (_analytics.DetailsParPompe.Count == 0)
        {
            PompesContainer.Add(new Label { Text = "Aucun releve de pompe", TextColor = Color.FromArgb("#999"), FontSize = 12, Margin = new Thickness(20, 10) });
        }
    }

    private void OnCloseClicked(object? sender, EventArgs e)
    {
        Close();
    }

    private async void OnModifierClicked(object? sender, EventArgs e)
    {
        await CloseAsync(new PeriodeViewResult 
        { 
            Action = "Edit", 
            Periode = _periode, 
            Details = _details 
        });
    }

    private async void OnEnregistrerPdfClicked(object? sender, EventArgs e)
    {
        try
        {
            PdfButton.IsEnabled = false;
            PdfButton.Text = "? Génération...";

            var pdfData = BuildPdfData();
            var pdfService = new PdfGeneratorService();
            var filePath = await pdfService.GeneratePeriodeReportAsync(pdfData);

            PdfButton.Text = "? Généré!";

            var openFile = await Application.Current!.MainPage!.DisplayAlert(
                "PDF Généré",
                $"Le rapport a été enregistré:\n{filePath}",
                "Ouvrir le dossier",
                "OK");

            if (openFile)
            {
                var folderPath = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(folderPath))
                {
                    await Launcher.Default.OpenAsync(new OpenFileRequest
                    {
                        File = new ReadOnlyFile(filePath)
                    });
                }
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

    private PeriodePdfData BuildPdfData()
    {
        var pdfData = new PeriodePdfData
        {
            PeriodeID = _analytics.PeriodeID,
            DateDebut = _analytics.DateDebut,
            DateFin = _analytics.DateFin,
            EmployeNom = _analytics.EmployeNom,
            TPE = _analytics.TPE,
            Especes = _analytics.Especes,
            Recette = _analytics.Recette,
            TotalQuantite = _analytics.TotalQuantite,
            TotalCoutAchat = _analytics.TotalCoutAchat
        };

        pdfData.TotauxParReservoir = _analytics.TotauxParReservoir
            .Select(r => new ReservoirPdfData
            {
                ReservoirNumero = r.ReservoirNumero,
                ProduitNom = r.ProduitNom,
                QuantiteConsommee = r.QuantiteConsommee,
                Montant = r.Montant
            }).ToList();

        pdfData.TotauxParProduit = _analytics.TotauxParProduit
            .Select(p => new ProduitPdfData
            {
                ProduitNom = p.ProduitNom,
                Quantite = p.Quantite,
                Montant = p.Montant
            }).ToList();

        pdfData.MargesParProduit = _analytics.ConsommationsStock
            .GroupBy(c => c.ProduitNom ?? "N/A")
            .Select(g => new MargePdfData
            {
                ProduitNom = g.Key,
                Quantite = g.Sum(c => c.QuantiteConsommee),
                CoutAchat = g.Sum(c => c.CoutAchat),
                Vente = g.Sum(c => c.Vente)
            }).ToList();

        pdfData.DetailsParPompe = _analytics.DetailsParPompe
            .Select(p => new PompePdfData
            {
                PompeNumero = p.PompeNumero,
                ProduitNom = p.ProduitNom,
                ReservoirNumero = p.ReservoirNumero,
                CompteurElecDebut = p.CompteurElecDebut,
                CompteurElecFin = p.CompteurElecFin,
                CompteurMecaDebut = p.CompteurMecaDebut,
                CompteurMecaFin = p.CompteurMecaFin,
                QuantiteVendue = p.QuantiteVendue,
                Montant = p.Montant
            }).ToList();

        return pdfData;
    }
}

public class PeriodeViewResult
{
    public string Action { get; set; } = "";
    public PeriodeDto? Periode { get; set; }
    public List<PeriodeDetailsDto>? Details { get; set; }
}
