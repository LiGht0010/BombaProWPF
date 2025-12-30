using BombaProMax.Models;
using CommunityToolkit.Maui.Views;

namespace BombaProMax.Views.CaisseViews;

public partial class DepotCaisseCreatePopup : Popup
{
    private readonly DepotCaisseDto? _existingDepot;
    private readonly bool _isEditMode;
    
    private string? _selectedFileName;
    private string? _selectedFileBase64;
    private string? _selectedFileType;

    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

    public DepotCaisseCreatePopup()
    {
        InitializeComponent();
        CanBeDismissedByTappingOutsideOfPopup = false;

        _isEditMode = false;
        _existingDepot = null;
        DateDepotPicker.Date = DateTime.Today;
    }

    public DepotCaisseCreatePopup(DepotCaisseDto depot)
    {
        InitializeComponent();
        _isEditMode = true;
        _existingDepot = depot;
        
        TitleLabel.Text = "Modifier le Depot";
        SaveButton.Text = "Mettre a jour";
        
        MontantEntry.Text = depot.Montant.ToString("F2");
        DateDepotPicker.Date = depot.DateDepot.Date;
        ReferenceEntry.Text = depot.ReferenceBancaire;
        BanqueEntry.Text = depot.Banque;
        NotesEditor.Text = depot.Notes;

        // Load existing file if present
        if (!string.IsNullOrEmpty(depot.PieceJustificativeBase64))
        {
            _selectedFileName = depot.PieceJustificativeNom;
            _selectedFileBase64 = depot.PieceJustificativeBase64;
            _selectedFileType = depot.PieceJustificativeType;
            UpdateFilePreview();
        }
    }

    private async void OnTakePhotoClicked(object sender, EventArgs e)
    {
        try
        {
            if (!MediaPicker.Default.IsCaptureSupported)
            {
                ErrorLabel.Text = "La capture photo n'est pas supportee sur cet appareil.";
                ErrorLabel.IsVisible = true;
                return;
            }

            var photo = await MediaPicker.Default.CapturePhotoAsync();
            if (photo != null)
            {
                await ProcessSelectedFile(photo);
            }
        }
        catch (PermissionException)
        {
            ErrorLabel.Text = "Permission camera refusee.";
            ErrorLabel.IsVisible = true;
        }
        catch (Exception ex)
        {
            ErrorLabel.Text = $"Erreur lors de la capture: {ex.Message}";
            ErrorLabel.IsVisible = true;
        }
    }

    private async void OnPickFileClicked(object sender, EventArgs e)
    {
        try
        {
            var customFileType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.iOS, new[] { "public.jpeg", "public.png", "com.adobe.pdf" } },
                { DevicePlatform.Android, new[] { "image/jpeg", "image/png", "application/pdf" } },
                { DevicePlatform.WinUI, new[] { ".jpg", ".jpeg", ".png", ".pdf" } },
                { DevicePlatform.macOS, new[] { "public.jpeg", "public.png", "com.adobe.pdf" } }
            });

            var options = new PickOptions
            {
                PickerTitle = "Selectionner un justificatif",
                FileTypes = customFileType
            };

            var result = await FilePicker.Default.PickAsync(options);
            if (result != null)
            {
                await ProcessSelectedFile(result);
            }
        }
        catch (Exception ex)
        {
            ErrorLabel.Text = $"Erreur lors de la selection: {ex.Message}";
            ErrorLabel.IsVisible = true;
        }
    }

    private async Task ProcessSelectedFile(FileResult file)
    {
        try
        {
            ErrorLabel.IsVisible = false;

            using var stream = await file.OpenReadAsync();
            
            // Check file size
            if (stream.Length > MaxFileSizeBytes)
            {
                ErrorLabel.Text = "Le fichier depasse la taille maximale de 5 Mo.";
                ErrorLabel.IsVisible = true;
                return;
            }

            // Read file to byte array
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            var bytes = memoryStream.ToArray();

            _selectedFileName = file.FileName;
            _selectedFileBase64 = Convert.ToBase64String(bytes);
            _selectedFileType = file.ContentType ?? GetMimeType(file.FileName);

            UpdateFilePreview();
        }
        catch (Exception ex)
        {
            ErrorLabel.Text = $"Erreur lors du traitement: {ex.Message}";
            ErrorLabel.IsVisible = true;
        }
    }

    private void UpdateFilePreview()
    {
        FileNameLabel.Text = _selectedFileName ?? "Aucun fichier selectionne";
        FileNameLabel.TextColor = string.IsNullOrEmpty(_selectedFileName) ? Color.FromArgb("#999") : Color.FromArgb("#333");

        // Show image preview for image files
        if (!string.IsNullOrEmpty(_selectedFileBase64) && _selectedFileType?.StartsWith("image/") == true)
        {
            try
            {
                var bytes = Convert.FromBase64String(_selectedFileBase64);
                ImagePreview.Source = ImageSource.FromStream(() => new MemoryStream(bytes));
                ImagePreviewContainer.IsVisible = true;
            }
            catch
            {
                ImagePreviewContainer.IsVisible = false;
            }
        }
        else if (!string.IsNullOrEmpty(_selectedFileBase64) && _selectedFileType == "application/pdf")
        {
            // Show PDF icon
            ImagePreview.Source = "pdf_icon.png";
            ImagePreviewContainer.IsVisible = true;
        }
        else
        {
            ImagePreviewContainer.IsVisible = false;
        }
    }

    private void OnRemoveFileClicked(object sender, EventArgs e)
    {
        _selectedFileName = null;
        _selectedFileBase64 = null;
        _selectedFileType = null;
        FileNameLabel.Text = "Aucun fichier selectionne";
        FileNameLabel.TextColor = Color.FromArgb("#999");
        ImagePreviewContainer.IsVisible = false;
    }

    private static string GetMimeType(string fileName)
    {
        var extension = Path.GetExtension(fileName)?.ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".pdf" => "application/pdf",
            _ => "application/octet-stream"
        };
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (!decimal.TryParse(MontantEntry.Text, out decimal montant) || montant <= 0)
        {
            ErrorLabel.Text = "Veuillez entrer un montant valide superieur a 0.";
            ErrorLabel.IsVisible = true;
            return;
        }

        var depot = _isEditMode && _existingDepot != null
            ? new DepotCaisseDto
            {
                ID = _existingDepot.ID,
                Montant = montant,
                DateDepot = DateDepotPicker.Date,
                ReferenceBancaire = string.IsNullOrWhiteSpace(ReferenceEntry.Text) ? null : ReferenceEntry.Text.Trim(),
                Banque = string.IsNullOrWhiteSpace(BanqueEntry.Text) ? null : BanqueEntry.Text.Trim(),
                Notes = string.IsNullOrWhiteSpace(NotesEditor.Text) ? null : NotesEditor.Text.Trim(),
                PieceJustificativeNom = _selectedFileName,
                PieceJustificativeBase64 = _selectedFileBase64,
                PieceJustificativeType = _selectedFileType,
                ValidePar = _existingDepot.ValidePar,
                AjoutePar = _existingDepot.AjoutePar,
                DateCreation = _existingDepot.DateCreation
            }
            : new DepotCaisseDto
            {
                Montant = montant,
                DateDepot = DateDepotPicker.Date,
                ReferenceBancaire = string.IsNullOrWhiteSpace(ReferenceEntry.Text) ? null : ReferenceEntry.Text.Trim(),
                Banque = string.IsNullOrWhiteSpace(BanqueEntry.Text) ? null : BanqueEntry.Text.Trim(),
                Notes = string.IsNullOrWhiteSpace(NotesEditor.Text) ? null : NotesEditor.Text.Trim(),
                PieceJustificativeNom = _selectedFileName,
                PieceJustificativeBase64 = _selectedFileBase64,
                PieceJustificativeType = _selectedFileType,
                ValidePar = App.CurrentUser?.UserId ?? App.user?.UserId
            };

        await CloseAsync(depot);
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await CloseAsync(null);
    }
}
