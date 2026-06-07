using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FourniPro.Localization;
using FourniPro.Models;
using Wpf.Ui.Controls;

namespace FourniPro.ViewModels;

/// <summary>
/// Top-level view model that drives the main shell window: sidebar items,
/// pane toggle, current user pill and navigation / logout events.
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

    /// <summary>Raised after <see cref="SelectedItem"/> changes so the host routes navigation.</summary>
    public event Action<NavItem>? NavigationRequested;

    /// <summary>Raised when the user clicks the logout button.</summary>
    public event Action? LogoutRequested;

    public ShellViewModel()
    {
        Items = new ObservableCollection<NavItem>(BuildItems());

        var user = App.CurrentUser;
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
            NavigationRequested?.Invoke(value);
    }

    [RelayCommand]
    private void TogglePane() => IsPaneOpen = !IsPaneOpen;

    [RelayCommand]
    private void Logout() => LogoutRequested?.Invoke();

    private static IEnumerable<NavItem> BuildItems()
    {
        var loc = LanguageManager.Instance;
        yield return new NavItem("dashboard",   () => loc["Nav_Dashboard"],   SymbolRegular.DataPie24,        tooltipAccessor: () => loc["NavTip_Dashboard"]);
        yield return new NavItem("operations",  () => loc["Nav_Operations"],  SymbolRegular.ArrowTrending24,  tooltipAccessor: () => loc["NavTip_Operations"]);
        yield return new NavItem("partenaires", () => loc["Nav_Partenaires"], SymbolRegular.People24,         tooltipAccessor: () => loc["NavTip_Partenaires"]);
        yield return new NavItem("ressources",  () => loc["Nav_Ressources"],  SymbolRegular.Box24,            tooltipAccessor: () => loc["NavTip_Ressources"]);
        yield return new NavItem("caisse",      () => loc["Nav_Caisse"],      SymbolRegular.Money24,        isEnabled: false, tooltipAccessor: () => loc["NavTip_Caisse"]);
        yield return new NavItem("rapports",    () => loc["Nav_Rapports"],    SymbolRegular.DocumentText24, isEnabled: false, tooltipAccessor: () => loc["NavTip_Rapports"]);
        yield return new NavItem("parametres",  () => loc["Nav_Parametres"],  SymbolRegular.Settings24,       tooltipAccessor: () => loc["NavTip_Parametres"]);
    }
}
