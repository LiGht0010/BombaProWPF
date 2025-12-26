using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using BombaProMax.Models;

// Alias to avoid conflicts with MAUI types
using QColors = QuestPDF.Helpers.Colors;
using QContainer = QuestPDF.Infrastructure.IContainer;

namespace BombaProMax.Services;

/// <summary>
/// Service for generating PDF reports for Periode, Facture, BonLivraison, Achat, and Rapport data.
/// Uses QuestPDF for clean, code-based PDF generation.
/// </summary>
public class PdfGeneratorService
{
    // Brand colors
    private static readonly string PrimaryBlue = "#1976D2";
    private static readonly string DarkBlue = "#1565C0";
    private static readonly string Green = "#2E7D32";
    private static readonly string FactureGreen = "#5EAA8D";
    private static readonly string BLBlue = "#4A8FBF";
    private static readonly string Orange = "#E65100";
    private static readonly string Red = "#C62828";
    private static readonly string Gray = "#666666";
    private static readonly string LightGray = "#F5F5F5";
    private static readonly string White = "#FFFFFF";

    static PdfGeneratorService()
    {
        // Configure QuestPDF license (Community license for open source)
        QuestPDF.Settings.License = LicenseType.Community;
    }

    #region Periode PDF Generation

    /// <summary>
    /// Generates a PDF report for a Periode and saves it to the specified path.
    /// </summary>
    public async Task<string> GeneratePeriodeReportAsync(PeriodePdfData data)
    {
        // Create output directory
        var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var outputDir = Path.Combine(documentsPath, "BombaProMax", "Periodes");
        Directory.CreateDirectory(outputDir);

        // Generate filename
        var fileName = $"Periode_{data.DateDebut:yyyy-MM-dd}_{data.PeriodeID}.pdf";
        var filePath = Path.Combine(outputDir, fileName);

        // Generate PDF
        await Task.Run(() =>
        {
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Element(c => ComposeHeader(c, data));
                    page.Content().Element(c => ComposeContent(c, data));
                    page.Footer().Element(c => ComposeFooter(c));
                });
            }).GeneratePdf(filePath);
        });

        return filePath;
    }

    private void ComposeHeader(QContainer container, PeriodePdfData data)
    {
        container.Column(column =>
        {
            // Title bar
            column.Item().Background(PrimaryBlue).Padding(15).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("RAPPORT DE PERIODE")
                        .FontSize(20).Bold().FontColor(White);
                    col.Item().Text($"Station: BombaProMax")
                        .FontSize(11).FontColor("#B3E5FC");
                });

                row.ConstantItem(150).AlignRight().Column(col =>
                {
                    col.Item().Text($"Periode #{data.PeriodeID}")
                        .FontSize(14).Bold().FontColor(White).AlignRight();
                    col.Item().Text($"Genere le {DateTime.Now:dd/MM/yyyy HH:mm}")
                        .FontSize(9).FontColor("#B3E5FC").AlignRight();
                });
            });

            // Info row
            column.Item().Background(LightGray).Padding(10).Row(row =>
            {
                row.RelativeItem().Text(text =>
                {
                    text.Span("Date: ").SemiBold();
                    text.Span($"{data.DateDebut:dd/MM/yyyy HH:mm} - {data.DateFin:HH:mm}");
                });
                row.RelativeItem().Text(text =>
                {
                    text.Span("Employe: ").SemiBold();
                    text.Span(data.EmployeNom ?? "Non assigne");
                });
                row.RelativeItem().Text(text =>
                {
                    text.Span("Duree: ").SemiBold();
                    var duree = data.DateFin - data.DateDebut;
                    text.Span($"{(int)duree.TotalHours}h {duree.Minutes:D2}min");
                });
            });

            column.Item().Height(10);
        });
    }

    private void ComposeContent(QContainer container, PeriodePdfData data)
    {
        container.Column(column =>
        {
            // Financial Summary
            column.Item().Element(c => ComposeFinancialSummary(c, data));
            column.Item().Height(15);

            // Two columns: Reservoirs + Products
            column.Item().Row(row =>
            {
                row.RelativeItem().Element(c => ComposeReservoirSection(c, data));
                row.ConstantItem(15);
                row.RelativeItem().Element(c => ComposeProductSection(c, data));
            });
            column.Item().Height(15);

            // Marge Analysis
            column.Item().Element(c => ComposeMargeSection(c, data));
            column.Item().Height(15);

            // Pump Details Table
            column.Item().Element(c => ComposePumpTable(c, data));

            // Signature section
            column.Item().Height(30);
            column.Item().Element(c => ComposeSignatureSection(c));
        });
    }

    private void ComposeFinancialSummary(QContainer container, PeriodePdfData data)
    {
        container.Border(1).BorderColor("#E0E0E0").Background(White).Padding(15).Column(column =>
        {
            column.Item().Text("RESUME FINANCIER").FontSize(12).Bold().FontColor(DarkBlue);
            column.Item().Height(10);

            column.Item().Row(row =>
            {
                // Quantite Totale
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("Quantite Totale").FontSize(9).FontColor(Gray);
                    col.Item().Text($"{data.TotalQuantite:N2} L").FontSize(22).Bold().FontColor("#333");
                });

                // TPE
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("TPE").FontSize(9).FontColor(Gray);
                    col.Item().Text($"{data.TPE:N2} MAD").FontSize(16).Bold().FontColor(PrimaryBlue);
                });

                // Especes
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("Especes").FontSize(9).FontColor(Gray);
                    col.Item().Text($"{data.Especes:N2} MAD").FontSize(16).Bold().FontColor(Green);
                });

                // Recette
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("Recette").FontSize(9).FontColor(Gray);
                    col.Item().Text($"{data.Recette:N2} MAD").FontSize(16).Bold().FontColor(Orange);
                });

                // Ecart
                row.RelativeItem().Column(col =>
                {
                    var ecart = (data.TPE + data.Especes) - data.Recette;
                    var ecartColor = ecart >= 0 ? Green : Red;
                    var ecartSign = ecart >= 0 ? "+" : "";
                    col.Item().Text("Ecart").FontSize(9).FontColor(Gray);
                    col.Item().Text($"{ecartSign}{ecart:N2} MAD").FontSize(16).Bold().FontColor(ecartColor);
                });
            });
        });
    }

    private void ComposeReservoirSection(QContainer container, PeriodePdfData data)
    {
        container.Border(1).BorderColor("#E0E0E0").Background(White).Padding(12).Column(column =>
        {
            column.Item().Text("TOTAUX PAR RESERVOIR").FontSize(11).Bold().FontColor(DarkBlue);
            column.Item().Height(8);

            foreach (var reservoir in data.TotauxParReservoir)
            {
                column.Item().Row(row =>
                {
                    row.RelativeItem(2).Column(col =>
                    {
                        col.Item().Text(reservoir.ReservoirNumero).FontSize(10).Bold();
                        col.Item().Text(reservoir.ProduitNom ?? "").FontSize(8).FontColor(Gray);
                    });
                    row.RelativeItem().Text($"-{reservoir.QuantiteConsommee:N2} L")
                        .FontSize(10).FontColor(Red).AlignCenter();
                    row.RelativeItem().Text($"{reservoir.Montant:N2} MAD")
                        .FontSize(10).Bold().AlignRight();
                });
                column.Item().Height(4);
            }

            if (data.TotauxParReservoir.Count == 0)
            {
                column.Item().Text("Aucune donnee").FontSize(9).FontColor(Gray);
            }
        });
    }

    private void ComposeProductSection(QContainer container, PeriodePdfData data)
    {
        container.Border(1).BorderColor("#E0E0E0").Background(White).Padding(12).Column(column =>
        {
            column.Item().Text("TOTAUX PAR PRODUIT").FontSize(11).Bold().FontColor(DarkBlue);
            column.Item().Height(8);

            var colors = new[] { "#4CAF50", "#2196F3", "#FF9800", "#9C27B0", "#795548" };
            var colorIndex = 0;

            foreach (var produit in data.TotauxParProduit)
            {
                var color = colors[colorIndex % colors.Length];
                colorIndex++;

                column.Item().Row(row =>
                {
                    row.ConstantItem(4).Background(color).Height(16);
                    row.ConstantItem(6);
                    row.RelativeItem(2).Text(produit.ProduitNom).FontSize(10).Bold();
                    row.RelativeItem().Text($"{produit.Quantite:N2} L").FontSize(10).FontColor(Gray).AlignCenter();
                    row.RelativeItem().Text($"{produit.Montant:N2} MAD").FontSize(10).Bold().FontColor(color).AlignRight();
                });
                column.Item().Height(4);
            }

            if (data.TotauxParProduit.Count == 0)
            {
                column.Item().Text("Aucune donnee").FontSize(9).FontColor(Gray);
            }
        });
    }

    private void ComposeMargeSection(QContainer container, PeriodePdfData data)
    {
        container.Border(1).BorderColor("#E0E0E0").Background(White).Padding(12).Column(column =>
        {
            column.Item().Text("ANALYSE DES MARGES (FIFO)").FontSize(11).Bold().FontColor(DarkBlue);
            column.Item().Height(8);

            // Header row
            column.Item().Background(LightGray).Padding(6).Row(row =>
            {
                row.RelativeItem(2).Text("Produit").FontSize(9).Bold();
                row.RelativeItem().Text("Quantite").FontSize(9).Bold().AlignCenter();
                row.RelativeItem().Text("Cout Achat").FontSize(9).Bold().AlignRight();
                row.RelativeItem().Text("Vente").FontSize(9).Bold().AlignRight();
                row.RelativeItem().Text("Marge").FontSize(9).Bold().AlignRight();
            });

            foreach (var marge in data.MargesParProduit)
            {
                var margeColor = marge.Marge >= 0 ? Green : Red;
                column.Item().Padding(6).Row(row =>
                {
                    row.RelativeItem(2).Text(marge.ProduitNom ?? "N/A").FontSize(9);
                    row.RelativeItem().Text($"{marge.Quantite:N2} L").FontSize(9).AlignCenter();
                    row.RelativeItem().Text($"{marge.CoutAchat:N2}").FontSize(9).AlignRight();
                    row.RelativeItem().Text($"{marge.Vente:N2}").FontSize(9).AlignRight();
                    row.RelativeItem().Text($"{marge.Marge:N2} ({marge.MargePercent}%)")
                        .FontSize(9).Bold().FontColor(margeColor).AlignRight();
                });
            }

            // Total row
            column.Item().Background("#E3F2FD").Padding(6).Row(row =>
            {
                row.RelativeItem(2).Text("TOTAL").FontSize(10).Bold().FontColor(PrimaryBlue);
                row.RelativeItem().Text($"{data.TotalQuantite:N2} L").FontSize(10).Bold().AlignCenter();
                row.RelativeItem().Text($"{data.TotalCoutAchat:N2}").FontSize(10).Bold().AlignRight();
                row.RelativeItem().Text($"{data.Recette:N2}").FontSize(10).Bold().AlignRight();
                var totalMarge = data.Recette - data.TotalCoutAchat;
                var margeColor = totalMarge >= 0 ? Green : Red;
                row.RelativeItem().Text($"{totalMarge:N2} MAD")
                    .FontSize(10).Bold().FontColor(margeColor).AlignRight();
            });
        });
    }

    private void ComposePumpTable(QContainer container, PeriodePdfData data)
    {
        container.Border(1).BorderColor("#E0E0E0").Background(White).Column(column =>
        {
            column.Item().Padding(12).Text("DETAILS PAR POMPE").FontSize(11).Bold().FontColor(DarkBlue);

            // Table header
            column.Item().Background(LightGray).Padding(8).Row(row =>
            {
                row.RelativeItem().Text("Pompe").FontSize(8).Bold();
                row.RelativeItem().Text("Produit").FontSize(8).Bold();
                row.RelativeItem().Text("Reservoir").FontSize(8).Bold();
                row.RelativeItem().Text("Elec D").FontSize(8).Bold().FontColor(PrimaryBlue).AlignCenter();
                row.RelativeItem().Text("Elec F").FontSize(8).Bold().FontColor(PrimaryBlue).AlignCenter();
                row.RelativeItem().Text("Meca D").FontSize(8).Bold().FontColor("#795548").AlignCenter();
                row.RelativeItem().Text("Meca F").FontSize(8).Bold().FontColor("#795548").AlignCenter();
                row.RelativeItem().Text("Quantite").FontSize(8).Bold().AlignCenter();
                row.RelativeItem().Text("Montant").FontSize(8).Bold().AlignRight();
            });

            // Table rows
            var isAlternate = false;
            foreach (var pompe in data.DetailsParPompe)
            {
                var bgColor = isAlternate ? "#FAFAFA" : White;
                isAlternate = !isAlternate;

                column.Item().Background(bgColor).Padding(6).Row(row =>
                {
                    row.RelativeItem().Text(pompe.PompeNumero).FontSize(9).Bold();
                    row.RelativeItem().Text(pompe.ProduitNom ?? "N/A").FontSize(8);
                    row.RelativeItem().Text(pompe.ReservoirNumero ?? "N/A").FontSize(8);
                    row.RelativeItem().Text($"{pompe.CompteurElecDebut:N0}").FontSize(8).FontColor(PrimaryBlue).AlignCenter();
                    row.RelativeItem().Text($"{pompe.CompteurElecFin:N0}").FontSize(8).FontColor(PrimaryBlue).AlignCenter();
                    row.RelativeItem().Text($"{pompe.CompteurMecaDebut:N0}").FontSize(8).FontColor("#795548").AlignCenter();
                    row.RelativeItem().Text($"{pompe.CompteurMecaFin:N0}").FontSize(8).FontColor("#795548").AlignCenter();
                    row.RelativeItem().Text($"{pompe.QuantiteVendue:N2} L").FontSize(9).Bold().AlignCenter();
                    row.RelativeItem().Text($"{pompe.Montant:N2}").FontSize(9).Bold().FontColor(PrimaryBlue).AlignRight();
                });
            }

            // Table footer
            column.Item().Background("#E3F2FD").Padding(8).Row(row =>
            {
                row.RelativeItem().Text("TOTAL").FontSize(10).Bold().FontColor(PrimaryBlue);
                row.RelativeItem();
                row.RelativeItem();
                row.RelativeItem();
                row.RelativeItem();
                row.RelativeItem();
                row.RelativeItem();
                row.RelativeItem().Text($"{data.TotalQuantite:N2} L").FontSize(10).Bold().FontColor(PrimaryBlue).AlignCenter();
                row.RelativeItem().Text($"{data.Recette:N2} MAD").FontSize(10).Bold().FontColor(PrimaryBlue).AlignRight();
            });
        });
    }

    private void ComposeSignatureSection(QContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                col.Item().Text("Signature Employe:").FontSize(9);
                col.Item().Height(5);
                col.Item().LineHorizontal(1).LineColor(Gray);
                col.Item().Height(25);
            });

            row.ConstantItem(50);

            row.RelativeItem().Column(col =>
            {
                col.Item().Text("Signature Gerant:").FontSize(9);
                col.Item().Height(5);
                col.Item().LineHorizontal(1).LineColor(Gray);
                col.Item().Height(25);
            });
        });
    }

    private void ComposeFooter(QContainer container)
    {
        container.AlignCenter().Text(text =>
        {
            text.Span("BombaProMax - Rapport genere automatiquement - ")
                .FontSize(8).FontColor(Gray);
            text.CurrentPageNumber().FontSize(8).FontColor(Gray);
            text.Span(" / ").FontSize(8).FontColor(Gray);
            text.TotalPages().FontSize(8).FontColor(Gray);
        });
    }

    #endregion

    #region Facture PDF Generation

    /// <summary>
    /// Generates a PDF report for a Facture and saves it to the specified path.
    /// </summary>
    public async Task<string> GenerateFactureReportAsync(FacturePdfData data)
    {
        // Create output directory
        var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var outputDir = Path.Combine(documentsPath, "BombaProMax", "Factures");
        Directory.CreateDirectory(outputDir);

        // Generate filename
        var fileName = $"Facture_{data.NumeroFacture.Replace("/", "-")}_{data.DateFacture:yyyy-MM-dd}.pdf";
        var filePath = Path.Combine(outputDir, fileName);

        // Generate PDF
        await Task.Run(() =>
        {
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Element(c => ComposeFactureHeader(c, data));
                    page.Content().Element(c => ComposeFactureContent(c, data));
                    page.Footer().Element(c => ComposeFooter(c));
                });
            }).GeneratePdf(filePath);
        });

        return filePath;
    }

    private void ComposeFactureHeader(QContainer container, FacturePdfData data)
    {
        container.Column(column =>
        {
            // Title bar
            column.Item().Background(FactureGreen).Padding(15).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("FACTURE")
                        .FontSize(24).Bold().FontColor(White);
                    col.Item().Text("BombaProMax - Station Service")
                        .FontSize(11).FontColor("#C8E6C9");
                });

                row.ConstantItem(180).AlignRight().Column(col =>
                {
                    col.Item().Text(data.NumeroFacture)
                        .FontSize(16).Bold().FontColor(White).AlignRight();
                    col.Item().Text($"Date: {data.DateFacture:dd/MM/yyyy}")
                        .FontSize(10).FontColor("#C8E6C9").AlignRight();
                    var isPaid = data.Statut?.ToLower().Contains("pay") == true;
                    var statutText = isPaid ? "PAYEE" : "NON PAYEE";
                    col.Item().Text(statutText)
                        .FontSize(10).Bold().FontColor(isPaid ? "#C8E6C9" : "#FFCDD2").AlignRight();
                });
            });

            column.Item().Height(10);
        });
    }

    private void ComposeFactureContent(QContainer container, FacturePdfData data)
    {
        container.Column(column =>
        {
            // Client Info Section
            column.Item().Row(row =>
            {
                row.RelativeItem().Element(c => ComposeFactureClientInfo(c, data));
                row.ConstantItem(20);
                row.RelativeItem().Element(c => ComposeFactureSummary(c, data));
            });
            column.Item().Height(20);

            // Linked BLs (if any)
            if (data.BonsLivraisonLies.Count > 0)
            {
                column.Item().Element(c => ComposeFactureLinkedBLs(c, data));
                column.Item().Height(15);
            }

            // Elements Table
            column.Item().Element(c => ComposeFactureElementsTable(c, data));
            column.Item().Height(20);

            // Total Section
            column.Item().Element(c => ComposeFactureTotalSection(c, data));
            column.Item().Height(30);

            // Signature section
            column.Item().Element(c => ComposeFactureSignatureSection(c));
        });
    }

    private void ComposeFactureClientInfo(QContainer container, FacturePdfData data)
    {
        container.Border(1).BorderColor("#E0E0E0").Background(White).Padding(15).Column(column =>
        {
            column.Item().Text("INFORMATIONS CLIENT").FontSize(11).Bold().FontColor(FactureGreen);
            column.Item().Height(8);
            column.Item().LineHorizontal(1).LineColor("#E0E0E0");
            column.Item().Height(8);

            column.Item().Text(text =>
            {
                text.Span("Client: ").SemiBold().FontColor(Gray);
                text.Span(data.ClientNom ?? "N/A").Bold();
            });

            if (!string.IsNullOrEmpty(data.ClientNumero))
            {
                column.Item().Text(text =>
                {
                    text.Span("N° Client: ").FontColor(Gray);
                    text.Span(data.ClientNumero);
                });
            }

            if (!string.IsNullOrEmpty(data.ClientContact))
            {
                column.Item().Text(text =>
                {
                    text.Span("Contact: ").FontColor(Gray);
                    text.Span(data.ClientContact);
                });
            }

            if (!string.IsNullOrEmpty(data.ClientAdresse))
            {
                column.Item().Text(text =>
                {
                    text.Span("Adresse: ").FontColor(Gray);
                    text.Span(data.ClientAdresse);
                });
            }
        });
    }

    private void ComposeFactureSummary(QContainer container, FacturePdfData data)
    {
        container.Border(1).BorderColor("#E0E0E0").Background(White).Padding(15).Column(column =>
        {
            column.Item().Text("RESUME").FontSize(11).Bold().FontColor(FactureGreen);
            column.Item().Height(8);
            column.Item().LineHorizontal(1).LineColor("#E0E0E0");
            column.Item().Height(8);

            column.Item().Text(text =>
            {
                text.Span("Nombre d'elements: ").FontColor(Gray);
                text.Span($"{data.Elements.Count}").Bold();
            });

            column.Item().Text(text =>
            {
                text.Span("Statut: ").FontColor(Gray);
                var isPaid = data.Statut?.ToLower().Contains("pay") == true;
                text.Span(data.Statut ?? "N/A").Bold().FontColor(isPaid ? Green : Orange);
            });

            if (data.DatePaiement.HasValue)
            {
                column.Item().Text(text =>
                {
                    text.Span("Date Paiement: ").FontColor(Gray);
                    text.Span(data.DatePaiement.Value.ToString("dd/MM/yyyy")).Bold().FontColor(Green);
                });
            }

            if (!string.IsNullOrEmpty(data.MoyenPaiementNom))
            {
                column.Item().Text(text =>
                {
                    text.Span("Moyen de Paiement: ").FontColor(Gray);
                    text.Span(data.MoyenPaiementNom);
                });
            }
        });
    }

    private void ComposeFactureLinkedBLs(QContainer container, FacturePdfData data)
    {
        container.Border(1).BorderColor("#E0E0E0").Background(White).Padding(12).Column(column =>
        {
            column.Item().Text("BONS DE LIVRAISON LIES").FontSize(11).Bold().FontColor(FactureGreen);
            column.Item().Height(8);

            foreach (var bl in data.BonsLivraisonLies)
            {
                column.Item().Row(row =>
                {
                    row.RelativeItem(2).Text(bl.NumeroBL).FontSize(10).Bold().FontColor(BLBlue);
                    row.RelativeItem().Text(bl.DateBL.ToString("dd/MM/yyyy")).FontSize(10).FontColor(Gray).AlignCenter();
                    row.RelativeItem().Text($"{bl.MontantBL:N2} MAD").FontSize(10).Bold().AlignRight();
                });
                column.Item().Height(4);
            }
        });
    }

    private void ComposeFactureElementsTable(QContainer container, FacturePdfData data)
    {
        container.Border(1).BorderColor("#E0E0E0").Background(White).Column(column =>
        {
            column.Item().Padding(12).Text("ELEMENTS DE LA FACTURE").FontSize(11).Bold().FontColor(FactureGreen);

            // Table header
            column.Item().Background(LightGray).Padding(8).Row(row =>
            {
                row.RelativeItem(3).Text("Designation").FontSize(9).Bold();
                row.RelativeItem().Text("Quantite").FontSize(9).Bold().AlignCenter();
                row.RelativeItem().Text("Prix Unit.").FontSize(9).Bold().AlignRight();
                row.RelativeItem(1.5f).Text("Montant").FontSize(9).Bold().AlignRight();
            });

            // Table rows
            var isAlternate = false;
            foreach (var element in data.Elements)
            {
                var bgColor = isAlternate ? "#FAFAFA" : White;
                isAlternate = !isAlternate;

                column.Item().Background(bgColor).Padding(8).Row(row =>
                {
                    row.RelativeItem(3).Text(element.DisplayName).FontSize(10);
                    row.RelativeItem().Text(element.Quantite.ToString()).FontSize(10).Bold().AlignCenter();
                    row.RelativeItem().Text($"{element.PrixUnitaire:N2}").FontSize(10).AlignRight();
                    row.RelativeItem(1.5f).Text($"{element.MontantLigne:N2} MAD").FontSize(10).Bold().FontColor(FactureGreen).AlignRight();
                });
            }

            if (data.Elements.Count == 0)
            {
                column.Item().Padding(12).Text("Aucun element").FontSize(10).FontColor(Gray);
            }
        });
    }

    private void ComposeFactureTotalSection(QContainer container, FacturePdfData data)
    {
        container.AlignRight().Width(250).Border(1).BorderColor("#E0E0E0").Background("#E8F5E9").Padding(15).Column(column =>
        {
            column.Item().Row(row =>
            {
                row.RelativeItem().Text("TOTAL TTC").FontSize(14).Bold().FontColor(FactureGreen);
                row.RelativeItem().Text($"{data.MontantTotal:N2} MAD").FontSize(18).Bold().FontColor(FactureGreen).AlignRight();
            });
        });
    }

    private void ComposeFactureSignatureSection(QContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                col.Item().Text("Signature Client:").FontSize(9).FontColor(Gray);
                col.Item().Height(5);
                col.Item().LineHorizontal(1).LineColor(Gray);
                col.Item().Height(30);
            });

            row.ConstantItem(50);

            row.RelativeItem().Column(col =>
            {
                col.Item().Text("Cachet et Signature:").FontSize(9).FontColor(Gray);
                col.Item().Height(5);
                col.Item().LineHorizontal(1).LineColor(Gray);
                col.Item().Height(30);
            });
        });
    }

    #endregion

    #region Bon de Livraison PDF Generation

    /// <summary>
    /// Generates a PDF report for a Bon de Livraison and saves it to the specified path.
    /// </summary>
    public async Task<string> GenerateBLReportAsync(BonLivraisonPdfData data)
    {
        // Create output directory
        var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var outputDir = Path.Combine(documentsPath, "BombaProMax", "BonsLivraison");
        Directory.CreateDirectory(outputDir);

        // Generate filename
        var fileName = $"BL_{data.NumeroBL.Replace("/", "-")}_{data.DateBL:yyyy-MM-dd}.pdf";
        var filePath = Path.Combine(outputDir, fileName);

        // Generate PDF
        await Task.Run(() =>
        {
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Element(c => ComposeBLHeader(c, data));
                    page.Content().Element(c => ComposeBLContent(c, data));
                    page.Footer().Element(c => ComposeFooter(c));
                });
            }).GeneratePdf(filePath);
        });

        return filePath;
    }

    private void ComposeBLHeader(QContainer container, BonLivraisonPdfData data)
    {
        container.Column(column =>
        {
            // Title bar
            column.Item().Background(BLBlue).Padding(15).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("BON DE LIVRAISON")
                        .FontSize(24).Bold().FontColor(White);
                    col.Item().Text("BombaProMax - Station Service")
                        .FontSize(11).FontColor("#B3D9F2");
                });

                row.ConstantItem(180).AlignRight().Column(col =>
                {
                    col.Item().Text(data.NumeroBL)
                        .FontSize(16).Bold().FontColor(White).AlignRight();
                    col.Item().Text($"Date: {data.DateBL:dd/MM/yyyy}")
                        .FontSize(10).FontColor("#B3D9F2").AlignRight();
                    var statutText = data.EstFacture ? "FACTURE" : "NON FACTURE";
                    col.Item().Text(statutText)
                        .FontSize(10).Bold().FontColor(data.EstFacture ? "#C8E6C9" : "#FFCDD2").AlignRight();
                });
            });

            column.Item().Height(10);
        });
    }

    private void ComposeBLContent(QContainer container, BonLivraisonPdfData data)
    {
        container.Column(column =>
        {
            // Client Info Section
            column.Item().Row(row =>
            {
                row.RelativeItem().Element(c => ComposeBLClientInfo(c, data));
                row.ConstantItem(20);
                row.RelativeItem().Element(c => ComposeBLSummary(c, data));
            });
            column.Item().Height(20);

            // Product Totals
            if (data.TotauxParProduit.Count > 0)
            {
                column.Item().Element(c => ComposeBLProductTotals(c, data));
                column.Item().Height(15);
            }

            // Details Table
            column.Item().Element(c => ComposeBLDetailsTable(c, data));
            column.Item().Height(20);

            // Total Section
            column.Item().Element(c => ComposeBLTotalSection(c, data));
            column.Item().Height(15);

            // Notes (if any)
            if (!string.IsNullOrEmpty(data.Notes))
            {
                column.Item().Element(c => ComposeBLNotesSection(c, data));
                column.Item().Height(15);
            }

            // Signature section
            column.Item().Height(20);
            column.Item().Element(c => ComposeBLSignatureSection(c));
        });
    }

    private void ComposeBLClientInfo(QContainer container, BonLivraisonPdfData data)
    {
        container.Border(1).BorderColor("#E0E0E0").Background(White).Padding(15).Column(column =>
        {
            column.Item().Text("INFORMATIONS CLIENT").FontSize(11).Bold().FontColor(BLBlue);
            column.Item().Height(8);
            column.Item().LineHorizontal(1).LineColor("#E0E0E0");
            column.Item().Height(8);

            column.Item().Text(text =>
            {
                text.Span("Client: ").SemiBold().FontColor(Gray);
                text.Span(data.ClientNom ?? "N/A").Bold();
            });

            if (!string.IsNullOrEmpty(data.ClientNumero))
            {
                column.Item().Text(text =>
                {
                    text.Span("N° Client: ").FontColor(Gray);
                    text.Span(data.ClientNumero);
                });
            }

            if (!string.IsNullOrEmpty(data.ClientContact))
            {
                column.Item().Text(text =>
                {
                    text.Span("Contact: ").FontColor(Gray);
                    text.Span(data.ClientContact);
                });
            }

            if (!string.IsNullOrEmpty(data.ClientAdresse))
            {
                column.Item().Text(text =>
                {
                    text.Span("Adresse: ").FontColor(Gray);
                    text.Span(data.ClientAdresse);
                });
            }
        });
    }

    private void ComposeBLSummary(QContainer container, BonLivraisonPdfData data)
    {
        container.Border(1).BorderColor("#E0E0E0").Background(White).Padding(15).Column(column =>
        {
            column.Item().Text("RESUME").FontSize(11).Bold().FontColor(BLBlue);
            column.Item().Height(8);
            column.Item().LineHorizontal(1).LineColor("#E0E0E0");
            column.Item().Height(8);

            column.Item().Text(text =>
            {
                text.Span("Nombre de lignes: ").FontColor(Gray);
                text.Span($"{data.Details.Count}").Bold();
            });

            column.Item().Text(text =>
            {
                text.Span("Quantite totale: ").FontColor(Gray);
                var totalQte = data.Details.Sum(d => d.Quantite);
                text.Span($"{totalQte}").Bold();
            });

            column.Item().Text(text =>
            {
                text.Span("Statut: ").FontColor(Gray);
                text.Span(data.EstFacture ? "Facture" : "Non Facture").Bold().FontColor(data.EstFacture ? Green : Orange);
            });
        });
    }

    private void ComposeBLProductTotals(QContainer container, BonLivraisonPdfData data)
    {
        container.Border(1).BorderColor("#E0E0E0").Background(White).Padding(12).Column(column =>
        {
            column.Item().Text("TOTAUX PAR PRODUIT").FontSize(11).Bold().FontColor(BLBlue);
            column.Item().Height(8);

            var colors = new[] { "#4CAF50", "#2196F3", "#FF9800", "#9C27B0", "#795548" };
            var colorIndex = 0;

            foreach (var produit in data.TotauxParProduit)
            {
                var color = colors[colorIndex % colors.Length];
                colorIndex++;

                column.Item().Row(row =>
                {
                    row.ConstantItem(4).Background(color).Height(16);
                    row.ConstantItem(6);
                    row.RelativeItem(2).Text(produit.ProduitNom).FontSize(10).Bold();
                    row.RelativeItem().Text($"x{produit.QuantiteTotale}").FontSize(10).FontColor(Gray).AlignCenter();
                    row.RelativeItem().Text($"{produit.MontantTotal:N2} MAD").FontSize(10).Bold().FontColor(color).AlignRight();
                });
                column.Item().Height(4);
            }
        });
    }

    private void ComposeBLDetailsTable(QContainer container, BonLivraisonPdfData data)
    {
        container.Border(1).BorderColor("#E0E0E0").Background(White).Column(column =>
        {
            column.Item().Padding(12).Text("DETAILS DU BON DE LIVRAISON").FontSize(11).Bold().FontColor(BLBlue);

            // Table header
            column.Item().Background(LightGray).Padding(8).Row(row =>
            {
                row.RelativeItem(3).Text("Designation").FontSize(9).Bold();
                row.RelativeItem().Text("Quantite").FontSize(9).Bold().AlignCenter();
                row.RelativeItem().Text("Prix Unit.").FontSize(9).Bold().AlignRight();
                row.RelativeItem(1.5f).Text("Montant").FontSize(9).Bold().AlignRight();
            });

            // Table rows
            var isAlternate = false;
            foreach (var detail in data.Details)
            {
                var bgColor = isAlternate ? "#FAFAFA" : White;
                isAlternate = !isAlternate;

                column.Item().Background(bgColor).Padding(8).Row(row =>
                {
                    row.RelativeItem(3).Text(detail.DisplayName).FontSize(10);
                    row.RelativeItem().Text(detail.Quantite.ToString()).FontSize(10).Bold().AlignCenter();
                    row.RelativeItem().Text($"{detail.PrixUnitaire:N2}").FontSize(10).AlignRight();
                    row.RelativeItem(1.5f).Text($"{detail.MontantLigne:N2} MAD").FontSize(10).Bold().FontColor(BLBlue).AlignRight();
                });
            }

            if (data.Details.Count == 0)
            {
                column.Item().Padding(12).Text("Aucun detail").FontSize(10).FontColor(Gray);
            }
        });
    }

    private void ComposeBLTotalSection(QContainer container, BonLivraisonPdfData data)
    {
        container.AlignRight().Width(250).Border(1).BorderColor("#E0E0E0").Background("#E3F2FD").Padding(15).Column(column =>
        {
            column.Item().Row(row =>
            {
                row.RelativeItem().Text("TOTAL").FontSize(14).Bold().FontColor(BLBlue);
                row.RelativeItem().Text($"{data.MontantTotal:N2} MAD").FontSize(18).Bold().FontColor(BLBlue).AlignRight();
            });
        });
    }

    private void ComposeBLNotesSection(QContainer container, BonLivraisonPdfData data)
    {
        container.Border(1).BorderColor("#E0E0E0").Background(White).Padding(12).Column(column =>
        {
            column.Item().Text("NOTES").FontSize(11).Bold().FontColor(BLBlue);
            column.Item().Height(6);
            column.Item().Text(data.Notes).FontSize(10).FontColor(Gray);
        });
    }

    private void ComposeBLSignatureSection(QContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                col.Item().Text("Signature Client:").FontSize(9).FontColor(Gray);
                col.Item().Height(5);
                col.Item().LineHorizontal(1).LineColor(Gray);
                col.Item().Height(30);
            });

            row.ConstantItem(50);

            row.RelativeItem().Column(col =>
            {
                col.Item().Text("Signature Livreur:").FontSize(9).FontColor(Gray);
                col.Item().Height(5);
                col.Item().LineHorizontal(1).LineColor(Gray);
                col.Item().Height(30);
            });
        });
    }

    #endregion

    #region Achat (Purchase) PDF Generation

    // Brand color for Achat
    private static readonly string AchatOrange = "#E8A84C";

    /// <summary>
    /// Generates a PDF report for an Achat (Purchase) and saves it to the specified path.
    /// </summary>
    public async Task<string> GenerateAchatReportAsync(AchatPdfData data)
    {
        // Create output directory
        var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var outputDir = Path.Combine(documentsPath, "BombaProMax", "Achats");
        Directory.CreateDirectory(outputDir);

        // Generate filename
        var fileName = $"Achat_{data.Numero.Replace("/", "-")}_{data.Date:yyyy-MM-dd}.pdf";
        var filePath = Path.Combine(outputDir, fileName);

        // Generate PDF
        await Task.Run(() =>
        {
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Element(c => ComposeAchatHeader(c, data));
                    page.Content().Element(c => ComposeAchatContent(c, data));
                    page.Footer().Element(c => ComposeFooter(c));
                });
            }).GeneratePdf(filePath);
        });

        return filePath;
    }

    private void ComposeAchatHeader(QContainer container, AchatPdfData data)
    {
        container.Column(column =>
        {
            // Title bar
            column.Item().Background(AchatOrange).Padding(15).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("BON D'ACHAT")
                        .FontSize(24).Bold().FontColor(White);
                    col.Item().Text("BombaProMax - Station Service")
                        .FontSize(11).FontColor("#FFF3E0");
                });

                row.ConstantItem(180).AlignRight().Column(col =>
                {
                    col.Item().Text(data.Numero)
                        .FontSize(16).Bold().FontColor(White).AlignRight();
                    col.Item().Text($"Date: {data.Date:dd/MM/yyyy}")
                        .FontSize(10).FontColor("#FFF3E0").AlignRight();
                    if (data.LivraisonDefectueuse)
                    {
                        col.Item().Text("LIVRAISON DEFECTUEUSE")
                            .FontSize(9).Bold().FontColor("#FFCDD2").AlignRight();
                    }
                });
            });

            column.Item().Height(10);
        });
    }

    private void ComposeAchatContent(QContainer container, AchatPdfData data)
    {
        container.Column(column =>
        {
            // Purchase Info + Financial Summary
            column.Item().Row(row =>
            {
                row.RelativeItem().Element(c => ComposeAchatPurchaseInfo(c, data));
                row.ConstantItem(20);
                row.RelativeItem().Element(c => ComposeAchatFinancialSummary(c, data));
            });
            column.Item().Height(20);

            // Transport Info + Delivery Status
            column.Item().Row(row =>
            {
                row.RelativeItem().Element(c => ComposeAchatTransportInfo(c, data));
                row.ConstantItem(20);
                row.RelativeItem().Element(c => ComposeAchatDeliveryStatus(c, data));
            });
            column.Item().Height(20);

            // Allocation Status
            column.Item().Element(c => ComposeAchatAllocationStatus(c, data));
            column.Item().Height(15);

            // Allocation Table (if carburant and has allocations)
            if (data.EstCarburant && data.Allocations.Count > 0)
            {
                column.Item().Element(c => ComposeAchatAllocationsTable(c, data));
                column.Item().Height(15);
            }

            // Non-Carburant Message
            if (!data.EstCarburant)
            {
                column.Item().Element(c => ComposeAchatNonCarburantMessage(c));
                column.Item().Height(15);
            }

            // Signature section
            column.Item().Height(20);
            column.Item().Element(c => ComposeAchatSignatureSection(c));
        });
    }

    private void ComposeAchatPurchaseInfo(QContainer container, AchatPdfData data)
    {
        container.Border(1).BorderColor("#E0E0E0").Background(White).Padding(15).Column(column =>
        {
            column.Item().Text("INFORMATIONS ACHAT").FontSize(11).Bold().FontColor(AchatOrange);
            column.Item().Height(8);
            column.Item().LineHorizontal(1).LineColor("#E0E0E0");
            column.Item().Height(8);

            column.Item().Text(text =>
            {
                text.Span("Fournisseur: ").SemiBold().FontColor(Gray);
                text.Span(data.FournisseurNom ?? "N/A").Bold();
            });

            column.Item().Text(text =>
            {
                text.Span("Produit: ").FontColor(Gray);
                text.Span(data.ProduitNom ?? "N/A").Bold();
            });

            column.Item().Text(text =>
            {
                text.Span("Quantite: ").FontColor(Gray);
                text.Span($"{data.Quantite:N0} L").Bold();
            });

            column.Item().Text(text =>
            {
                text.Span("Date: ").FontColor(Gray);
                text.Span(data.Date.ToString("dd/MM/yyyy"));
            });
        });
    }

    private void ComposeAchatFinancialSummary(QContainer container, AchatPdfData data)
    {
        container.Border(1).BorderColor("#E0E0E0").Background(White).Padding(15).Column(column =>
        {
            column.Item().Text("RESUME FINANCIER").FontSize(11).Bold().FontColor(AchatOrange);
            column.Item().Height(8);
            column.Item().LineHorizontal(1).LineColor("#E0E0E0");
            column.Item().Height(8);

            column.Item().Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("Prix Unitaire").FontSize(9).FontColor(Gray);
                    col.Item().Text($"{data.PrixUnitaire:N2} DH/L").FontSize(14).Bold();
                });
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("Quantite").FontSize(9).FontColor(Gray);
                    col.Item().Text($"{data.Quantite:N0} L").FontSize(14).Bold();
                });
            });

            column.Item().Height(10);
            column.Item().Text("Cout Total").FontSize(9).FontColor(Gray);
            column.Item().Text($"{data.CoutTotal:N2} DH").FontSize(22).Bold().FontColor(AchatOrange);
        });
    }

    private void ComposeAchatTransportInfo(QContainer container, AchatPdfData data)
    {
        container.Border(1).BorderColor("#E0E0E0").Background(White).Padding(15).Column(column =>
        {
            column.Item().Text("TRANSPORT").FontSize(11).Bold().FontColor(AchatOrange);
            column.Item().Height(8);
            column.Item().LineHorizontal(1).LineColor("#E0E0E0");
            column.Item().Height(8);

            column.Item().Text(text =>
            {
                text.Span("Chauffeur: ").FontColor(Gray);
                text.Span(data.ChauffeurNom ?? "Non assigne").Bold();
            });

            column.Item().Text(text =>
            {
                text.Span("Camion: ").FontColor(Gray);
                text.Span(data.CamionImmatriculation ?? "Non assigne").Bold();
            });
        });
    }

    private void ComposeAchatDeliveryStatus(QContainer container, AchatPdfData data)
    {
        var bgColor = data.LivraisonDefectueuse ? "#FFEBEE" : "#E8F5E9";
        var textColor = data.LivraisonDefectueuse ? Red : Green;
        var statusText = data.LivraisonDefectueuse ? "LIVRAISON DEFECTUEUSE" : "LIVRAISON OK";
        var statusIcon = data.LivraisonDefectueuse ? "!" : "OK";
        var subText = data.LivraisonDefectueuse 
            ? "Problemes signales lors de la reception" 
            : "Aucun probleme signale";

        container.Border(1).BorderColor("#E0E0E0").Background(bgColor).Padding(15).Column(column =>
        {
            column.Item().Text("STATUT LIVRAISON").FontSize(11).Bold().FontColor(AchatOrange);
            column.Item().Height(8);
            column.Item().LineHorizontal(1).LineColor("#E0E0E0");
            column.Item().Height(8);

            column.Item().Row(row =>
            {
                row.ConstantItem(40).AlignCenter().Text(statusIcon).FontSize(24).Bold().FontColor(textColor);
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text(statusText).FontSize(14).Bold().FontColor(textColor);
                    col.Item().Text(subText).FontSize(10).FontColor(Gray);
                });
            });
        });
    }

    private void ComposeAchatAllocationStatus(QContainer container, AchatPdfData data)
    {
        container.Border(1).BorderColor("#E0E0E0").Background(White).Padding(15).Column(column =>
        {
            column.Item().Row(row =>
            {
                row.RelativeItem().Text("ALLOCATION AUX RESERVOIRS").FontSize(11).Bold().FontColor(AchatOrange);

                // Status badge
                var badgeBg = data.EstCompletementAlloue ? "#E8F5E9" : 
                              data.TotalAlloue > 0 ? "#FFF3E0" : "#FFEBEE";
                var badgeColor = data.EstCompletementAlloue ? Green : 
                                 data.TotalAlloue > 0 ? Orange : Red;
                var badgeText = data.EstCompletementAlloue ? "Entierement alloue" : 
                                data.TotalAlloue > 0 ? "Partiellement alloue" : "Non alloue";

                if (!data.EstCarburant)
                {
                    badgeBg = "#E3F2FD";
                    badgeColor = PrimaryBlue;
                    badgeText = "Non-Carburant";
                }

                row.ConstantItem(130).AlignRight().Background(badgeBg).Padding(6).Text(badgeText)
                    .FontSize(9).Bold().FontColor(badgeColor).AlignCenter();
            });

            column.Item().Height(12);

            column.Item().Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("Quantite Achetee").FontSize(9).FontColor(Gray);
                    col.Item().Text($"{data.Quantite:N0} L").FontSize(14).Bold();
                });
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("Quantite Allouee").FontSize(9).FontColor(Gray);
                    col.Item().Text($"{data.TotalAlloue:N0} L").FontSize(14).Bold().FontColor(Green);
                });
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("Quantite Restante").FontSize(9).FontColor(Gray);
                    var restantColor = data.QuantiteRestante <= 0 ? Green : Orange;
                    col.Item().Text($"{data.QuantiteRestante:N0} L").FontSize(14).Bold().FontColor(restantColor);
                });
            });
        });
    }

    private void ComposeAchatAllocationsTable(QContainer container, AchatPdfData data)
    {
        container.Border(1).BorderColor("#E0E0E0").Background(White).Column(column =>
        {
            column.Item().Padding(12).Text("DETAILS DES ALLOCATIONS").FontSize(11).Bold().FontColor(AchatOrange);

            // Table header
            column.Item().Background(LightGray).Padding(8).Row(row =>
            {
                row.RelativeItem(1.5f).Text("Reservoir").FontSize(9).Bold();
                row.RelativeItem(1.5f).Text("Produit").FontSize(9).Bold();
                row.RelativeItem().Text("Quantite").FontSize(9).Bold().AlignCenter();
                row.RelativeItem().Text("Date").FontSize(9).Bold().AlignCenter();
                row.RelativeItem().Text("Statut").FontSize(9).Bold().AlignCenter();
            });

            // Table rows
            var isAlternate = false;
            decimal totalAlloue = 0;

            foreach (var allocation in data.Allocations)
            {
                var bgColor = isAlternate ? "#FAFAFA" : White;
                isAlternate = !isAlternate;
                totalAlloue += allocation.QuantiteAllouee;

                var statutColor = allocation.Statut switch
                {
                    "Confirmée" or "Confirmee" => Green,
                    "En Attente" => Orange,
                    "Annulée" or "Annulee" => Red,
                    _ => Gray
                };

                column.Item().Background(bgColor).Padding(8).Row(row =>
                {
                    row.RelativeItem(1.5f).Text(allocation.ReservoirNumero).FontSize(10).Bold();
                    row.RelativeItem(1.5f).Text(allocation.ProduitNom ?? "N/A").FontSize(10);
                    row.RelativeItem().Text($"{allocation.QuantiteAllouee:N0} L").FontSize(10).Bold().FontColor(AchatOrange).AlignCenter();
                    row.RelativeItem().Text(allocation.DateAllocation.ToString("dd/MM/yyyy")).FontSize(10).AlignCenter();
                    row.RelativeItem().Text(allocation.Statut).FontSize(9).Bold().FontColor(statutColor).AlignCenter();
                });
            }

            // Table footer
            column.Item().Background("#FFF3E0").Padding(8).Row(row =>
            {
                row.RelativeItem(1.5f).Text("TOTAL").FontSize(10).Bold().FontColor(AchatOrange);
                row.RelativeItem(1.5f);
                row.RelativeItem().Text($"{totalAlloue:N0} L").FontSize(10).Bold().FontColor(AchatOrange).AlignCenter();
                row.RelativeItem();
                row.RelativeItem();
            });
        });
    }

    private void ComposeAchatNonCarburantMessage(QContainer container)
    {
        container.Border(1).BorderColor("#E0E0E0").Background("#E3F2FD").Padding(15).Row(row =>
        {
            row.ConstantItem(40).AlignCenter().Text("i").FontSize(20).Bold().FontColor(PrimaryBlue);
            row.RelativeItem().Column(col =>
            {
                col.Item().Text("Produit Non-Carburant").FontSize(12).Bold().FontColor(PrimaryBlue);
                col.Item().Text("Le stock a ete automatiquement mis a jour lors de la creation de l'achat.")
                    .FontSize(10).FontColor(Gray);
            });
        });
    }

    private void ComposeAchatSignatureSection(QContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                col.Item().Text("Signature Receptionnaire:").FontSize(9).FontColor(Gray);
                col.Item().Height(5);
                col.Item().LineHorizontal(1).LineColor(Gray);
                col.Item().Height(30);
            });

            row.ConstantItem(50);

            row.RelativeItem().Column(col =>
            {
                col.Item().Text("Cachet et Signature:").FontSize(9).FontColor(Gray);
                col.Item().Height(5);
                col.Item().LineHorizontal(1).LineColor(Gray);
                col.Item().Height(30);
            });
        });
    }

    #endregion

}

