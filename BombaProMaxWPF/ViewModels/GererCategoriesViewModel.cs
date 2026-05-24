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
/// ViewModel for the Gérer les Catégories dialog.
/// Supports inline edit — clicking Edit turns the row label into a TextBox
/// without navigating away; clicking ✓ commits, ✗ reverts.
/// </summary>
public partial class GererCategoriesViewModel : ObservableObject
{
    private readonly CategorieService _service = new();

    public ObservableCollection<CategorieRowItem> Rows { get; } = new();

    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string _newNom = string.Empty;
    [ObservableProperty] private string? _errorMessage;
    [ObservableProperty] private bool _hasRows;

    public IAsyncRelayCommand LoadCommand { get; }
    public IAsyncRelayCommand AddCommand { get; }
    public IRelayCommand<CategorieRowItem> BeginEditCommand { get; }
    public IAsyncRelayCommand<CategorieRowItem> CommitEditCommand { get; }
    public IRelayCommand<CategorieRowItem> CancelEditCommand { get; }
    public IAsyncRelayCommand<CategorieRowItem> DeleteCommand { get; }

    public GererCategoriesViewModel()
    {
        LoadCommand = new AsyncRelayCommand(LoadAsync);
        AddCommand = new AsyncRelayCommand(AddAsync, () => !string.IsNullOrWhiteSpace(NewNom));
        BeginEditCommand = new RelayCommand<CategorieRowItem>(BeginEdit);
        CommitEditCommand = new AsyncRelayCommand<CategorieRowItem>(CommitEditAsync);
        CancelEditCommand = new RelayCommand<CategorieRowItem>(CancelEdit);
        DeleteCommand = new AsyncRelayCommand<CategorieRowItem>(DeleteAsync);
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
                    Rows.Add(new CategorieRowItem(c));
                HasRows = Rows.Count > 0;
            });
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur: {ex.Message}";
            Debug.WriteLine($"[GererCategoriesVM] Load failed: {ex}");
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
            var created = await _service.CreateCategorieAsync(new CategorieDto { Nom = nom })
                                        .ConfigureAwait(false);

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                if (created is not null)
                {
                    Rows.Add(new CategorieRowItem(created));
                    HasRows = true;
                    NewNom = string.Empty;
                    AddCommand.NotifyCanExecuteChanged();
                }
                else
                {
                    ErrorMessage = LanguageManager.Instance["CatDlgAddError"];
                }
            });
        }
        catch (Exception ex)
        {
            await Application.Current.Dispatcher.InvokeAsync(
                () => ErrorMessage = $"Erreur: {ex.Message}");
            Debug.WriteLine($"[GererCategoriesVM] Add failed: {ex}");
        }
    }

    // ── Inline edit ───────────────────────────────────────────────────

    private void BeginEdit(CategorieRowItem? row)
    {
        if (row is null) return;
        // Cancel any other open edit first
        foreach (var r in Rows)
            if (r.IsEditing && r != row) CancelEdit(r);
        row.BeginEdit();
    }

    private async Task CommitEditAsync(CategorieRowItem? row)
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
            var dto = new CategorieDto { ID = row.ID, Nom = newNom };
            var ok = await _service.UpdateCategorieAsync(dto).ConfigureAwait(false);

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                if (ok)
                    row.CommitEdit(newNom);
                else
                {
                    row.CancelEdit();
                    ErrorMessage = LanguageManager.Instance["CatDlgUpdateError"];
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
            Debug.WriteLine($"[GererCategoriesVM] Update failed: {ex}");
        }
    }

    private void CancelEdit(CategorieRowItem? row) => row?.CancelEdit();

    // ── Delete ────────────────────────────────────────────────────────

    private async Task DeleteAsync(CategorieRowItem? row)
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
                    ErrorMessage = LanguageManager.Instance["CatDlgDeleteError"];
                }
            });
        }
        catch (Exception ex)
        {
            await Application.Current.Dispatcher.InvokeAsync(
                () => ErrorMessage = $"Erreur: {ex.Message}");
            Debug.WriteLine($"[GererCategoriesVM] Delete failed: {ex}");
        }
    }

    // ── NewNom change → revalidate Add ───────────────────────────────

    partial void OnNewNomChanged(string value) =>
        AddCommand.NotifyCanExecuteChanged();
}

/// <summary>
/// Observable row item for a single <see cref="CategorieDto"/>.
/// Supports toggling between display mode and inline-edit mode.
/// </summary>
public partial class CategorieRowItem : ObservableObject
{
    public CategorieRowItem(CategorieDto dto)
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
