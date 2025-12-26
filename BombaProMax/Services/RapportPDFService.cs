using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using BombaProMax.Models;

// Alias to avoid conflicts with MAUI types
using QContainer = QuestPDF.Infrastructure.IContainer;

namespace BombaProMax.Services;

/// <summary>
/// Service for generating PDF reports for Rapport data (Ventes, Dépenses, Stock).
/// Uses QuestPDF for clean, code-based PDF generation.
/// </summary>
public class RapportPdfService
{
    // Brand colors
    private static readonly string PrimaryBlue = "#1976D2";
    private static readonly string Green = "#2E7D32";
    private static readonly string Orange = "#E65100";
    private static readonly string Red = "#C62828";
    private static readonly string Gray = "#666666";
    private static readonly string LightGray = "#F5F5F5";
    private static readonly string White = "#FFFFFF";

    static RapportPdfService()
    {
        // Configure QuestPDF license (Community license for open source)
        QuestPDF.Settings.License = LicenseType.Community;
    }

    /// <summary>
    /// Generates a comprehensive PDF report containing Ventes, Dépenses, and Stock data.
    /// </summary>
    public async Task<string> GenerateRapportReportAsync(RapportPdfData data)
    {
        // Create output directory
        var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var outputDir = Path.Combine(documentsPath, "BombaProMax", "Rapports");
        Directory.CreateDirectory(outputDir);

        // Generate filename with period info
        var periodeSafe = data.PeriodeLabel.Replace("/", "-").Replace(" ", "_");
        var fileName = $"Rapport_{periodeSafe}_{data.GeneratedAt:yyyy-MM-dd_HHmm}.pdf";
        var filePath = Path.Combine(outputDir, fileName);

        // Generate PDF
        await Task.Run(() =>
        {
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(25);
                    page.DefaultTextStyle(x => x.FontSize(9));

                    page.Header().Element(c => ComposeRapportHeader(c, data));
                    page.Content().Element(c => ComposeRapportContent(c, data));
                    page.Footer().Element(c => ComposeRapportFooter(c));
                });
            }).GeneratePdf(filePath);
        });

        return filePath;
    }

    private void ComposeRapportHeader(QContainer container, RapportPdfData data)
    {
        container.Column(column =>
        {
            // Title bar
            column.Item().Background(PrimaryBlue).Padding(12).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("RAPPORT COMPLET")
                        .FontSize(18).Bold().FontColor(White);
                    col.Item().Text("BombaProMax - Station Service")
                        .FontSize(10).FontColor("#B3E5FC");
                });

                row.ConstantItem(180).AlignRight().Column(col =>
                {
                    col.Item().Text($"Periode: {data.PeriodeLabel}")
                        .FontSize(12).Bold().FontColor(White).AlignRight();
                    col.Item().Text($"Genere le {data.GeneratedAt:dd/MM/yyyy HH:mm}")
                        .FontSize(9).FontColor("#B3E5FC").AlignRight();
                });
            });

            column.Item().Height(8);
        });
    }

    private void ComposeRapportContent(QContainer container, RapportPdfData data)
    {
        container.Column(column =>
        {
            // SECTION 1: VENTES
            column.Item().Element(c => ComposeRapportVentesSection(c, data.Ventes));
            column.Item().Height(15);

            // SECTION 2: DEPENSES
            column.Item().Element(c => ComposeRapportDepensesSection(c, data.Depenses));
            column.Item().Height(15);

            // SECTION 3: STOCK
            column.Item().Element(c => ComposeRapportStockSection(c, data.Stock));
        });
    }

    #region Ventes Section

    private void ComposeRapportVentesSection(QContainer container, RapportVentesPdfData ventes)
    {
        container.Column(column =>
        {
            // Section Title
            column.Item().Background(Green).Padding(10).Row(row =>
            {
                row.RelativeItem().Text("VENTES").FontSize(14).Bold().FontColor(White);
            });

            column.Item().Border(1).BorderColor("#E0E0E0").Background(White).Padding(12).Column(innerCol =>
            {
                // Summary Cards Row
                innerCol.Item().Row(row =>
                {
                    row.RelativeItem().Element(c => ComposeVentesSummaryCard(c, "Total Ventes", $"{ventes.TotalVentes:N2} MAD", Green));
                    row.ConstantItem(10);
                    row.RelativeItem().Element(c => ComposeVentesSummaryCard(c, "Carburant", $"{ventes.TotalVentesCarburant:N2} MAD", PrimaryBlue, $"{ventes.TotalQuantiteCarburant:N0} L"));
                    row.ConstantItem(10);
                    row.RelativeItem().Element(c => ComposeVentesSummaryCard(c, "Lub/Articles", $"{ventes.TotalVentesLubArticles:N2} MAD", Orange, $"{ventes.TotalQuantiteLubArticles} unites"));
                });

                innerCol.Item().Height(12);

                // Carburant Table
                if (ventes.VentesCarburantParProduit.Count > 0)
                {
                    innerCol.Item().Text("Ventes Carburant par Produit").FontSize(11).Bold().FontColor("#333");
                    innerCol.Item().Height(6);
                    innerCol.Item().Element(c => ComposeVentesCarburantTable(c, ventes.VentesCarburantParProduit));
                    innerCol.Item().Height(10);
                }

                // Lub/Articles Table
                if (ventes.VentesLubArticlesParProduit.Count > 0)
                {
                    innerCol.Item().Text("Ventes Lubrifiants/Articles par Produit").FontSize(11).Bold().FontColor("#333");
                    innerCol.Item().Height(6);
                    innerCol.Item().Element(c => ComposeVentesLubArticlesTable(c, ventes.VentesLubArticlesParProduit));
                }

                if (ventes.VentesCarburantParProduit.Count == 0 && ventes.VentesLubArticlesParProduit.Count == 0)
                {
                    innerCol.Item().Padding(20).AlignCenter().Text("Aucune vente pour cette periode").FontSize(10).FontColor(Gray);
                }
            });
        });
    }

    private void ComposeVentesSummaryCard(QContainer container, string title, string value, string color, string? subtitle = null)
    {
        container.Background("#F8F9FA").Border(1).BorderColor("#E0E0E0").Padding(10).Column(col =>
        {
            col.Item().Text(title).FontSize(9).FontColor(Gray);
            col.Item().Text(value).FontSize(14).Bold().FontColor(color);
            if (!string.IsNullOrEmpty(subtitle))
            {
                col.Item().Text(subtitle).FontSize(8).FontColor("#999");
            }
        });
    }

    private void ComposeVentesCarburantTable(QContainer container, List<RapportVenteCarburantProduitPdfData> items)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(2);
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();
            });

            // Header
            table.Header(header =>
            {
                header.Cell().Background(LightGray).Padding(6).Text("Produit").FontSize(8).Bold();
                header.Cell().Background(LightGray).Padding(6).AlignRight().Text("Quantite").FontSize(8).Bold();
                header.Cell().Background(LightGray).Padding(6).AlignRight().Text("Montant").FontSize(8).Bold();
                header.Cell().Background(LightGray).Padding(6).AlignRight().Text("Periodes").FontSize(8).Bold();
            });

            // Rows
            foreach (var item in items)
            {
                table.Cell().BorderBottom(1).BorderColor("#EEE").Padding(6).Text(item.ProduitNom).FontSize(9).Bold().FontColor(PrimaryBlue);
                table.Cell().BorderBottom(1).BorderColor("#EEE").Padding(6).AlignRight().Text($"{item.TotalQuantite:N0} L").FontSize(9).FontColor(PrimaryBlue);
                table.Cell().BorderBottom(1).BorderColor("#EEE").Padding(6).AlignRight().Text($"{item.TotalMontant:N2}").FontSize(9).Bold();
                table.Cell().BorderBottom(1).BorderColor("#EEE").Padding(6).AlignRight().Text(item.NombrePeriodes.ToString()).FontSize(9).FontColor(Gray);
            }

            // Footer total
            var totalQte = items.Sum(i => i.TotalQuantite);
            var totalMontant = items.Sum(i => i.TotalMontant);
            table.Cell().Background("#E3F2FD").Padding(6).Text("TOTAL").FontSize(9).Bold().FontColor(PrimaryBlue);
            table.Cell().Background("#E3F2FD").Padding(6).AlignRight().Text($"{totalQte:N0} L").FontSize(9).Bold().FontColor(PrimaryBlue);
            table.Cell().Background("#E3F2FD").Padding(6).AlignRight().Text($"{totalMontant:N2}").FontSize(9).Bold().FontColor(PrimaryBlue);
            table.Cell().Background("#E3F2FD").Padding(6);
        });
    }

    private void ComposeVentesLubArticlesTable(QContainer container, List<RapportVenteLubArticleProduitPdfData> items)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(2);      // Produit
                columns.RelativeColumn(1.5f);   // Categorie
                columns.RelativeColumn();       // Quantite
                columns.RelativeColumn();       // Montant
                columns.RelativeColumn();       // Ventes
            });

            // Header
            table.Header(header =>
            {
                header.Cell().Background(LightGray).Padding(6).Text("Produit").FontSize(8).Bold();
                header.Cell().Background(LightGray).Padding(6).Text("Categorie").FontSize(8).Bold();
                header.Cell().Background(LightGray).Padding(6).AlignRight().Text("Quantite").FontSize(8).Bold();
                header.Cell().Background(LightGray).Padding(6).AlignRight().Text("Montant").FontSize(8).Bold();
                header.Cell().Background(LightGray).Padding(6).AlignRight().Text("Ventes").FontSize(8).Bold();
            });

            // Rows
            foreach (var item in items)
            {
                table.Cell().BorderBottom(1).BorderColor("#EEE").Padding(6).Text(item.ProduitNom).FontSize(9);
                table.Cell().BorderBottom(1).BorderColor("#EEE").Padding(6).Text(item.CategorieNom ?? "-").FontSize(8).FontColor(Gray);
                table.Cell().BorderBottom(1).BorderColor("#EEE").Padding(6).AlignRight().Text(item.TotalQuantite.ToString()).FontSize(9).FontColor(Orange);
                table.Cell().BorderBottom(1).BorderColor("#EEE").Padding(6).AlignRight().Text($"{item.TotalMontant:N2}").FontSize(9).Bold();
                table.Cell().BorderBottom(1).BorderColor("#EEE").Padding(6).AlignRight().Text(item.NombreVentes.ToString()).FontSize(9).FontColor(Gray);
            }

            // Footer total
            var totalQte = items.Sum(i => i.TotalQuantite);
            var totalMontant = items.Sum(i => i.TotalMontant);
            table.Cell().Background("#FFF3E0").Padding(6).Text("TOTAL").FontSize(9).Bold().FontColor(Orange);
            table.Cell().Background("#FFF3E0").Padding(6);
            table.Cell().Background("#FFF3E0").Padding(6).AlignRight().Text(totalQte.ToString()).FontSize(9).Bold().FontColor(Orange);
            table.Cell().Background("#FFF3E0").Padding(6).AlignRight().Text($"{totalMontant:N2}").FontSize(9).Bold().FontColor(Orange);
            table.Cell().Background("#FFF3E0").Padding(6);
        });
    }

    #endregion

    #region Depenses Section

    private void ComposeRapportDepensesSection(QContainer container, RapportDepensesPdfData depenses)
    {
        container.Column(column =>
        {
            // Section Title
            column.Item().Background(Red).Padding(10).Row(row =>
            {
                row.RelativeItem().Text("DEPENSES").FontSize(14).Bold().FontColor(White);
            });

            column.Item().Border(1).BorderColor("#E0E0E0").Background(White).Padding(12).Column(innerCol =>
            {
                // Summary Row
                innerCol.Item().Row(row =>
                {
                    row.RelativeItem().Background("#FFEBEE").Border(1).BorderColor("#FFCDD2").Padding(12).Column(col =>
                    {
                        col.Item().Text("Total Depenses").FontSize(9).FontColor(Gray);
                        col.Item().Text($"{depenses.TotalDepenses:N2} MAD").FontSize(18).Bold().FontColor(Red);
                    });
                    row.ConstantItem(15);
                    row.RelativeItem().Background(LightGray).Border(1).BorderColor("#E0E0E0").Padding(12).Column(col =>
                    {
                        col.Item().Text("Nombre de Depenses").FontSize(9).FontColor(Gray);
                        col.Item().Text(depenses.NombreDepenses.ToString()).FontSize(18).Bold().FontColor("#333");
                    });
                });

                innerCol.Item().Height(12);

                // Categories
                if (depenses.DepensesParCategorie.Count > 0)
                {
                    innerCol.Item().Text("Depenses par Categorie").FontSize(11).Bold().FontColor("#333");
                    innerCol.Item().Height(6);
                    innerCol.Item().Element(c => ComposeDepensesCategoriesGrid(c, depenses.DepensesParCategorie));
                    innerCol.Item().Height(10);
                }

                // Details Table
                if (depenses.DepensesDetails.Count > 0)
                {
                    innerCol.Item().Text("Liste des Depenses").FontSize(11).Bold().FontColor("#333");
                    innerCol.Item().Height(6);
                    innerCol.Item().Element(c => ComposeDepensesDetailsTable(c, depenses.DepensesDetails));
                }

                if (depenses.DepensesParCategorie.Count == 0 && depenses.DepensesDetails.Count == 0)
                {
                    innerCol.Item().Padding(20).AlignCenter().Text("Aucune depense pour cette periode").FontSize(10).FontColor(Gray);
                }
            });
        });
    }

    private void ComposeDepensesCategoriesGrid(QContainer container, List<RapportDepenseCategoriePdfData> categories)
    {
        container.Row(row =>
        {
            foreach (var cat in categories)
            {
                row.RelativeItem().Background("#FFF8F8").Border(1).BorderColor("#FFCDD2").Padding(10).Column(col =>
                {
                    col.Item().Text(cat.CategorieNom).FontSize(9).FontColor(Gray);
                    col.Item().Text($"{cat.TotalMontant:N2} MAD").FontSize(12).Bold().FontColor(Red);
                    col.Item().Text($"{cat.NombreDepenses} depenses").FontSize(8).FontColor("#999");
                });
                row.ConstantItem(8);
            }
        });
    }

    private void ComposeDepensesDetailsTable(QContainer container, List<RapportDepenseDetailPdfData> details)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(1.5f);
                columns.RelativeColumn();
                columns.RelativeColumn(1.2f);
                columns.RelativeColumn();
                columns.RelativeColumn(2);
            });

            // Header
            table.Header(header =>
            {
                header.Cell().Background(LightGray).Padding(6).Text("Numero").FontSize(8).Bold();
                header.Cell().Background(LightGray).Padding(6).Text("Date").FontSize(8).Bold();
                header.Cell().Background(LightGray).Padding(6).Text("Categorie").FontSize(8).Bold();
                header.Cell().Background(LightGray).Padding(6).AlignRight().Text("Montant").FontSize(8).Bold();
                header.Cell().Background(LightGray).Padding(6).Text("Description").FontSize(8).Bold();
            });

            // Rows (limit to first 20 to avoid overly long PDFs)
            var displayItems = details.Take(20).ToList();
            foreach (var item in displayItems)
            {
                table.Cell().BorderBottom(1).BorderColor("#EEE").Padding(5).Text(item.Numero ?? "-").FontSize(8).FontColor(PrimaryBlue);
                table.Cell().BorderBottom(1).BorderColor("#EEE").Padding(5).Text(item.DateDisplay).FontSize(8).FontColor(Gray);
                table.Cell().BorderBottom(1).BorderColor("#EEE").Padding(5).Text(item.Categorie ?? "-").FontSize(8);
                table.Cell().BorderBottom(1).BorderColor("#EEE").Padding(5).AlignRight().Text($"{item.Montant:N2}").FontSize(8).Bold().FontColor(Red);
                table.Cell().BorderBottom(1).BorderColor("#EEE").Padding(5).Text(item.Description ?? "-").FontSize(8).FontColor(Gray);
            }

            if (details.Count > 20)
            {
                table.Cell().ColumnSpan(5).Padding(6).AlignCenter()
                    .Text($"... et {details.Count - 20} autres depenses").FontSize(8).FontColor(Gray);
            }
        });
    }

    #endregion

    #region Stock Section

    private void ComposeRapportStockSection(QContainer container, RapportStockPdfData stock)
    {
        container.Column(column =>
        {
            // Section Title
            column.Item().Background(PrimaryBlue).Padding(10).Row(row =>
            {
                row.RelativeItem().Text("STOCK").FontSize(14).Bold().FontColor(White);
            });

            column.Item().Border(1).BorderColor("#E0E0E0").Background(White).Padding(12).Column(innerCol =>
            {
                // Summary Row
                innerCol.Item().Row(row =>
                {
                    row.RelativeItem().Background("#E3F2FD").Border(1).BorderColor("#BBDEFB").Padding(10).Column(col =>
                    {
                        col.Item().Text("Stock Carburant").FontSize(9).FontColor(Gray);
                        col.Item().Text($"{stock.TotalStockCarburantLitres:N0} L").FontSize(14).Bold().FontColor(PrimaryBlue);
                    });
                    row.ConstantItem(10);
                    row.RelativeItem().Background("#F3E5F5").Border(1).BorderColor("#E1BEE7").Padding(10).Column(col =>
                    {
                        col.Item().Text("Stock Produits").FontSize(9).FontColor(Gray);
                        col.Item().Text($"{stock.TotalStockProduits} unites").FontSize(14).Bold().FontColor("#7B1FA2");
                    });
                    row.ConstantItem(10);
                    row.RelativeItem().Background("#FFF3E0").Border(1).BorderColor("#FFE0B2").Padding(10).Column(col =>
                    {
                        col.Item().Text("Achats Periode").FontSize(9).FontColor(Gray);
                        col.Item().Text($"{stock.TotalAchatsPeriode:N2} MAD").FontSize(14).Bold().FontColor(Orange);
                    });
                });

                innerCol.Item().Height(12);

                // Jaugeage Analysis (if available)
                if (stock.JaugeageAnalyse.HasData)
                {
                    innerCol.Item().Element(c => ComposeJaugeageAnalyseSection(c, stock.JaugeageAnalyse));
                    innerCol.Item().Height(10);
                }

                // Reservoir Stock Table
                if (stock.StockCarburant.Count > 0)
                {
                    innerCol.Item().Text("Stock Carburant (Reservoirs)").FontSize(11).Bold().FontColor("#333");
                    innerCol.Item().Height(6);
                    innerCol.Item().Element(c => ComposeStockReservoirTable(c, stock.StockCarburant));
                    innerCol.Item().Height(10);
                }

                // Products Stock Table
                if (stock.StockProduits.Count > 0)
                {
                    innerCol.Item().Text("Stock Produits (Non-Carburant)").FontSize(11).Bold().FontColor("#333");
                    innerCol.Item().Height(6);
                    innerCol.Item().Element(c => ComposeStockProduitsTable(c, stock.StockProduits));
                    innerCol.Item().Height(10);
                }

                // Achats Table
                if (stock.AchatsParProduit.Count > 0)
                {
                    innerCol.Item().Text("Achats de la Periode").FontSize(11).Bold().FontColor("#333");
                    innerCol.Item().Height(6);
                    innerCol.Item().Element(c => ComposeAchatsTable(c, stock.AchatsParProduit));
                }
            });
        });
    }

    private void ComposeJaugeageAnalyseSection(QContainer container, RapportJaugeageAnalysePdfData analyse)
    {
        container.Background("#ECEFF1").Border(1).BorderColor("#CFD8DC").Padding(10).Column(col =>
        {
            // Header
            col.Item().Row(row =>
            {
                row.RelativeItem().Text("Analyse Jaugeage vs Ventes").FontSize(11).Bold().FontColor("#37474F");
                if (!string.IsNullOrEmpty(analyse.PeriodeAnalyse))
                {
                    row.ConstantItem(150).AlignRight().Text($"Periode: {analyse.PeriodeAnalyse}").FontSize(8).FontColor("#78909C");
                }
            });

            col.Item().Height(6);

            // Jaugeage info row
            if (!string.IsNullOrEmpty(analyse.JaugeagePrecedentInfo) || !string.IsNullOrEmpty(analyse.JaugeageActuelInfo))
            {
                col.Item().Row(row =>
                {
                    row.RelativeItem().Background(White).Border(1).BorderColor("#CFD8DC").Padding(8).Column(c =>
                    {
                        c.Item().Text("Jaugeage Precedent (n-1)").FontSize(8).FontColor("#78909C");
                        c.Item().Text(analyse.JaugeagePrecedentInfo ?? "N/A").FontSize(9).Bold().FontColor("#455A64");
                    });
                    row.ConstantItem(10);
                    row.RelativeItem().Background(White).Border(1).BorderColor("#CFD8DC").Padding(8).Column(c =>
                    {
                        c.Item().Text("Jaugeage Actuel (n)").FontSize(8).FontColor("#78909C");
                        c.Item().Text(analyse.JaugeageActuelInfo ?? "N/A").FontSize(9).Bold().FontColor("#455A64");
                    });
                });
                col.Item().Height(8);
            }

            // Comparison Table
            if (analyse.Comparaisons.Count > 0)
            {
                col.Item().Element(c => ComposeJaugeageComparisonTable(c, analyse.Comparaisons));
            }
        });
    }

    private void ComposeJaugeageComparisonTable(QContainer container, List<RapportJaugeageComparisonPdfData> comparaisons)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(1.2f);
                columns.RelativeColumn(1.2f);
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn(1.2f);
            });

            // Header
            table.Header(header =>
            {
                header.Cell().Background("#546E7A").Padding(5).Text("Reservoir").FontSize(7).Bold().FontColor(White);
                header.Cell().Background("#546E7A").Padding(5).Text("Produit").FontSize(7).Bold().FontColor(White);
                header.Cell().Background("#546E7A").Padding(5).AlignRight().Text("Vol.(n-1)").FontSize(7).Bold().FontColor(White);
                header.Cell().Background("#546E7A").Padding(5).AlignRight().Text("Vol.(n)").FontSize(7).Bold().FontColor(White);
                header.Cell().Background("#546E7A").Padding(5).AlignRight().Text("Consomme").FontSize(7).Bold().FontColor(White);
                header.Cell().Background("#546E7A").Padding(5).AlignRight().Text("Vendu").FontSize(7).Bold().FontColor(White);
                header.Cell().Background("#546E7A").Padding(5).AlignRight().Text("Ecart").FontSize(7).Bold().FontColor(White);
                header.Cell().Background("#546E7A").Padding(5).AlignCenter().Text("Statut").FontSize(7).Bold().FontColor(White);
            });

            // Rows
            foreach (var comp in comparaisons)
            {
                table.Cell().Background(White).BorderBottom(1).BorderColor("#E0E0E0").Padding(5).Text(comp.ReservoirNumero).FontSize(8).Bold().FontColor(PrimaryBlue);
                table.Cell().Background(White).BorderBottom(1).BorderColor("#E0E0E0").Padding(5).Text(comp.ProduitNom ?? "-").FontSize(8);
                table.Cell().Background(White).BorderBottom(1).BorderColor("#E0E0E0").Padding(5).AlignRight().Text($"{comp.VolumePrecedent:N0}").FontSize(8).FontColor(Gray);
                table.Cell().Background(White).BorderBottom(1).BorderColor("#E0E0E0").Padding(5).AlignRight().Text($"{comp.VolumeActuel:N0}").FontSize(8).FontColor(Gray);
                table.Cell().Background(White).BorderBottom(1).BorderColor("#E0E0E0").Padding(5).AlignRight().Text($"{comp.StockConsomme:N0} L").FontSize(8).Bold().FontColor("#37474F");
                table.Cell().Background(White).BorderBottom(1).BorderColor("#E0E0E0").Padding(5).AlignRight().Text($"{comp.QuantiteVendue:N0} L").FontSize(8).FontColor(PrimaryBlue);
                table.Cell().Background(White).BorderBottom(1).BorderColor("#E0E0E0").Padding(5).AlignRight().Text($"{comp.Ecart:N0} L").FontSize(8).Bold().FontColor(comp.StatutColor);
                table.Cell().Background(White).BorderBottom(1).BorderColor("#E0E0E0").Padding(5).AlignCenter()
                    .Text(comp.Statut).FontSize(7).Bold().FontColor(comp.StatutColor);
            }
        });
    }

    private void ComposeStockReservoirTable(QContainer container, List<RapportStockReservoirPdfData> reservoirs)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(1.5f);
                columns.RelativeColumn(1.5f);
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();
            });

            // Header
            table.Header(header =>
            {
                header.Cell().Background(LightGray).Padding(6).Text("Reservoir").FontSize(8).Bold();
                header.Cell().Background(LightGray).Padding(6).Text("Produit").FontSize(8).Bold();
                header.Cell().Background(LightGray).Padding(6).AlignRight().Text("Capacite").FontSize(8).Bold();
                header.Cell().Background(LightGray).Padding(6).AlignRight().Text("Niveau").FontSize(8).Bold();
                header.Cell().Background(LightGray).Padding(6).AlignRight().Text("%").FontSize(8).Bold();
            });

            // Rows
            foreach (var res in reservoirs)
            {
                var pctColor = res.PourcentageRemplissage < 20 ? Red : res.PourcentageRemplissage < 50 ? Orange : Green;
                table.Cell().BorderBottom(1).BorderColor("#EEE").Padding(5).Text(res.ReservoirNumero).FontSize(9).Bold().FontColor(PrimaryBlue);
                table.Cell().BorderBottom(1).BorderColor("#EEE").Padding(5).Text(res.ProduitNom ?? "-").FontSize(9);
                table.Cell().BorderBottom(1).BorderColor("#EEE").Padding(5).AlignRight().Text($"{res.Capacite:N0} L").FontSize(9).FontColor(Gray);
                table.Cell().BorderBottom(1).BorderColor("#EEE").Padding(5).AlignRight().Text($"{res.NiveauActuel:N0} L").FontSize(9).Bold();
                table.Cell().BorderBottom(1).BorderColor("#EEE").Padding(5).AlignRight().Text($"{res.PourcentageRemplissage:N0}%").FontSize(9).FontColor(pctColor);
            }
        });
    }

    private void ComposeStockProduitsTable(QContainer container, List<RapportStockProduitPdfData> produits)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(2);
                columns.RelativeColumn(1.5f);
                columns.RelativeColumn();
                columns.RelativeColumn();
            });

            // Header
            table.Header(header =>
            {
                header.Cell().Background(LightGray).Padding(6).Text("Produit").FontSize(8).Bold();
                header.Cell().Background(LightGray).Padding(6).Text("Categorie").FontSize(8).Bold();
                header.Cell().Background(LightGray).Padding(6).AlignRight().Text("Stock").FontSize(8).Bold();
                header.Cell().Background(LightGray).Padding(6).AlignRight().Text("Min").FontSize(8).Bold();
            });

            // Rows
            foreach (var prod in produits)
            {
                var stockColor = prod.IsLowStock ? Red : "#333";
                table.Cell().BorderBottom(1).BorderColor("#EEE").Padding(5).Text(prod.ProduitNom).FontSize(9);
                table.Cell().BorderBottom(1).BorderColor("#EEE").Padding(5).Text(prod.CategorieNom ?? "-").FontSize(8).FontColor(Gray);
                table.Cell().BorderBottom(1).BorderColor("#EEE").Padding(5).AlignRight().Text(prod.StockActuel.ToString()).FontSize(9).Bold().FontColor(stockColor);
                table.Cell().BorderBottom(1).BorderColor("#EEE").Padding(5).AlignRight().Text(prod.StockMinimum?.ToString() ?? "-").FontSize(8).FontColor(Gray);
            }
        });
    }

    private void ComposeAchatsTable(QContainer container, List<RapportAchatProduitPdfData> achats)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(2);
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();
            });

            // Header
            table.Header(header =>
            {
                header.Cell().Background(LightGray).Padding(6).Text("Produit").FontSize(8).Bold();
                header.Cell().Background(LightGray).Padding(6).AlignRight().Text("Quantite").FontSize(8).Bold();
                header.Cell().Background(LightGray).Padding(6).AlignRight().Text("Montant").FontSize(8).Bold();
                header.Cell().Background(LightGray).Padding(6).AlignRight().Text("Achats").FontSize(8).Bold();
            });

            // Rows
            foreach (var achat in achats)
            {
                table.Cell().BorderBottom(1).BorderColor("#EEE").Padding(5).Text(achat.ProduitNom).FontSize(9);
                table.Cell().BorderBottom(1).BorderColor("#EEE").Padding(5).AlignRight().Text($"{achat.TotalQuantite:N0}").FontSize(9).FontColor(Orange);
                table.Cell().BorderBottom(1).BorderColor("#EEE").Padding(5).AlignRight().Text($"{achat.TotalMontant:N2}").FontSize(9).Bold();
                table.Cell().BorderBottom(1).BorderColor("#EEE").Padding(5).AlignRight().Text(achat.NombreAchats.ToString()).FontSize(9).FontColor(Gray);
            }

            // Footer total
            var totalMontant = achats.Sum(a => a.TotalMontant);
            table.Cell().Background("#FFF3E0").Padding(6).Text("TOTAL").FontSize(9).Bold().FontColor(Orange);
            table.Cell().Background("#FFF3E0").Padding(6);
            table.Cell().Background("#FFF3E0").Padding(6).AlignRight().Text($"{totalMontant:N2} MAD").FontSize(9).Bold().FontColor(Orange);
            table.Cell().Background("#FFF3E0").Padding(6);
        });
    }

    #endregion

    private void ComposeRapportFooter(QContainer container)
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
}
