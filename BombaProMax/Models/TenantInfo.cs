using CommunityToolkit.Mvvm.ComponentModel;

namespace BombaProMax.Models;

/// <summary>
/// Represents a tenant/client profile for display in the selection screen.
/// </summary>
public partial class TenantInfo : ObservableObject
{
    /// <summary>
    /// Unique identifier for the tenant (e.g., "client1", "sidikassem")
    /// </summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Display name for the tenant
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional description or location
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Icon or initials to display (e.g., "C1", "SK")
    /// </summary>
    public string Initials { get; set; } = string.Empty;

    /// <summary>
    /// Color for the tenant card (hex string)
    /// </summary>
    public string Color { get; set; } = "#1F4E45";

    /// <summary>
    /// Whether this tenant is currently selected
    /// </summary>
    [ObservableProperty]
    private bool _isSelected;
}
