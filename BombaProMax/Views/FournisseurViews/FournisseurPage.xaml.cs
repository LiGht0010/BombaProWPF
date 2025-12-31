using BombaProMax.Models;
using BombaProMax.Services;
using BombaProMax.ViewModels;
using BombaProMax.Views.ChauffeurViews;
using BombaProMax.Views.CamionViews;
using BombaProMax.Views.CiterneViews;
using CommunityToolkit.Maui.Views;

namespace BombaProMax.Views.FournisseurViews;

public partial class FournisseurPage : ContentPage
{
    private readonly FournisseurViewModel _fournisseurViewModel;
    private readonly ChauffeurViewModel _chauffeurViewModel;
    private readonly CamionViewModel _camionViewModel;
    private readonly CiterneViewModel _citerneViewModel;
    
    // Services for popups
    private readonly ChauffeurService _chauffeurService;
    private readonly CamionService _camionService;
    private readonly CiterneService _citerneService;
    private readonly FournisseurService _fournisseurService;

    public FournisseurPage(
        FournisseurViewModel fournisseurViewModel,
        ChauffeurViewModel chauffeurViewModel,
        CamionViewModel camionViewModel,
        CiterneViewModel citerneViewModel,
        ChauffeurService chauffeurService,
        CamionService camionService,
        CiterneService citerneService,
        FournisseurService fournisseurService)
    {
        InitializeComponent();
        
        _fournisseurViewModel = fournisseurViewModel;
        _chauffeurViewModel = chauffeurViewModel;
        _camionViewModel = camionViewModel;
        _citerneViewModel = citerneViewModel;
        
        _chauffeurService = chauffeurService;
        _camionService = camionService;
        _citerneService = citerneService;
        _fournisseurService = fournisseurService;
        
        BindingContext = _fournisseurViewModel;
        
        // Bind collection views to their respective ViewModels
        ChauffeursCollectionView.ItemsSource = _chauffeurViewModel.Chauffeurs;
        CamionsCollectionView.ItemsSource = _camionViewModel.Camions;
        CiternesCollectionView.ItemsSource = _citerneViewModel.Citernes;
        
        // Subscribe to collection changes for count updates
        _chauffeurViewModel.Chauffeurs.CollectionChanged += (s, e) => UpdateChauffeurCount();
        _camionViewModel.Camions.CollectionChanged += (s, e) => UpdateCamionCount();
        _citerneViewModel.Citernes.CollectionChanged += (s, e) => UpdateCiterneCount();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _fournisseurViewModel.LoadFournisseursCommand.Execute(null);
    }

    #region Tab Switching

    private void OnFournisseursTabClicked(object sender, EventArgs e)
    {
        SetActiveTab(0);
        _fournisseurViewModel.LoadFournisseursCommand.Execute(null);
    }

    private void OnChauffeursTabClicked(object sender, EventArgs e)
    {
        SetActiveTab(1);
        LoadChauffeurs();
    }

    private void OnCamionsTabClicked(object sender, EventArgs e)
    {
        SetActiveTab(2);
        LoadCamions();
    }

    private void OnCiternesTabClicked(object sender, EventArgs e)
    {
        SetActiveTab(3);
        LoadCiternes();
    }

    private void SetActiveTab(int tabIndex)
    {
        // Hide all tabs
        FournisseursTab.IsVisible = false;
        ChauffeursTab.IsVisible = false;
        CamionsTab.IsVisible = false;
        CiternesTab.IsVisible = false;

        // Reset all button styles to inactive
        SetTabButtonInactive(FournisseursTabButton);
        SetTabButtonInactive(ChauffeursTabButton);
        SetTabButtonInactive(CamionsTabButton);
        SetTabButtonInactive(CiternesTabButton);

        // Show selected tab and set active style
        switch (tabIndex)
        {
            case 0:
                FournisseursTab.IsVisible = true;
                SetTabButtonActive(FournisseursTabButton);
                break;
            case 1:
                ChauffeursTab.IsVisible = true;
                SetTabButtonActive(ChauffeursTabButton);
                break;
            case 2:
                CamionsTab.IsVisible = true;
                SetTabButtonActive(CamionsTabButton);
                break;
            case 3:
                CiternesTab.IsVisible = true;
                SetTabButtonActive(CiternesTabButton);
                break;
        }
    }

    private static void SetTabButtonActive(Button button)
    {
        button.BackgroundColor = Color.FromArgb("#FFFFFF");
        button.TextColor = Color.FromArgb("#4A8FBF");
        button.FontAttributes = FontAttributes.Bold;
        button.Opacity = 1;
    }

    private static void SetTabButtonInactive(Button button)
    {
        button.BackgroundColor = Colors.Transparent;
        button.TextColor = Color.FromArgb("#FFFFFF");
        button.FontAttributes = FontAttributes.None;
        button.Opacity = 0.85;
    }

    #endregion

    #region Data Loading

    private async void LoadChauffeurs()
    {
        ChauffeurLoadingIndicator.IsRunning = true;
        ChauffeurLoadingIndicator.IsVisible = true;
        
        _chauffeurViewModel.LoadChauffeursCommand.Execute(null);
        await Task.Delay(100); // Give time for async load
        
        ChauffeurLoadingIndicator.IsRunning = false;
        ChauffeurLoadingIndicator.IsVisible = false;
        UpdateChauffeurCount();
    }

    private async void LoadCamions()
    {
        CamionLoadingIndicator.IsRunning = true;
        CamionLoadingIndicator.IsVisible = true;
        
        _camionViewModel.LoadCamionsCommand.Execute(null);
        await Task.Delay(100);
        
        CamionLoadingIndicator.IsRunning = false;
        CamionLoadingIndicator.IsVisible = false;
        UpdateCamionCount();
    }

    private async void LoadCiternes()
    {
        CiterneLoadingIndicator.IsRunning = true;
        CiterneLoadingIndicator.IsVisible = true;
        
        _citerneViewModel.LoadCiternesCommand.Execute(null);
        await Task.Delay(100);
        
        CiterneLoadingIndicator.IsRunning = false;
        CiterneLoadingIndicator.IsVisible = false;
        UpdateCiterneCount();
    }

    private void UpdateChauffeurCount()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            ChauffeurCountLabel.Text = _chauffeurViewModel.Chauffeurs.Count.ToString();
        });
    }

    private void UpdateCamionCount()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            CamionCountLabel.Text = _camionViewModel.Camions.Count.ToString();
        });
    }

    private void UpdateCiterneCount()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            CiterneCountLabel.Text = _citerneViewModel.Citernes.Count.ToString();
        });
    }

    #endregion

    #region Chauffeur Actions

    private async void OnAddChauffeurClicked(object sender, EventArgs e)
    {
        var popup = new ChauffeurCreatePopup(_chauffeurService, _fournisseurService);
        var result = await this.ShowPopupAsync(popup);
        
        // Refresh only if a chauffeur was created
        if (result is ChauffeurDto)
        {
            LoadChauffeurs();
        }
    }

    private async void OnEditChauffeurClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is ChauffeurDto chauffeur)
        {
            var popup = new ChauffeurEditPopup(chauffeur);
            var result = await this.ShowPopupAsync(popup);
            
            // Refresh if edit was successful (returns true or the updated chauffeur)
            if (result is true or ChauffeurDto)
            {
                LoadChauffeurs();
            }
        }
    }

    private async void OnDeleteChauffeurClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is ChauffeurDto chauffeur)
        {
            bool confirm = await DisplayAlert("Confirmation", 
                $"Voulez-vous vraiment supprimer le chauffeur {chauffeur.Nom} {chauffeur.Prenom}?", 
                "Oui", "Non");
            
            if (confirm)
            {
                var success = await _chauffeurService.DeleteChauffeurAsync(chauffeur.ID);
                if (success)
                {
                    await DisplayAlert("Succès", "Chauffeur supprimé avec succès", "OK");
                    LoadChauffeurs();
                }
                else
                {
                    await DisplayAlert("Erreur", "Impossible de supprimer le chauffeur", "OK");
                }
            }
        }
    }

    #endregion

    #region Camion Actions

    private async void OnAddCamionClicked(object sender, EventArgs e)
    {
        var popup = new CamionCreatePopup(_camionService, _fournisseurService, _citerneService);
        var result = await this.ShowPopupAsync(popup);
        
        // Refresh only if a camion was created
        if (result is CamionDto)
        {
            LoadCamions();
        }
    }

    private async void OnCamionDetailsClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is CamionDto camion)
        {
            var popup = new CamionDetailsPopup(camion);
            await this.ShowPopupAsync(popup);
        }
    }

    private async void OnEditCamionClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is CamionDto camion)
        {
            var popup = new CamionEditPopup(_camionService, _fournisseurService, _citerneService, camion);
            var result = await this.ShowPopupAsync(popup);
            
            // Refresh if edit was successful
            if (result is true or CamionDto)
            {
                LoadCamions();
            }
        }
    }

    private async void OnDeleteCamionClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is CamionDto camion)
        {
            bool confirm = await DisplayAlert("Confirmation", 
                $"Voulez-vous vraiment supprimer le camion {camion.Matricule}?", 
                "Oui", "Non");
            
            if (confirm)
            {
                var success = await _camionService.DeleteCamionAsync(camion.ID);
                if (success)
                {
                    await DisplayAlert("Succès", "Camion supprimé avec succès", "OK");
                    LoadCamions();
                }
                else
                {
                    await DisplayAlert("Erreur", "Impossible de supprimer le camion", "OK");
                }
            }
        }
    }

    #endregion

    #region Citerne Actions

    private async void OnAddCiterneClicked(object sender, EventArgs e)
    {
        var popup = new CiterneCreatePopup(_citerneService, _fournisseurService);
        var result = await this.ShowPopupAsync(popup);
        
        // Refresh only if a citerne was created
        if (result is CiterneDto)
        {
            LoadCiternes();
        }
    }

    private async void OnCiterneDetailsClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is CiterneDto citerne)
        {
            _citerneViewModel.ShowDetailsCommand.Execute(citerne);
        }
    }

    private async void OnEditCiterneClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is CiterneDto citerne)
        {
            var popup = new CiterneEditPopup(_citerneService, _fournisseurService, citerne);
            var result = await this.ShowPopupAsync(popup);
            
            // Refresh if edit was successful
            if (result is true or CiterneDto)
            {
                LoadCiternes();
            }
        }
    }

    private async void OnDeleteCiterneClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is CiterneDto citerne)
        {
            bool confirm = await DisplayAlert("Confirmation", 
                $"Voulez-vous vraiment supprimer la citerne {citerne.MatriculeCiterne}?", 
                "Oui", "Non");
            
            if (confirm)
            {
                var success = await _citerneService.DeleteCiterneAsync(citerne.ID);
                if (success)
                {
                    await DisplayAlert("Succès", "Citerne supprimée avec succès", "OK");
                    LoadCiternes();
                }
                else
                {
                    await DisplayAlert("Erreur", "Impossible de supprimer la citerne", "OK");
                }
            }
        }
    }

    #endregion
}