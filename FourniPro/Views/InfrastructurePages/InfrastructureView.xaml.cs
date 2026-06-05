using FourniPro.Views.InfrastructurePages.Sections.Achats;
using FourniPro.Views.InfrastructurePages.Sections.Camions;
using FourniPro.Views.InfrastructurePages.Sections.Citernes;
using FourniPro.Views.InfrastructurePages.Sections.Chauffeurs;
using FourniPro.Views.InfrastructurePages.Sections.Clients;
using FourniPro.Views.InfrastructurePages.Sections.Credits;
using FourniPro.Views.InfrastructurePages.Sections.Fournisseurs;
using FourniPro.Views.InfrastructurePages.Sections.Ventes;
using FourniPro.Views.InfrastructurePages.Sections.Produits;
using FourniPro.Views.InfrastructurePages.Sections.Employes;
using FourniPro.Views.InfrastructurePages.Sections.Voyages;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace FourniPro.Views.InfrastructurePages;

/// <summary>
/// Infrastructure shell — hub-and-spoke pattern.
/// State 1 (landing): section cards.
/// State 2 (section): selected sub-section UserControl with a back button.
/// Sub-section UserControls are instantiated lazily on first selection and
/// cached for the lifetime of this view.
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
        LandingPanel.Visibility = Visibility.Visible;
        SectionPanel.Visibility = Visibility.Collapsed;
        BackButton.Visibility   = Visibility.Collapsed;
        _loadCts = new CancellationTokenSource();
    }

    private void UserControl_Unloaded(object sender, RoutedEventArgs e)
    {
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
                "produits"      => new ProduitsSection(),
                "fournisseurs"  => new FournisseursSection(),
                "clients"       => new ClientsSection(),
                "chauffeurs"    => new ChauffeursSection(),
                "camions"       => new CamionsSection(),
                "citernes"      => new CiternesSection(),
                "achats"        => new AchatsSection(),
                "ventes"        => new VentesSection(),
                "employes"      => new EmployesSection(),
                "credits"       => new CreditsSection(),
                "voyages"       => new VoyagesSection(),
                _ => throw new ArgumentOutOfRangeException(nameof(key), key, "Unknown infrastructure section.")
            };
            _sectionCache[key] = section;
        }

        SectionHost.Content = section;
    }

    private Task TryLoadCurrentSectionAsync(CancellationToken ct)
    {
        if (SectionHost.Content is FrameworkElement { DataContext: ViewModels.ProduitsSectionViewModel pvm })
            return pvm.EnsureLoadedAsync(ct);

        if (SectionHost.Content is FrameworkElement { DataContext: ViewModels.FournisseursSectionViewModel fvm })
            return fvm.EnsureLoadedAsync(ct);

        if (SectionHost.Content is FrameworkElement { DataContext: ViewModels.ChauffeursSectionViewModel cvm })
            return cvm.EnsureLoadedAsync();

        if (SectionHost.Content is FrameworkElement { DataContext: ViewModels.CamionsSectionViewModel camvm })
            return camvm.EnsureLoadedAsync();

        if (SectionHost.Content is FrameworkElement { DataContext: ViewModels.CiternesSectionViewModel ctrvm })
            return ctrvm.EnsureLoadedAsync();

        if (SectionHost.Content is FrameworkElement { DataContext: ViewModels.AchatsSectionViewModel acvm })
            return acvm.EnsureLoadedAsync();

        if (SectionHost.Content is FrameworkElement { DataContext: ViewModels.VentesSectionViewModel vcvm })
            return vcvm.EnsureLoadedAsync();

        if (SectionHost.Content is FrameworkElement { DataContext: ViewModels.EmployesSectionViewModel ecvm })
            return ecvm.EnsureLoadedAsync();

        if (SectionHost.Content is FrameworkElement { DataContext: ViewModels.CreditsSectionViewModel crvm })
            return crvm.EnsureLoadedAsync();

        if (SectionHost.Content is FrameworkElement { DataContext: ViewModels.VoyagesSectionViewModel vgsvm })
            return vgsvm.EnsureLoadedAsync();

        return Task.CompletedTask;
    }
}
