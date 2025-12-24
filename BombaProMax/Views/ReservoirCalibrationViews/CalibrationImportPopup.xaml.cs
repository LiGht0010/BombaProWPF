using BombaProMax.Models;
using CommunityToolkit.Maui.Views;

namespace BombaProMax.Views.ReservoirCalibrationViews;

public partial class CalibrationImportPopup : Popup
{
    private readonly ReservoirDto _reservoir;

    public CalibrationImportPopup(ReservoirDto reservoir)
    {
        InitializeComponent();
        _reservoir = reservoir;
        
        // Set reservoir info
        ReservoirInfoLabel.Text = $"Réservoir: {reservoir.Numero}";
        ReservoirNumeroLabel.Text = reservoir.Numero;
        ReservoirCapaciteLabel.Text = $"{reservoir.Capacite:N0} L";
        ReservoirProduitLabel.Text = reservoir.ProduitNom ?? "Non assigné";

        // Track line count as user types
        CsvEditor.TextChanged += OnCsvTextChanged;
    }

    private void OnCsvTextChanged(object? sender, TextChangedEventArgs e)
    {
        var lineCount = string.IsNullOrWhiteSpace(CsvEditor.Text) 
            ? 0 
            : CsvEditor.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Length;
        
        // Subtract 1 for header line if present
        var dataLines = lineCount > 0 && 
            (CsvEditor.Text.StartsWith("Hauteur", StringComparison.OrdinalIgnoreCase) ||
             CsvEditor.Text.StartsWith("Height", StringComparison.OrdinalIgnoreCase))
            ? lineCount - 1
            : lineCount;

        LineCountLabel.Text = $"{dataLines} ligne(s) de données détectée(s)";
        LineCountLabel.TextColor = dataLines > 0 
            ? Color.FromArgb("#2E7D32")  // Green
            : Color.FromArgb("#8B939E"); // Muted
    }

    private async void OnPickFileClicked(object sender, EventArgs e)
    {
        try
        {
            var customFileType = new FilePickerFileType(
                new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.iOS, new[] { "public.comma-separated-values-text", "public.plain-text" } },
                    { DevicePlatform.Android, new[] { "text/csv", "text/plain", "text/comma-separated-values" } },
                    { DevicePlatform.WinUI, new[] { ".csv", ".txt" } },
                    { DevicePlatform.MacCatalyst, new[] { "public.comma-separated-values-text", "public.plain-text" } }
                });

            var options = new PickOptions
            {
                PickerTitle = "Sélectionner un fichier CSV de calibration",
                FileTypes = customFileType
            };

            var result = await FilePicker.Default.PickAsync(options);

            if (result != null)
            {
                using var stream = await result.OpenReadAsync();
                using var reader = new StreamReader(stream);
                var content = await reader.ReadToEndAsync();
                
                CsvEditor.Text = content;
            }
        }
        catch (Exception ex)
        {
            await Application.Current!.MainPage!.DisplayAlert(
                "Erreur", 
                $"Impossible de lire le fichier: {ex.Message}", 
                "OK");
        }
    }

    private void OnCancelClicked(object sender, EventArgs e)
    {
        Close(null);
    }

    private async void OnImportClicked(object sender, EventArgs e)
    {
        var csvContent = CsvEditor.Text?.Trim();

        if (string.IsNullOrWhiteSpace(csvContent))
        {
            await Application.Current!.MainPage!.DisplayAlert(
                "Attention", 
                "Veuillez entrer ou coller des données CSV", 
                "OK");
            return;
        }

        // Basic validation - check for at least some numeric data
        var lines = csvContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var dataLines = lines.Where(l => 
            !l.StartsWith("Hauteur", StringComparison.OrdinalIgnoreCase) &&
            !l.StartsWith("Height", StringComparison.OrdinalIgnoreCase) &&
            l.Contains(',') || l.Contains(';')).ToList();

        if (dataLines.Count == 0)
        {
            await Application.Current!.MainPage!.DisplayAlert(
                "Format invalide", 
                "Aucune donnée valide trouvée.\n\nFormat attendu:\nHauteurCm,VolumeLitres\n1,13\n2,38\n...", 
                "OK");
            return;
        }

        // Confirm import
        var confirm = await Application.Current!.MainPage!.DisplayAlert(
            "Confirmer l'importation",
            $"Importer {dataLines.Count} entrées de calibration pour le réservoir '{_reservoir.Numero}'?\n\nCette action remplacera toutes les données existantes.",
            "Importer",
            "Annuler");

        if (confirm)
        {
            Close(csvContent);
        }
    }
}