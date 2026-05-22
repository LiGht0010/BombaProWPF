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
/// Infrastructure shell — hosts a segmented pill bar across four sub-sections
/// (Produits, R\u00e9servoirs, Pompes, Services). Sub-section UserControls are
/// instantiated lazily on first selection and cached for the lifetime of this
/// view, so switching pills is instant after the initial build.
/// Jaugeage lives inside R\u00e9servoirs as a nested concern, not a top-level pill.
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
        // First show: surface the default section. The IsChecked=True on
        // PillProduits already fires Pill_Checked during template apply,
        // but Loaded re-asserts in case the section host was cleared.
        if (SectionHost.Content is null)
        {
            ShowSection("produits");
        }

        // Kick off any async data load on the currently visible section.
        _loadCts = new CancellationTokenSource();
        _ = TryLoadCurrentSectionAsync(_loadCts.Token);
    }

    private void UserControl_Unloaded(object sender, RoutedEventArgs e)
    {
        // Cancel in-flight loads so a slow fetch doesn't update a stale UI.
        _loadCts?.Cancel();
        _loadCts?.Dispose();
        _loadCts = null;
    }

    private void Pill_Checked(object sender, RoutedEventArgs e)
    {
        // Guard: the IsChecked="True" on PillProduits in XAML fires this
        // handler during InitializeComponent, before SectionHost has been
        // field-assigned. UserControl_Loaded handles the initial show.
        if (SectionHost is null) return;

        if (sender is RadioButton { Tag: string key })
        {
            ShowSection(key);

            // Trigger lazy load on the new section if it implements IAsyncLoadable.
            _loadCts ??= new CancellationTokenSource();
            _ = TryLoadCurrentSectionAsync(_loadCts.Token);
        }
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
