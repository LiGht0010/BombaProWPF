using BombaProMax.Models;
using BombaProMax.Services;
using CommunityToolkit.Maui.Views;

namespace BombaProMax.Views.SettingsViews;

public partial class StationInfoEditPopup : Popup
{
    private readonly StationInfoService _stationInfoService;
    private readonly StationInfoDto? _existingStationInfo;
    private readonly bool _isEditMode;

    public StationInfoEditPopup(StationInfoDto? existingStationInfo = null)
    {
        InitializeComponent();
        CanBeDismissedByTappingOutsideOfPopup = false;

        _stationInfoService = new StationInfoService();
        _existingStationInfo = existingStationInfo;
        _isEditMode = existingStationInfo?.ID > 0;

        if (_isEditMode)
        {
            TitleLabel.Text = "Modifier les Informations";
            SubtitleLabel.Text = "Mettre a jour les informations de la station";
            SaveButton.Text = "Mettre a jour";
        }
        else
        {
            TitleLabel.Text = "Nouvelle Configuration";
            SubtitleLabel.Text = "Configurer les informations de votre station";
            SaveButton.Text = "Creer";
        }

        if (_existingStationInfo != null)
        {
            PopulateFields(_existingStationInfo);
        }
    }

    private void PopulateFields(StationInfoDto stationInfo)
    {
        StationNameEntry.Text = stationInfo.StationName;
        AdresseEntry.Text = stationInfo.Adresse;
        VilleEntry.Text = stationInfo.Ville;
        ICEEntry.Text = stationInfo.ICE;
        IFEntry.Text = stationInfo.IF;
        RCEntry.Text = stationInfo.RC;
        TPEntry.Text = stationInfo.TP;
        CNSSEntry.Text = stationInfo.CNSS;
        TelEntry.Text = stationInfo.Tel;
        FaxEntry.Text = stationInfo.Fax;
        EmailEntry.Text = stationInfo.Email;
        SiteWebEntry.Text = stationInfo.SiteWeb;
    }

    private void OnCancelClicked(object? sender, EventArgs e)
    {
        Close(null);
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(StationNameEntry.Text))
            {
                ShowError("Le nom de la station est obligatoire");
                return;
            }

            var stationInfo = new StationInfoDto
            {
                ID = _existingStationInfo?.ID ?? 0,
                StationName = StationNameEntry.Text.Trim(),
                Adresse = string.IsNullOrWhiteSpace(AdresseEntry.Text) ? null : AdresseEntry.Text.Trim(),
                Ville = string.IsNullOrWhiteSpace(VilleEntry.Text) ? null : VilleEntry.Text.Trim(),
                ICE = string.IsNullOrWhiteSpace(ICEEntry.Text) ? null : ICEEntry.Text.Trim(),
                IF = string.IsNullOrWhiteSpace(IFEntry.Text) ? null : IFEntry.Text.Trim(),
                RC = string.IsNullOrWhiteSpace(RCEntry.Text) ? null : RCEntry.Text.Trim(),
                TP = string.IsNullOrWhiteSpace(TPEntry.Text) ? null : TPEntry.Text.Trim(),
                CNSS = string.IsNullOrWhiteSpace(CNSSEntry.Text) ? null : CNSSEntry.Text.Trim(),
                Tel = string.IsNullOrWhiteSpace(TelEntry.Text) ? null : TelEntry.Text.Trim(),
                Fax = string.IsNullOrWhiteSpace(FaxEntry.Text) ? null : FaxEntry.Text.Trim(),
                Email = string.IsNullOrWhiteSpace(EmailEntry.Text) ? null : EmailEntry.Text.Trim(),
                SiteWeb = string.IsNullOrWhiteSpace(SiteWebEntry.Text) ? null : SiteWebEntry.Text.Trim(),
                LogoBase64 = _existingStationInfo?.LogoBase64
            };

            SaveButton.IsEnabled = false;
            SaveButton.Text = "Enregistrement...";

            var result = await _stationInfoService.SaveStationInfoAsync(stationInfo);

            if (result != null)
            {
                Close(result);
            }
            else
            {
                ShowError("Erreur lors de l enregistrement. Veuillez reessayer.");
                SaveButton.IsEnabled = true;
                SaveButton.Text = _isEditMode ? "Mettre a jour" : "Creer";
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving station info: {ex.Message}");
            ShowError($"Erreur: {ex.Message}");
            SaveButton.IsEnabled = true;
            SaveButton.Text = _isEditMode ? "Mettre a jour" : "Creer";
        }
    }

    private void ShowError(string message)
    {
        ErrorLabel.Text = message;
        ErrorLabel.IsVisible = true;
    }
}
