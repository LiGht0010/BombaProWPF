using BombaProMaxWPF.Models;
using BombaProMaxWPF.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;

namespace BombaProMaxWPF.ViewModels;

/// <summary>
/// ViewModel for the read-only Jaugeage Details dialog.
/// Loads a <see cref="JaugeageWithDetailsDto"/> by ID and exposes
/// display-ready rows for the reservoir detail table.
/// </summary>
public partial class JaugeageDetailsViewModel : ObservableObject
{
    private readonly JaugeageService _jaugeageService = new();

    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string? _errorMessage;

    // ── Header fields ────────────────────────────────────────────────
    [ObservableProperty] private string _numero = "—";
    [ObservableProperty] private string _dateDisplay = "—";
    [ObservableProperty] private string _temoinNom = "—";
    [ObservableProperty] private string _statutText = "Validé";
    [ObservableProperty] private bool _isPending;
    [ObservableProperty] private string _observations = string.Empty;
    [ObservableProperty] private bool _hasObservations;

    public ObservableCollection<JaugeageDetailRow> DetailRows { get; } = new();

    /// <summary>Loads the jaugeage and its detail lines by ID.</summary>
    public async Task LoadAsync(int jaugeageId, CancellationToken ct = default)
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            var dto = await _jaugeageService.GetJaugeageWithDetailsAsync(jaugeageId).ConfigureAwait(false);
            ct.ThrowIfCancellationRequested();

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                if (dto is null)
                {
                    ErrorMessage = "Impossible de charger les détails du jaugeage.";
                    return;
                }

                Numero = string.IsNullOrWhiteSpace(dto.NumeroJaugeage)
                    ? $"#JG-{dto.ID:0000}"
                    : dto.NumeroJaugeage;

                var date = dto.DateJaugeage != default
                    ? dto.DateJaugeage
                    : dto.DateCreation ?? DateTime.MinValue;
                DateDisplay = date == DateTime.MinValue
                    ? "—"
                    : (date.Kind == DateTimeKind.Utc ? date.ToLocalTime() : date).ToString("dd/MM/yyyy");

                TemoinNom = string.IsNullOrWhiteSpace(dto.TemoinNom) ? "—" : dto.TemoinNom!;

                // Status — extend when a real status field is added to the API
                IsPending = false;
                StatutText = "Validé";

                Observations = string.IsNullOrWhiteSpace(dto.Observations)
                    ? string.Empty
                    : dto.Observations!;
                HasObservations = !string.IsNullOrWhiteSpace(Observations);

                DetailRows.Clear();
                foreach (var d in dto.Details)
                    DetailRows.Add(new JaugeageDetailRow(d));
            });
        }
        catch (OperationCanceledException) { /* dialog closed */ }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur: {ex.Message}";
            Debug.WriteLine($"[JaugeageDetailsVM] Load failed: {ex}");
        }
        finally
        {
            IsLoading = false;
        }
    }
}

/// <summary>
/// Display row for one <see cref="JaugeageDetailDto"/> in the reservoir table.
/// </summary>
public class JaugeageDetailRow
{
    public JaugeageDetailRow(JaugeageDetailDto d)
    {
        Source = d;
    }

    public JaugeageDetailDto Source { get; }

    /// <summary>"A1 - Cuve Nord" or just "A1" if no name is available.</summary>
    public string ReservoirDisplay
    {
        get
        {
            var num = Source.ReservoirNumero ?? $"#{Source.ReservoirID}";
            var nom = Source.ReservoirNom;
            return string.IsNullOrWhiteSpace(nom) ? num : $"{num} - {nom}";
        }
    }

    public string ProduitDisplay =>
        string.IsNullOrWhiteSpace(Source.ProduitNom) ? "—" : Source.ProduitNom!;

    public string HauteurDisplay => $"{Source.HauteurMesuree:0.0}";

    public string VolumeDisplay => $"{Source.VolumeCalcule:N2}";

    public string NotesDisplay =>
        string.IsNullOrWhiteSpace(Source.Notes) ? string.Empty : Source.Notes!;

    public bool HasNotes => !string.IsNullOrWhiteSpace(Source.Notes);
}
