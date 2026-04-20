using BombaProMaxWPF.Models;
using BombaProMaxWPF.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace BombaProMaxWPF.ViewModels;

public partial class UserViewModel : ObservableObject
{
    private readonly UserService _userService;
    private readonly IDialogService _dialogService;

    public ObservableCollection<UserDto> Users { get; } = [];

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isSaving;

    [ObservableProperty]
    private UserDto? _selectedUser;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string? _errorMessage;

    // Statistics
    [ObservableProperty]
    private int _totalUsers;

    [ObservableProperty]
    private int _activeUsers;

    [ObservableProperty]
    private int _inactiveUsers;

    [ObservableProperty]
    private int _adminUsers;

    public UserViewModel(UserService userService, IDialogService dialogService)
    {
        _userService = userService;
        _dialogService = dialogService;
    }

    // ????????????????????????????????????????????????????????????????
    // LOAD COMMANDS
    // ????????????????????????????????????????????????????????????????

    [RelayCommand]
    public async Task LoadUsersAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            var users = await _userService.GetAllAsync();

            Users.Clear();
            foreach (var user in users.OrderByDescending(u => u.CreatedAt))
            {
                Users.Add(user);
            }

            CalculateStatistics();
            Debug.WriteLine($"[UserViewModel] Loaded {Users.Count} users");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur de chargement: {ex.Message}";
            Debug.WriteLine($"[UserViewModel] Error loading users: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task SearchUsersAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            var users = await _userService.SearchAsync(SearchText);

            Users.Clear();
            foreach (var user in users.OrderByDescending(u => u.CreatedAt))
            {
                Users.Add(user);
            }

            CalculateStatistics();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur de recherche: {ex.Message}";
            Debug.WriteLine($"[UserViewModel] Error searching users: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task FilterByStatusAsync(bool isActive)
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            // Filter client-side from all users
            var allUsers = await _userService.GetAllAsync();
            var filteredUsers = allUsers.Where(u => u.IsActive == isActive).ToList();

            Users.Clear();
            foreach (var user in filteredUsers.OrderByDescending(u => u.CreatedAt))
            {
                Users.Add(user);
            }

            CalculateStatistics();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur de filtrage: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    // ????????????????????????????????????????????????????????????????
    // CRUD COMMANDS
    // ????????????????????????????????????????????????????????????????

    [RelayCommand]
    public async Task AddUserAsync()
    {
        var currentUser = App.CurrentUser;
        if (currentUser == null || !currentUser.CanManageUsers)
        {
            await _dialogService.ShowAlertAsync("Accès refusé", "Vous n'avez pas la permission de gérer les utilisateurs");
            return;
        }

        var newUser = await _dialogService.ShowUserCreatePopupAsync();
        if (newUser != null)
        {
            Users.Insert(0, newUser);
            CalculateStatistics();
            await _dialogService.ShowAlertAsync("Succès", $"Utilisateur '{newUser.Name}' créé avec succès");
        }
    }

    [RelayCommand]
    public async Task EditUserAsync(UserDto? user)
    {
        if (user == null) return;

        var currentUser = App.CurrentUser;
        if (currentUser == null || !currentUser.CanManageUsers)
        {
            await _dialogService.ShowAlertAsync("Accès refusé", "Vous n'avez pas la permission de modifier les utilisateurs");
            return;
        }

        var success = await _dialogService.ShowUserEditPopupAsync(user);
        if (success)
        {
            await LoadUsersAsync();
        }
    }

    [RelayCommand]
    public async Task DeleteUserAsync(UserDto? user)
    {
        if (user == null) return;

        var currentUser = App.CurrentUser;
        if (currentUser == null || !currentUser.CanManageUsers)
        {
            await _dialogService.ShowAlertAsync("Accès refusé", "Vous n'avez pas la permission de supprimer les utilisateurs");
            return;
        }

        // Prevent deleting yourself
        if (user.UserId == currentUser.UserId)
        {
            await _dialogService.ShowAlertAsync("Action impossible", "Vous ne pouvez pas supprimer votre propre compte");
            return;
        }

        // Prevent deleting super admin if you're not super admin
        if (user.IsSuperAdmin && !currentUser.IsSuperAdmin)
        {
            await _dialogService.ShowAlertAsync("Action impossible", "Seul un superviseur peut supprimer un autre superviseur");
            return;
        }

        var confirm = await _dialogService.ShowConfirmationAsync(
            "Confirmer la suppression",
            $"Êtes-vous sûr de vouloir supprimer l'utilisateur '{user.Name}'?\n\nCette action est irréversible.");

        if (!confirm) return;

        try
        {
            IsSaving = true;
            var success = await _userService.DeleteAsync(user.UserId);

            if (success)
            {
                Users.Remove(user);
                CalculateStatistics();
                await _dialogService.ShowAlertAsync("Succès", "Utilisateur supprimé avec succès");
            }
            else
            {
                await _dialogService.ShowAlertAsync("Erreur", "Échec de la suppression de l'utilisateur");
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Erreur", $"Erreur lors de la suppression: {ex.Message}");
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand]
    public async Task ToggleUserStatusAsync(UserDto? user)
    {
        if (user == null) return;

        var currentUser = App.CurrentUser;
        if (currentUser == null || !currentUser.CanManageUsers)
        {
            await _dialogService.ShowAlertAsync("Accès refusé", "Vous n'avez pas la permission de modifier les utilisateurs");
            return;
        }

        // Prevent deactivating yourself
        if (user.UserId == currentUser.UserId)
        {
            await _dialogService.ShowAlertAsync("Action impossible", "Vous ne pouvez pas désactiver votre propre compte");
            return;
        }

        var newStatus = !user.IsActive;
        var action = newStatus ? "activer" : "désactiver";

        var confirm = await _dialogService.ShowConfirmationAsync(
            "Confirmer le changement",
            $"Voulez-vous {action} l'utilisateur '{user.Name}'?");

        if (!confirm) return;

        try
        {
            IsSaving = true;
            user.IsActive = newStatus;
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = currentUser.UserId;

            var success = await _userService.UpdateAsync(user);

            if (success)
            {
                CalculateStatistics();
                await _dialogService.ShowAlertAsync("Succès", $"Utilisateur {(newStatus ? "activé" : "désactivé")} avec succès");
            }
            else
            {
                user.IsActive = !newStatus; // Revert
                await _dialogService.ShowAlertAsync("Erreur", "Échec de la mise à jour du statut");
            }
        }
        catch (Exception ex)
        {
            user.IsActive = !newStatus; // Revert
            await _dialogService.ShowAlertAsync("Erreur", $"Erreur: {ex.Message}");
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand]
    public async Task ShowUserDetailsAsync(UserDto? user)
    {
        if (user == null) return;

        await _dialogService.ShowUserDetailsPopupAsync(user);
    }

    // ????????????????????????????????????????????????????????????????
    // HELPER METHODS
    // ????????????????????????????????????????????????????????????????

    private void CalculateStatistics()
    {
        TotalUsers = Users.Count;
        ActiveUsers = Users.Count(u => u.IsActive);
        InactiveUsers = Users.Count(u => !u.IsActive);
        AdminUsers = Users.Count(u => u.IsAdmin || u.IsSuperAdmin);
    }

    public void ClearError()
    {
        ErrorMessage = null;
    }
}
