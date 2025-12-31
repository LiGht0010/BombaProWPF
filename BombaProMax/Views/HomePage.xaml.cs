using BombaProMax.Services;
using BombaProMax.Views.AchatViews;
using BombaProMax.Views.ClientViews;
using BombaProMax.Views.DepenseViews;
using BombaProMax.Views.PeriodeViews;
using BombaProMax.Views.PompeViews;
using BombaProMax.Views.ProduitViews;
using BombaProMax.Views.ReservoirViews;
using BombaProMax.Views.User;
using BombaProMax.Views.VenteLubEtArticles;
using System.Diagnostics;

namespace BombaProMax.Views;

public partial class HomePage : ContentPage
{
    public HomePage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        UpdateTenantDisplay();
        await LoadDashboardDataAsync();
    }

    private void UpdateTenantDisplay()
    {
        TenantNameLabel.Text = ApiConfig.TenantFullDisplayName;
    }

    private async Task LoadDashboardDataAsync()
    {
        try
        {
            // Update user status
            var userName = App.CurrentUser?.Name ?? App.user?.Name ?? "Utilisateur";
            UserStatusLabel.Text = $"Connecté en tant que {userName} • Système actif";

            // Use HttpClientFactory for tenant-aware requests
            using var httpClient = HttpClientFactory.Create();
            var baseUrl = ApiConfig.Home;

            var tasks = new List<Task<string>>
            {
                SafeGetAsync(httpClient, $"{baseUrl}/Employes"),
                SafeGetAsync(httpClient, $"{baseUrl}/Fournisseurs"),
                SafeGetAsync(httpClient, $"{baseUrl}/Produits"),
                SafeGetAsync(httpClient, $"{baseUrl}/Pompes"),
                SafeGetAsync(httpClient, $"{baseUrl}/Reservoirs"),
                SafeGetAsync(httpClient, $"{baseUrl}/Clients"),
                SafeGetAsync(httpClient, $"{baseUrl}/Depenses"),
                SafeGetAsync(httpClient, $"{baseUrl}/Achats"),
                SafeGetAsync(httpClient, $"{baseUrl}/Periodes")
            };

            var results = await Task.WhenAll(tasks);

            // Parse counts from JSON arrays
            var employesCount = CountItems(results[0]);
            var fournisseursCount = CountItems(results[1]);
            var produitsCount = CountItems(results[2]);
            var pompesCount = CountItems(results[3]);
            var reservoirsCount = CountItems(results[4]);
            var clientsCount = CountItems(results[5]);
            var depensesCount = CountItems(results[6]);
            var achatsCount = CountItems(results[7]);
            var periodesCount = CountItems(results[8]);

            // Update UI on main thread
            MainThread.BeginInvokeOnMainThread(() =>
            {
                EmployesCountLabel.Text = employesCount.ToString();
                FournisseursCountLabel.Text = fournisseursCount.ToString();
                ProduitsCountLabel.Text = produitsCount.ToString();
                PompesReservoirsLabel.Text = $"{pompesCount}/{reservoirsCount}";
                ClientsCountLabel.Text = clientsCount.ToString();
                DepensesCountLabel.Text = depensesCount.ToString();

                // Update recent items labels
                AchatsRecentsLabel.Text = achatsCount > 0 
                    ? $"{achatsCount} achat(s) enregistré(s)" 
                    : "Aucun achat récent";
                
                PeriodesRecentsLabel.Text = periodesCount > 0 
                    ? $"{periodesCount} période(s) enregistrée(s)" 
                    : "Aucune période récente";

                // Check for alerts (low stock, etc.)
                CheckAlerts(reservoirsCount, pompesCount);
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[HomePage] Error loading dashboard: {ex.Message}");
            MainThread.BeginInvokeOnMainThread(() =>
            {
                AchatsRecentsLabel.Text = "Erreur de chargement";
                PeriodesRecentsLabel.Text = "Erreur de chargement";
            });
        }
    }

    private static async Task<string> SafeGetAsync(HttpClient client, string url)
    {
        try
        {
            return await client.GetStringAsync(url);
        }
        catch
        {
            return "[]";
        }
    }

    private static int CountItems(string json)
    {
        try
        {
            if (string.IsNullOrEmpty(json) || json == "[]")
                return 0;

            // Simple count by counting array elements
            var items = Newtonsoft.Json.JsonConvert.DeserializeObject<List<object>>(json);
            return items?.Count ?? 0;
        }
        catch
        {
            return 0;
        }
    }

    private void CheckAlerts(int reservoirsCount, int pompesCount)
    {
        var alerts = new List<string>();

        if (reservoirsCount == 0)
            alerts.Add("Aucun réservoir configuré");
        if (pompesCount == 0)
            alerts.Add("Aucune pompe configurée");

        if (alerts.Count > 0)
        {
            AlertesLabel.Text = string.Join(", ", alerts);
            AlertesLabel.TextColor = Color.FromArgb("#C62828");
        }
        else
        {
            AlertesLabel.Text = "Aucune alerte critique";
            AlertesLabel.TextColor = Color.FromArgb("#888888");
        }
    }

    #region Navigation Handlers

    private async void OnPeriodesTapped(object? sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync($"//{nameof(PeriodePage)}");
    }

    private async void OnAchatsTapped(object? sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync($"//{nameof(AchatPage)}");
    }

    private async void OnClientsTapped(object? sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync($"//{nameof(ClientPage)}");
    }

    private async void OnProduitsTapped(object? sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync($"//{nameof(ProduitPage)}");
    }

    private async void OnVentesLubTapped(object? sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync($"//{nameof(VenteLubrifiantsEtArticlesPage)}");
    }

    private async void OnDepensesTapped(object? sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync($"//{nameof(DepensePage)}");
    }

    private async void OnReservoirsTapped(object? sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync($"//{nameof(ReservoirPage)}");
    }



    private async void OnPompesTapped(object? sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync($"//{nameof(PompePage)}");
    }

    private async void OnAddSaleButtonTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"//{nameof(VenteLubrifiantsEtArticlesPage)}");
    }

    #endregion
}