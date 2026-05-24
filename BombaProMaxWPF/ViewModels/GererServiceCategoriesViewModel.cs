using BombaProMaxWPF.Localization;
using BombaProMaxWPF.Models;
using BombaProMaxWPF.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;

namespace BombaProMaxWPF.ViewModels;

/// <summary>
/// ViewModel for the Gérer les Catégories de Services dialog.
/// Supports inline edit — clicking Edit turns the row label into a TextBox;
/// clicking ✓ commits, ✗ reverts.
/// </summary>
public partial class GererServiceCategoriesViewModel : ObservableObject
{
    private readonly ServiceCategorieService _service = new();

    public ObservableCollection<ServiceCategorieRowItem> Rows { get; } = new();

    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string _newNom = string.Empty;
    [ObservableProperty] private string? _errorMessage;
    [ObservableProperty] private bool _hasRows;

    public IAsyncRelayCommand LoadCommand { get; }
    public IAsyncRelayCommand AddCommand { get; }
    public IRelayCommand<ServiceCategorieRowItem> BeginEditCommand { get; }
    public IAsyncRelayCommand<ServiceCategorieRowItem> CommitEditCommand { get; }
    public IRelayCommand<ServiceCategorieRowItem> CancelEditCommand { get; }
    public IAsyncRelayCommand<ServiceCategorieRowItem> DeleteCommand { get; }

    public GererServiceCategoriesViewModel()
    {
        LoadCommand = new AsyncRelayCommand(LoadAsync);
        AddCommand = new AsyncRelayCommand(AddAsync, () => !string.IsNullOrWhiteSpace(NewNom));
        BeginEditCommand = new RelayCommand<ServiceCategorieRowItem>(BeginEdit);
        CommitEditCommand = new AsyncRelayCommand<ServiceCategorieRowItem>(CommitEditAsync);
        CancelEditCommand = new RelayCommand<ServiceCategorieRowItem>(CancelEdit);
        DeleteCommand = new AsyncRelayCommand<ServiceCategorieRowItem>(DeleteAsync);
    }

    // ── Load ─────────────────────────────────────────────────────────

    public async Task LoadAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            var list = await _service.GetAllCategoriesAsync().ConfigureAwait(false);

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                Rows.Clear();
                foreach (var c in list)
                    Rows.Add(new ServiceCategorieRowItem(c));
                HasRows = Rows.Count > 0;
            });
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur: {ex.Message}";
            Debug.WriteLine($"[GererServiceCategoriesVM] Load failed: {ex}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    // ── Add ──────────────────────────────────────────────────────────

    private async Task AddAsync()
    {
        var nom = NewNom.Trim();
        if (string.IsNullOrWhiteSpace(nom)) return;

        try
        {
            ErrorMessage = null;
            var created = await _service.CreateCategorieAsync(new ServiceCategorieDto { Nom = nom })
                                        .ConfigureAwait(false);

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                if (created is not null)
                {
                    Rows.Add(new ServiceCategorieRowItem(created));
                    HasRows = true;
                    NewNom = string.Empty;
                    AddCommand.NotifyCanExecuteChanged();
                }
                else
                {
                    ErrorMessage = LanguageManager.Instance["CatSrvDlgAddError"];
                }
            });
        }
        catch (Exception ex)
        {
            await Application.Current.Dispatcher.InvokeAsync(
                () => ErrorMessage = $"Erreur: {ex.Message}");
            Debug.WriteLine($"[GererServiceCategoriesVM] Add failed: {ex}");
        }
    }

    // ── Inline edit ───────────────────────────────────────────────────

    private void BeginEdit(ServiceCategorieRowItem? row)
    {
        if (row is null) return;
        foreach (var r in Rows)
            if (r.IsEditing && r != row) CancelEdit(r);
        row.BeginEdit();
    }

    private async Task CommitEditAsync(ServiceCategorieRowItem? row)
    {
        if (row is null) return;
        var newNom = row.EditText.Trim();
        if (string.IsNullOrWhiteSpace(newNom))
        {
            CancelEdit(row);
            return;
        }

        try
        {
            ErrorMessage = null;
            var dto = new ServiceCategorieDto { ID = row.ID, Nom = newNom };
            var ok = await _service.UpdateCategorieAsync(dto).ConfigureAwait(false);

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                if (ok)
                    row.CommitEdit(newNom);
                else
                {
                    row.CancelEdit();
                    ErrorMessage = LanguageManager.Instance["CatSrvDlgUpdateError"];
                }
            });
        }
        catch (Exception ex)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                row.CancelEdit();
                ErrorMessage = $"Erreur: {ex.Message}";
            });
            Debug.WriteLine($"[GererServiceCategoriesVM] Update failed: {ex}");
        }
    }

    private void CancelEdit(ServiceCategorieRowItem? row) => row?.CancelEdit();

    // ── Delete ────────────────────────────────────────────────────────

    private async Task DeleteAsync(ServiceCategorieRowItem? row)
    {
        if (row is null) return;
        try
        {
            ErrorMessage = null;
            var ok = await _service.DeleteCategorieAsync(row.ID).ConfigureAwait(false);

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                if (ok)
                {
                    Rows.Remove(row);
                    HasRows = Rows.Count > 0;
                }
                else
                {
                    ErrorMessage = LanguageManager.Instance["CatSrvDlgDeleteError"];
                }
            });
        }
        catch (Exception ex)
        {
            await Application.Current.Dispatcher.InvokeAsync(
                () => ErrorMessage = $"Erreur: {ex.Message}");
            Debug.WriteLine($"[GererServiceCategoriesVM] Delete failed: {ex}");
        }
    }

    partial void OnNewNomChanged(string value) =>
        AddCommand.NotifyCanExecuteChanged();
}

/// <summary>
/// Observable row item for a single <see cref="ServiceCategorieDto"/>.
/// Supports toggling between display mode and inline-edit mode.
/// </summary>
public partial class ServiceCategorieRowItem : ObservableObject
{
    public ServiceCategorieRowItem(ServiceCategorieDto dto)
    {
        ID = dto.ID;
        _nom = dto.Nom;
        _editText = dto.Nom;
    }

    public int ID { get; }

    [ObservableProperty] private string _nom;
    [ObservableProperty] private string _editText;
    [ObservableProperty] private bool _isEditing;

    /// <summary>Enters edit mode — seeds EditText from current Nom.</summary>
    public void BeginEdit()
    {
        EditText = Nom;
        IsEditing = true;
    }

    /// <summary>Persists the new name and exits edit mode.</summary>
    public void CommitEdit(string newNom)
    {
        Nom = newNom;
        IsEditing = false;
    }

    /// <summary>Reverts to the original name and exits edit mode.</summary>
    public void CancelEdit()
    {
        EditText = Nom;
        IsEditing = false;
    }
}
