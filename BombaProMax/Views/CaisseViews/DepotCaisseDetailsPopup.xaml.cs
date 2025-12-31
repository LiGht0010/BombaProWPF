using BombaProMax.Models;
using BombaProMax.Services;
using CommunityToolkit.Maui.Views;

namespace BombaProMax.Views.CaisseViews;

public partial class DepotCaisseDetailsPopup : Popup
{
    private readonly DepotCaisseDto _depot;
    private readonly UserService _userService;

    public DepotCaisseDetailsPopup(DepotCaisseDto depot)
    {
        InitializeComponent();
        _depot = depot;
        _userService = new UserService();
        CanBeDismissedByTappingOutsideOfPopup = false;
        LoadDepotDetails();
        LoadAuditUserNamesAsync();
    }

    private void LoadDepotDetails()
    {
        // Main info
        MontantLabel.Text = _depot.MontantDisplay;
        DateLabel.Text = _depot.DateDepot.ToString("dddd, dd MMMM yyyy");
        ReferenceLabel.Text = string.IsNullOrWhiteSpace(_depot.ReferenceBancaire) ? "-" : _depot.ReferenceBancaire;
        BanqueLabel.Text = string.IsNullOrWhiteSpace(_depot.Banque) ? "-" : _depot.Banque;
        ValidateurLabel.Text = string.IsNullOrWhiteSpace(_depot.ValidateurNom) ? "-" : _depot.ValidateurNom;
        NotesLabel.Text = string.IsNullOrWhiteSpace(_depot.Notes) ? "Aucune note" : _depot.Notes;

        // Subtitle
        SubtitleLabel.Text = $"Depot du {_depot.DateDepot:dd/MM/yyyy}";

        // Audit info - dates
        DateCreationLabel.Text = _depot.DateCreation?.ToString("dd/MM/yyyy HH:mm") ?? "-";
        DateModificationLabel.Text = _depot.DateModification?.ToString("dd/MM/yyyy HH:mm") ?? "-";

        // Piece justificative
        if (_depot.HasPieceJustificative)
        {
            PieceJustificativeSection.IsVisible = true;
            NoPieceSection.IsVisible = false;

            FileNameLabel.Text = _depot.PieceJustificativeNom ?? "Fichier";
            FileTypeLabel.Text = GetFileTypeDescription(_depot.PieceJustificativeType);

            // Show image preview for images
            if (_depot.PieceJustificativeType?.StartsWith("image/") == true)
            {
                try
                {
                    var bytes = Convert.FromBase64String(_depot.PieceJustificativeBase64!);
                    ImagePreview.Source = ImageSource.FromStream(() => new MemoryStream(bytes));
                    ImagePreviewContainer.IsVisible = true;
                }
                catch
                {
                    ImagePreviewContainer.IsVisible = false;
                }
            }
            else
            {
                ImagePreviewContainer.IsVisible = false;
            }
        }
        else
        {
            PieceJustificativeSection.IsVisible = false;
            NoPieceSection.IsVisible = true;
        }
    }

    private async void LoadAuditUserNamesAsync()
    {
        try
        {
            var createdByName = await _userService.GetUserNameByIdAsync(_depot.AjoutePar);
            CreatedByLabel.Text = createdByName;

            var modifiedByName = await _userService.GetUserNameByIdAsync(_depot.ModifiePar);
            ModifiedByLabel.Text = modifiedByName;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DepotCaisseDetailsPopup] Error loading audit info: {ex.Message}");
            CreatedByLabel.Text = "Erreur de chargement";
            ModifiedByLabel.Text = "Erreur de chargement";
        }
    }

    private static string GetFileTypeDescription(string? mimeType)
    {
        return mimeType switch
        {
            "image/jpeg" => "Image JPEG",
            "image/png" => "Image PNG",
            "application/pdf" => "Document PDF",
            _ => "Fichier"
        };
    }

    private async void OnViewFileClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(_depot.PieceJustificativeBase64))
            return;

        try
        {
            var bytes = Convert.FromBase64String(_depot.PieceJustificativeBase64);
            var fileName = _depot.PieceJustificativeNom ?? $"justificatif_{_depot.ID}{GetFileExtension(_depot.PieceJustificativeType)}";
            
            // Save to temp file and open
            var tempPath = Path.Combine(FileSystem.CacheDirectory, fileName);
            await File.WriteAllBytesAsync(tempPath, bytes);

            await Launcher.Default.OpenAsync(new OpenFileRequest
            {
                File = new ReadOnlyFile(tempPath)
            });
        }
        catch (Exception ex)
        {
            await Application.Current!.MainPage!.DisplayAlert("Erreur", $"Impossible d'ouvrir le fichier: {ex.Message}", "OK");
        }
    }

    private async void OnDownloadFileClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(_depot.PieceJustificativeBase64))
            return;

        try
        {
            var bytes = Convert.FromBase64String(_depot.PieceJustificativeBase64);
            var fileName = _depot.PieceJustificativeNom ?? $"justificatif_{_depot.ID}{GetFileExtension(_depot.PieceJustificativeType)}";

#if WINDOWS
            // On Windows, use FileSavePicker
            var savePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);
            
            // Check if file exists and create unique name
            var counter = 1;
            var baseName = Path.GetFileNameWithoutExtension(fileName);
            var extension = Path.GetExtension(fileName);
            while (File.Exists(savePath))
            {
                savePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), $"{baseName}_{counter}{extension}");
                counter++;
            }

            await File.WriteAllBytesAsync(savePath, bytes);
            await Application.Current!.MainPage!.DisplayAlert("Succes", $"Fichier enregistre dans:\n{savePath}", "OK");
            
            // Open folder containing the file
            System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{savePath}\"");
#else
            // On mobile, save to app's documents and share
            var tempPath = Path.Combine(FileSystem.CacheDirectory, fileName);
            await File.WriteAllBytesAsync(tempPath, bytes);

            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Enregistrer le justificatif",
                File = new ShareFile(tempPath)
            });
#endif
        }
        catch (Exception ex)
        {
            await Application.Current!.MainPage!.DisplayAlert("Erreur", $"Impossible de telecharger le fichier: {ex.Message}", "OK");
        }
    }

    private static string GetFileExtension(string? mimeType)
    {
        return mimeType switch
        {
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            "application/pdf" => ".pdf",
            _ => ""
        };
    }

    private async void OnEditClicked(object sender, EventArgs e)
    {
        // Return the depot to signal edit mode
        await CloseAsync(("edit", _depot));
    }

    private async void OnCloseClicked(object sender, EventArgs e)
    {
        await CloseAsync(null);
    }
}
