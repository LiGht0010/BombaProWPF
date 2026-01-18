using BombaProMax.Models;
using BombaProMax.Services;
using CommunityToolkit.Maui.Views;

namespace BombaProMax.Views.OnboardingViews;

public partial class OpeningBalancePopup : Popup
{
    private readonly OpeningBalanceOnboardingService _onboardingService;
    private ReservoirStockStatusDto? _currentReservoir;
    private bool _isProcessing;
    
    public bool CompletedSuccessfully { get; private set; }

    public OpeningBalancePopup(OpeningBalanceOnboardingService onboardingService)
    {
        InitializeComponent();
        _onboardingService = onboardingService;
        
        _onboardingService.OnboardingCompleted += OnOnboardingCompleted;
        
        LoadCurrentReservoir();
    }

    private void LoadCurrentReservoir()
    {
        _currentReservoir = _onboardingService.GetCurrentReservoir();
        
        if (_currentReservoir == null)
        {
            CompletedSuccessfully = true;
            Close(true);
            return;
        }

        ProgressLabel.Text = $"{_onboardingService.CurrentProgress} / {_onboardingService.TotalCount}";

        ReservoirNumeroLabel.Text = $"Reservoir {_currentReservoir.ReservoirNumero}";
        ProduitNomLabel.Text = _currentReservoir.ProduitNom ?? "Produit non assigne";
        CapaciteLabel.Text = $"Capacite: {_currentReservoir.Capacite:N0} L";

        QuantiteEntry.Text = string.Empty;
        PrixAchatEntry.Text = string.Empty;
        ErrorLabel.IsVisible = false;
        
        UpdateFillProgress(0);

        if (_onboardingService.RemainingCount == 1)
        {
            ConfirmButton.Text = "Terminer";
            SkipButton.Text = "Passer et terminer";
            SkipAllButton.IsVisible = false;
        }
        else
        {
            ConfirmButton.Text = "Confirmer";
            SkipButton.Text = "Passer";
            SkipAllButton.IsVisible = true;
        }
    }

    private void OnQuantiteChanged(object? sender, TextChangedEventArgs e)
    {
        ErrorLabel.IsVisible = false;
        
        if (decimal.TryParse(QuantiteEntry.Text, out var quantite) && _currentReservoir != null)
        {
            if (quantite > _currentReservoir.Capacite)
            {
                QuantiteHintLabel.Text = $"Attention: Depasse la capacite de {_currentReservoir.Capacite:N0} L";
                QuantiteHintLabel.TextColor = Color.FromArgb("#C62828");
            }
            else if (quantite < 0)
            {
                QuantiteHintLabel.Text = "Attention: La quantite ne peut pas etre negative";
                QuantiteHintLabel.TextColor = Color.FromArgb("#C62828");
            }
            else
            {
                QuantiteHintLabel.Text = "Saisissez le niveau actuel de carburant dans le reservoir";
                QuantiteHintLabel.TextColor = Color.FromArgb("#8B939E");
            }

            var percentage = _currentReservoir.Capacite > 0 
                ? (double)(quantite / _currentReservoir.Capacite) * 100 
                : 0;
            UpdateFillProgress(Math.Min(100, Math.Max(0, percentage)));
        }
        else
        {
            QuantiteHintLabel.Text = "Saisissez le niveau actuel de carburant dans le reservoir";
            QuantiteHintLabel.TextColor = Color.FromArgb("#8B939E");
            UpdateFillProgress(0);
        }
    }

    private void UpdateFillProgress(double percentage)
    {
        var maxWidth = 450.0;
        var fillWidth = maxWidth * (percentage / 100.0);
        
        FillProgressBar.WidthRequest = fillWidth;
        FillPercentLabel.Text = $"{percentage:F0}% de la capacite";
        
        FillProgressBar.Color = percentage switch
        {
            <= 20 => Color.FromArgb("#C62828"),
            <= 40 => Color.FromArgb("#E65100"),
            <= 60 => Color.FromArgb("#E8A84C"),
            _ => Color.FromArgb("#2E7D32")
        };
    }

    private async void OnConfirmClicked(object? sender, EventArgs e)
    {
        if (_isProcessing || _currentReservoir == null)
            return;

        if (string.IsNullOrWhiteSpace(QuantiteEntry.Text))
        {
            ShowError("Veuillez saisir la quantite actuelle");
            return;
        }

        if (!decimal.TryParse(QuantiteEntry.Text, out var quantite) || quantite <= 0)
        {
            ShowError("Veuillez saisir une quantite valide superieure a 0");
            return;
        }

        if (quantite > _currentReservoir.Capacite)
        {
            ShowError($"La quantite ({quantite:N0} L) depasse la capacite ({_currentReservoir.Capacite:N0} L)");
            return;
        }

        decimal prixAchat = 0;
        if (!string.IsNullOrWhiteSpace(PrixAchatEntry.Text))
        {
            if (!decimal.TryParse(PrixAchatEntry.Text, out prixAchat) || prixAchat < 0)
            {
                ShowError("Veuillez saisir un prix valide ou laisser le champ vide");
                return;
            }
        }

        _isProcessing = true;
        ConfirmButton.IsEnabled = false;
        SkipButton.IsEnabled = false;
        SkipAllButton.IsEnabled = false;
        ConfirmButton.Text = "Enregistrement...";

        try
        {
            var result = await _onboardingService.CreateOpeningBalanceAsync(
                quantite, 
                prixAchat, 
                $"Stock initial configure lors de l'installation - {DateTime.Now:dd/MM/yyyy}");

            if (result.Success)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[OpeningBalancePopup] Successfully created opening balance: {quantite}L for {_currentReservoir.ReservoirNumero}");
                
                if (_onboardingService.HasMoreReservoirs)
                {
                    LoadCurrentReservoir();
                }
            }
            else
            {
                ShowError(result.Message);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[OpeningBalancePopup] Error: {ex.Message}");
            ShowError($"Erreur: {ex.Message}");
        }
        finally
        {
            _isProcessing = false;
            ConfirmButton.IsEnabled = true;
            SkipButton.IsEnabled = true;
            SkipAllButton.IsEnabled = true;
            
            if (_onboardingService.RemainingCount <= 1)
            {
                ConfirmButton.Text = "Terminer";
            }
            else
            {
                ConfirmButton.Text = "Confirmer";
            }
        }
    }

    private void OnSkipClicked(object? sender, EventArgs e)
    {
        if (_isProcessing)
            return;

        System.Diagnostics.Debug.WriteLine(
            $"[OpeningBalancePopup] Skipping reservoir {_currentReservoir?.ReservoirNumero}");
        
        _onboardingService.SkipCurrentReservoir();
        
        if (_onboardingService.HasMoreReservoirs)
        {
            LoadCurrentReservoir();
        }
    }

    private async void OnSkipAllClicked(object? sender, EventArgs e)
    {
        if (_isProcessing)
            return;

        var remaining = _onboardingService.RemainingCount;
        var confirm = await Application.Current!.MainPage!.DisplayAlert(
            "Passer la configuration",
            $"Voulez-vous vraiment passer la configuration des {remaining} reservoir(s) restant(s)?\n\n" +
            "Vous pourrez configurer le stock initial plus tard via le menu Reservoirs.",
            "Oui, passer tout",
            "Non, continuer");

        if (confirm)
        {
            System.Diagnostics.Debug.WriteLine(
                $"[OpeningBalancePopup] Skipping all {remaining} remaining reservoirs");
            
            _onboardingService.SkipAll();
        }
    }

    private void OnOnboardingCompleted(object? sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            CompletedSuccessfully = true;
            _onboardingService.OnboardingCompleted -= OnOnboardingCompleted;
            Close(true);
        });
    }

    private void ShowError(string message)
    {
        ErrorLabel.Text = message;
        ErrorLabel.IsVisible = true;
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();
        
        if (Handler == null)
        {
            _onboardingService.OnboardingCompleted -= OnOnboardingCompleted;
        }
    }
}
