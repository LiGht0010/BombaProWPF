using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using BombaProMaxWPF.Localization;
using BombaProMaxWPF.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Wpf.Ui.Controls;

namespace BombaProMaxWPF.ViewModels;

/// <summary>
/// Top-level view model that drives the main shell window: sidebar items,
/// pane toggle, current user pill and the navigation / logout events the
/// host window listens to.
/// </summary>
public partial class ShellViewModel : ObservableObject
{
    [ObservableProperty]
    private NavItem? _selectedItem;

    [ObservableProperty]
    private bool _isPaneOpen = true;

    [ObservableProperty]
    private string _userDisplayName = string.Empty;

    [ObservableProperty]
    private string _userRole = string.Empty;

    public ObservableCollection<NavItem> Items { get; }

    /// <summary>
    /// Raised after <see cref="SelectedItem"/> changes so the host can route
    /// the request to its <see cref="System.Windows.Controls.Frame"/>.
    /// </summary>
    public event Action<NavItem>? NavigationRequested;

    /// <summary>
    /// Raised when the user clicks the logout button in the sidebar footer.
    /// </summary>
    public event Action? LogoutRequested;

    /// <summary>
    /// Raised when the user clicks the sticky "Démarrer la journée" action.
    /// </summary>
    public event Action? StartDayRequested;

    public ShellViewModel()
    {
        Items = new ObservableCollection<NavItem>(BuildItems());

        var user = App.CurrentUser ?? App.user;
        UserDisplayName = user?.Name ?? "Invité";
        UserRole = user switch
        {
            { IsSuperAdmin: true } => "Super admin",
            { IsAdmin: true }      => "Administrateur",
            { }                    => "Utilisateur",
            _                      => string.Empty
        };
    }

    partial void OnSelectedItemChanged(NavItem? value)
    {
        if (value is not null)
        {
            NavigationRequested?.Invoke(value);
        }
    }

    [RelayCommand]
    private void TogglePane() => IsPaneOpen = !IsPaneOpen;

    [RelayCommand]
    private void StartDay() => StartDayRequested?.Invoke();

    [RelayCommand]
    private void Logout() => LogoutRequested?.Invoke();

    private static IEnumerable<NavItem> BuildItems()
    {
        var loc = LanguageManager.Instance;
        yield return new NavItem("dashboard",      () => loc["NavDashboard"],      SymbolRegular.DataPie24);
        yield return new NavItem("ventes",         () => loc["NavVentes"],         SymbolRegular.Cart24);
        yield return new NavItem("achats",         () => loc["NavAchats"],         SymbolRegular.BoxArrowUp24);
        yield return new NavItem("caisse",         () => loc["NavCaisse"],         SymbolRegular.Money24);
        yield return new NavItem("clients",        () => loc["NavClients"],        SymbolRegular.People24);
        yield return new NavItem("infrastructure", () => loc["NavInfrastructure"], SymbolRegular.Building24);
        yield return new NavItem("ressources",     () => loc["NavRessources"],     SymbolRegular.PeopleTeam24);
        yield return new NavItem("rapports",       () => loc["NavRapports"],       SymbolRegular.DocumentText24);
        yield return new NavItem("parametres",     () => loc["NavParametres"],     SymbolRegular.Settings24);
    }
}
