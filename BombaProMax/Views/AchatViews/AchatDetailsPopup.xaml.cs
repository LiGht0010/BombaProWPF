using BombaProMax.Models;
using BombaProMax.Services;
using CommunityToolkit.Maui.Views;

namespace BombaProMax.Views.AchatViews;

public partial class AchatDetailsPopup : Popup
{
    private readonly AchatDto _achat;
    private readonly AchatAllocationService _allocationService;
    private readonly UserService _userService;
    private readonly List<AchatAllocationDto> _allocations = [];
    private AchatAllocationStatusDto? _allocationStatus;

    public AchatDetailsPopup(AchatDto achat)
    {
        InitializeComponent();
        CanBeDismissedByTappingOutsideOfPopup = false;

        _achat = achat;
        _allocationService = new AchatAllocationService();
        _userService = new UserService();

        // Load data asynchronously
        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            // Load allocation status
            _allocationStatus = await _allocationService.CheckAchatAllocationStatusAsync(_achat.ID);

            // Load allocations if it's a fuel product
            if (_allocationStatus?.EstCarburant == true)
            {
                var allocations = await _allocationService.GetByAchatIdAsync(_achat.ID);
                _allocations.Clear();
                _allocations.AddRange(allocations.Where(a => a.Statut != "Annulée"));
            }

            PopulateUI();
            await LoadAuditInfoAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading achat details: {ex.Message}");
            PopulateUI(); // Populate with basic info
        }
    }

    private async Task LoadAuditInfoAsync()
    {
        try
        {
            // Set dates synchronously
            CreatedAtLabel.Text = _achat.DateCreation?.ToString("dd/MM/yyyy HH:mm") ?? "N/A";
            ModifiedAtLabel.Text = _achat.DateModification?.ToString("dd/MM/yyyy HH:mm") ?? "N/A";

            // Load user names asynchronously
            var createdByName = await _userService.GetUserNameByIdAsync(_achat.AjoutePar);
            CreatedByLabel.Text = createdByName;

            var modifiedByName = await _userService.GetUserNameByIdAsync(_achat.ModifiePar);
            ModifiedByLabel.Text = modifiedByName;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AchatDetailsPopup] Error loading audit info: {ex.Message}");
            CreatedByLabel.Text = "Erreur de chargement";
            ModifiedByLabel.Text = "Erreur de chargement";
        }
    }

    private void PopulateUI()
    {
        // Header info
        HeaderLabel.Text = "Détails de l'Achat";
        NumeroLabel.Text = _achat.Numero ?? "N/A";
        DateLabel.Text = _achat.Date.ToString("dd/MM/yyyy");
        FournisseurHeaderLabel.Text = $"Fournisseur: {_achat.FournisseurNom ?? "N/A"}";

        // Purchase info
        FournisseurLabel.Text = _achat.FournisseurNom ?? "N/A";
        ProduitLabel.Text = _achat.ProduitNom ?? "N/A";
        QuantiteLabel.Text = $"{_achat.Quantite ?? 0:N0} L";
        DateInfoLabel.Text = _achat.Date.ToString("dd/MM/yyyy");

        // Financial summary
        PrixUnitaireLabel.Text = $"{_achat.PrixAchatUnitaire ?? 0:N2} DH/L";
        QuantiteValueLabel.Text = $"{_achat.Quantite ?? 0:N0} L";
        CoutTotalLabel.Text = $"{_achat.Cout ?? 0:N2} DH";

        // Transport info
        ChauffeurLabel.Text = _achat.ChauffeurNom ?? "Non assigné";
        CamionLabel.Text = _achat.CamionImmatriculation ?? "Non assigné";

        // Delivery status
        if (_achat.LivraisonDefectueuse == true)
        {
            DeliveryStatusIcon.Text = "??";
            DeliveryStatusIcon.TextColor = Color.FromArgb("#F44336");
            DeliveryStatusLabel.Text = "Livraison Défectueuse";
            DeliveryStatusLabel.TextColor = Color.FromArgb("#F44336");
            DeliveryStatusSubLabel.Text = "Problèmes signalés lors de la réception";
            DeliveryStatusCard.BackgroundColor = Color.FromArgb("#FFEBEE");
        }
        else
        {
            DeliveryStatusIcon.Text = "?";
            DeliveryStatusIcon.TextColor = Color.FromArgb("#2E7D32");
            DeliveryStatusLabel.Text = "Livraison OK";
            DeliveryStatusLabel.TextColor = Color.FromArgb("#2E7D32");
            DeliveryStatusSubLabel.Text = "Aucun problème signalé";
            DeliveryStatusCard.BackgroundColor = Colors.White;
        }

        // Allocation status
        PopulateAllocationStatus();

        // Build allocation rows if applicable
        if (_allocationStatus?.EstCarburant == true && _allocations.Count > 0)
        {
            AllocationsTableSection.IsVisible = true;
            NonCarburantSection.IsVisible = false;
            BuildAllocationRows();
        }
        else if (_allocationStatus?.EstCarburant == false)
        {
            AllocationsTableSection.IsVisible = false;
            NonCarburantSection.IsVisible = true;
        }
        else
        {
            AllocationsTableSection.IsVisible = false;
            NonCarburantSection.IsVisible = false;
        }
    }

    private void PopulateAllocationStatus()
    {
        var quantite = _achat.Quantite ?? 0;
        var totalAlloue = _allocationStatus?.TotalAlloue ?? 0;
        var restant = _allocationStatus?.Restant ?? quantite;

        AllocationTotalLabel.Text = $"{quantite:N0} L";
        AllocationAlloueeLabel.Text = $"{totalAlloue:N0} L";
        AllocationRestanteLabel.Text = $"{restant:N0} L";

        // Style restante based on value
        if (restant <= 0)
        {
            AllocationRestanteLabel.TextColor = Color.FromArgb("#2E7D32");
        }
        else
        {
            AllocationRestanteLabel.TextColor = Color.FromArgb("#E65100");
        }

        // Status badge
        if (_allocationStatus?.EstCarburant == false)
        {
            AllocationStatusBadge.BackgroundColor = Color.FromArgb("#E3F2FD");
            AllocationStatusLabel.Text = "Non-Carburant";
            AllocationStatusLabel.TextColor = Color.FromArgb("#1976D2");
        }
        else if (_allocationStatus?.EstCompletementAlloue == true)
        {
            AllocationStatusBadge.BackgroundColor = Color.FromArgb("#E8F5E9");
            AllocationStatusLabel.Text = "? Entièrement alloué";
            AllocationStatusLabel.TextColor = Color.FromArgb("#2E7D32");
        }
        else if (totalAlloue > 0)
        {
            AllocationStatusBadge.BackgroundColor = Color.FromArgb("#FFF3E0");
            AllocationStatusLabel.Text = "Partiellement alloué";
            AllocationStatusLabel.TextColor = Color.FromArgb("#E65100");
        }
        else
        {
            AllocationStatusBadge.BackgroundColor = Color.FromArgb("#FFEBEE");
            AllocationStatusLabel.Text = "Non alloué";
            AllocationStatusLabel.TextColor = Color.FromArgb("#C62828");
        }
    }

    private void BuildAllocationRows()
    {
        AllocationsContainer.Children.Clear();

        var isAlternate = false;
        decimal totalAlloue = 0;

        foreach (var allocation in _allocations)
        {
            var bgColor = isAlternate ? Color.FromArgb("#FAFAFA") : Colors.White;
            isAlternate = !isAlternate;

            totalAlloue += allocation.QuantiteAllouee;

            var row = new Grid
            {
                ColumnDefinitions =
                [
                    new ColumnDefinition { Width = new GridLength(1.5, GridUnitType.Star) },
                    new ColumnDefinition { Width = new GridLength(1.5, GridUnitType.Star) },
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
                ],
                ColumnSpacing = 10,
                Padding = new Thickness(20, 10),
                BackgroundColor = bgColor
            };

            // Reservoir
            row.Add(new Label
            {
                Text = allocation.ReservoirNumero ?? "N/A",
                FontSize = 12,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#333"),
                VerticalOptions = LayoutOptions.Center
            }, 0, 0);

            // Produit
            row.Add(new Label
            {
                Text = allocation.ProduitNom ?? "N/A",
                FontSize = 11,
                TextColor = Color.FromArgb("#666"),
                VerticalOptions = LayoutOptions.Center
            }, 1, 0);

            // Quantite
            row.Add(new Label
            {
                Text = $"{allocation.QuantiteAllouee:N0} L",
                FontSize = 12,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#E8A84C"),
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalOptions = LayoutOptions.Center
            }, 2, 0);

            // Date
            row.Add(new Label
            {
                Text = allocation.DateAllocation.ToString("dd/MM/yyyy"),
                FontSize = 11,
                TextColor = Color.FromArgb("#666"),
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalOptions = LayoutOptions.Center
            }, 3, 0);

            // Statut badge
            var statutColor = allocation.Statut switch
            {
                "Confirmée" => "#2E7D32",
                "En Attente" => "#E65100",
                "Annulée" => "#C62828",
                _ => "#666"
            };
            var statutBgColor = allocation.Statut switch
            {
                "Confirmée" => "#E8F5E9",
                "En Attente" => "#FFF3E0",
                "Annulée" => "#FFEBEE",
                _ => "#F5F5F5"
            };

            var statutBadge = new Border
            {
                BackgroundColor = Color.FromArgb(statutBgColor),
                Padding = new Thickness(8, 4),
                StrokeThickness = 0,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };
            statutBadge.StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 8 };
            statutBadge.Content = new Label
            {
                Text = allocation.Statut ?? "N/A",
                FontSize = 10,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb(statutColor)
            };

            row.Add(statutBadge, 4, 0);

            AllocationsContainer.Add(row);
        }

        // Update footer
        TotalAllocationsFooter.Text = $"{totalAlloue:N0} L";

        if (_allocations.Count == 0)
        {
            AllocationsContainer.Add(new Label
            {
                Text = "Aucune allocation",
                TextColor = Color.FromArgb("#999"),
                FontSize = 12,
                Margin = new Thickness(20, 10)
            });
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

            // Build PDF data
            var pdfData = BuildPdfData();

            // Generate PDF
            var pdfService = new PdfGeneratorService();
            var filePath = await pdfService.GenerateAchatReportAsync(pdfData);

            PdfButton.Text = "? Généré!";

            // Show success and offer to open
            var openFile = await Application.Current!.MainPage!.DisplayAlert(
                "PDF Généré",
                $"Le bon d'achat a été enregistré:\n{filePath}",
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

    private AchatPdfData BuildPdfData()
    {
        var pdfData = new AchatPdfData
        {
            AchatID = _achat.ID,
            Numero = _achat.Numero ?? "",
            Date = _achat.Date,
            FournisseurNom = _achat.FournisseurNom,
            ProduitNom = _achat.ProduitNom,
            Quantite = _achat.Quantite ?? 0,
            PrixUnitaire = _achat.PrixAchatUnitaire ?? 0,
            CoutTotal = _achat.Cout ?? 0,
            ChauffeurNom = _achat.ChauffeurNom,
            CamionImmatriculation = _achat.CamionImmatriculation,
            LivraisonDefectueuse = _achat.LivraisonDefectueuse ?? false,
            EstCarburant = _allocationStatus?.EstCarburant ?? false,
            TotalAlloue = _allocationStatus?.TotalAlloue ?? 0,
            QuantiteRestante = _allocationStatus?.Restant ?? (_achat.Quantite ?? 0),
            EstCompletementAlloue = _allocationStatus?.EstCompletementAlloue ?? false
        };

        // Convert allocations
        pdfData.Allocations = _allocations.Select(a => new AchatAllocationPdfData
        {
            ReservoirNumero = a.ReservoirNumero ?? "",
            ProduitNom = a.ProduitNom,
            QuantiteAllouee = a.QuantiteAllouee,
            DateAllocation = a.DateAllocation,
            Statut = a.Statut ?? ""
        }).ToList();

        return pdfData;
    }
}