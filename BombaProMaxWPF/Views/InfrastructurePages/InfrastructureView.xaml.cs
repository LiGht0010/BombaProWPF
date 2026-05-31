using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using BombaProMaxWPF.Services;
using BombaProMaxWPF.Views.InfrastructurePages.Sections.Produits;
using BombaProMaxWPF.Views.InfrastructurePages.Sections.Reservoirs;
using BombaProMaxWPF.Views.InfrastructurePages.Sections.Pompes;
using BombaProMaxWPF.Views.InfrastructurePages.Sections.Services;

namespace BombaProMaxWPF.Views.InfrastructurePages;

/// <summary>
/// Infrastructure shell — hub-and-spoke pattern.
/// State 1 (landing): four clickable cards, one per sub-section.
/// State 2 (section): selected sub-section UserControl with a back button.
/// Sub-section UserControls are instantiated lazily on first selection and
/// cached for the lifetime of this view.
/// Jaugeage lives inside Réservoirs as a nested concern.
/// </summary>
public partial class InfrastructureView : UserControl
{
    private readonly Dictionary<string, UserControl> _sectionCache = new(StringComparer.OrdinalIgnoreCase);
    private CancellationTokenSource? _loadCts;

    public InfrastructureView()
    {
        InitializeComponent();
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        // Always start on the landing hub.
        LandingPanel.Visibility  = Visibility.Visible;
        SectionPanel.Visibility  = Visibility.Collapsed;
        BackButton.Visibility    = Visibility.Collapsed;

        _loadCts = new CancellationTokenSource();
    }

    private void UserControl_Unloaded(object sender, RoutedEventArgs e)
    {
        // Cancel in-flight loads so a slow fetch doesn't update a stale UI.
        _loadCts?.Cancel();
        _loadCts?.Dispose();
        _loadCts = null;
    }

    private void Card_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: string key }) return;

        ShowSection(key);

        LandingPanel.Visibility = Visibility.Collapsed;
        SectionPanel.Visibility = Visibility.Visible;
        BackButton.Visibility   = Visibility.Visible;

        _loadCts ??= new CancellationTokenSource();
        _ = TryLoadCurrentSectionAsync(_loadCts.Token);
    }

    private void Back_Click(object sender, RoutedEventArgs e)
    {
        SectionPanel.Visibility = Visibility.Collapsed;
        LandingPanel.Visibility = Visibility.Visible;
        BackButton.Visibility   = Visibility.Collapsed;
    }

    private void ShowSection(string key)
    {
        if (!_sectionCache.TryGetValue(key, out var section))
        {
            section = key switch
            {
                "produits"   => new ProduitsSection(),
                "reservoirs" => new ReservoirsSection(),
                "pompes"     => new PompesSection(),
                "services"   => new ServicesSection(),
                _ => throw new ArgumentOutOfRangeException(nameof(key), key, "Unknown infrastructure section."),
            };
            _sectionCache[key] = section;
        }

        SectionHost.Content = section;
    }

    private Task TryLoadCurrentSectionAsync(CancellationToken ct)
    {
        // Sections currently ship as static placeholders. When their VMs are
        // ported and start implementing IAsyncLoadable, this hook fires
        // EnsureLoadedAsync on first show and is idempotent thereafter.
        if (SectionHost.Content is FrameworkElement fe &&
            fe.DataContext is IAsyncLoadable loadable)
        {
            return loadable.EnsureLoadedAsync(ct);
        }
        return Task.CompletedTask;
    }
}
