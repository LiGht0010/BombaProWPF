using BombaProMax.Models;
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
using Microsoft.Maui.Controls.Shapes;
using Newtonsoft.Json;
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
        await LoadDashboardDataAsync();
    }

    private async Task LoadDashboardDataAsync()
    {
        try
        {
            // Update user status
            var userName = App.CurrentUser?.Name ?? App.user?.Name ?? "Utilisateur";
            UserStatusLabel.Text = $"Connecté en tant que {userName} • Système actif";

            // Use HttpClientFactory for requests
            var httpClient = HttpClientFactory.Create();

            // Load data in parallel
            var reservoirsTask = LoadReservoirsAsync(httpClient);
            var productsTask = LoadNonCarburantProductsAsync(httpClient);
            var achatsTask = SafeGetAsync(httpClient, $"{ApiConfig.Home}/Achats");
            var periodesTask = SafeGetAsync(httpClient, $"{ApiConfig.Home}/Periodes");

            await Task.WhenAll(reservoirsTask, productsTask, achatsTask, periodesTask);

            var achatsCount = CountItems(achatsTask.Result);
            var periodesCount = CountItems(periodesTask.Result);

            // Update UI on main thread
            MainThread.BeginInvokeOnMainThread(() =>
            {
                // Update recent items labels
                AchatsRecentsLabel.Text = achatsCount > 0 
                    ? $"{achatsCount} achat(s) enregistré(s)" 
                    : "Aucun achat récent";
                
                PeriodesRecentsLabel.Text = periodesCount > 0 
                    ? $"{periodesCount} période(s) enregistrée(s)" 
                    : "Aucune période récente";

                // Check for alerts
                CheckAlerts(reservoirsTask.Result);
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

    private async Task<List<ReservoirDto>> LoadReservoirsAsync(HttpClient httpClient)
    {
        try
        {
            var json = await httpClient.GetStringAsync(ApiConfig.Reservoirs);
            var reservoirs = JsonConvert.DeserializeObject<List<ReservoirDto>>(json) ?? [];

            MainThread.BeginInvokeOnMainThread(() =>
            {
                ReservoirsContainer.Children.Clear();

                if (reservoirs.Count == 0)
                {
                    ReservoirsContainer.Children.Add(new Label
                    {
                        Text = "Aucun réservoir configuré",
                        TextColor = Color.FromArgb("#8B939E"),
                        FontSize = 12
                    });
                    return;
                }

                foreach (var reservoir in reservoirs)
                {
                    var card = CreateReservoirCard(reservoir);
                    ReservoirsContainer.Children.Add(card);
                }
            });

            return reservoirs;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[HomePage] Error loading reservoirs: {ex.Message}");
            MainThread.BeginInvokeOnMainThread(() =>
            {
                ReservoirsContainer.Children.Clear();
                ReservoirsContainer.Children.Add(new Label
                {
                    Text = "Erreur de chargement des réservoirs",
                    TextColor = Color.FromArgb("#C62828"),
                    FontSize = 12
                });
            });
            return [];
        }
    }

    private Border CreateReservoirCard(ReservoirDto reservoir)
    {
        var percentage = reservoir.Capacite > 0 
            ? (double)(reservoir.NiveauDeCarburant / reservoir.Capacite) * 100 
            : 0;
        
        // Determine color based on percentage
        var progressColor = percentage switch
        {
            <= 20 => Color.FromArgb("#C62828"), // Red - critical
            <= 40 => Color.FromArgb("#E65100"), // Orange - warning
            <= 60 => Color.FromArgb("#E8A84C"), // Yellow - moderate
            _ => Color.FromArgb("#2E7D32")      // Green - good
        };

        var card = new Border
        {
            BackgroundColor = Color.FromArgb("#e8e8e8"),
            StrokeThickness = 0,
            Padding = new Thickness(15),
            Margin = new Thickness(0, 0, 10, 10),
            WidthRequest = 280,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(8) },
            Shadow = new Shadow
            {
                Brush = new SolidColorBrush(Color.FromArgb("#C5CAD1")),
                Offset = new Point(4, 4),
                Radius = 10,
                Opacity = 0.2f
            }
        };

        var cardStack = new VerticalStackLayout { Spacing = 10 };

        // Header with name (no emoji icon)
        var nameStack = new VerticalStackLayout { VerticalOptions = LayoutOptions.Center };
        nameStack.Children.Add(new Label
        {
            Text = $"Réservoir {reservoir.Numero}",
            FontSize = 14,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#2E2E2E")
        });
        nameStack.Children.Add(new Label
        {
            Text = reservoir.ProduitNom ?? "Non assigné",
            FontSize = 11,
            TextColor = Color.FromArgb("#5A6068")
        });
        cardStack.Children.Add(nameStack);

        // Progress bar
        var progressBg = new Border
        {
            BackgroundColor = Color.FromArgb("#E0E0E0"),
            HeightRequest = 12,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(6) }
        };

        var progressFill = new BoxView
        {
            HeightRequest = 12,
            HorizontalOptions = LayoutOptions.Start,
            CornerRadius = new CornerRadius(6),
            Color = progressColor,
            WidthRequest = Math.Max(0, Math.Min(250, 250 * percentage / 100)) // Max width 250
        };

        progressBg.Content = progressFill;
        cardStack.Children.Add(progressBg);

        // Stats row
        var statsGrid = new Grid
        {
            ColumnDefinitions = [
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star)
            ],
            ColumnSpacing = 8
        };

        // Percentage
        var percentStack = new VerticalStackLayout { HorizontalOptions = LayoutOptions.Center };
        percentStack.Children.Add(new Label
        {
            Text = $"{percentage:F0}%",
            FontSize = 13,
            FontAttributes = FontAttributes.Bold,
            TextColor = progressColor,
            HorizontalOptions = LayoutOptions.Center
        });
        percentStack.Children.Add(new Label
        {
            Text = "NIVEAU",
            FontSize = 9,
            TextColor = Color.FromArgb("#8B939E"),
            HorizontalOptions = LayoutOptions.Center
        });
        statsGrid.SetColumn(percentStack, 0);
        statsGrid.Children.Add(percentStack);

        // Quantity remaining
        var qtyStack = new VerticalStackLayout { HorizontalOptions = LayoutOptions.Center };
        qtyStack.Children.Add(new Label
        {
            Text = $"{reservoir.NiveauDeCarburant:N0}L",
            FontSize = 13,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#2E2E2E"),
            HorizontalOptions = LayoutOptions.Center
        });
        qtyStack.Children.Add(new Label
        {
            Text = "RESTANT",
            FontSize = 9,
            TextColor = Color.FromArgb("#8B939E"),
            HorizontalOptions = LayoutOptions.Center
        });
        statsGrid.SetColumn(qtyStack, 1);
        statsGrid.Children.Add(qtyStack);

        // Capacity
        var capacityStack = new VerticalStackLayout { HorizontalOptions = LayoutOptions.Center };
        capacityStack.Children.Add(new Label
        {
            Text = $"{reservoir.Capacite:N0}L",
            FontSize = 13,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#2E2E2E"),
            HorizontalOptions = LayoutOptions.Center
        });
        capacityStack.Children.Add(new Label
        {
            Text = "CAPACITÉ",
            FontSize = 9,
            TextColor = Color.FromArgb("#8B939E"),
            HorizontalOptions = LayoutOptions.Center
        });
        statsGrid.SetColumn(capacityStack, 2);
        statsGrid.Children.Add(capacityStack);

        cardStack.Children.Add(statsGrid);
        card.Content = cardStack;

        // Add tap gesture to navigate to reservoir page
        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += (s, e) => OnReservoirsTapped(s, (TappedEventArgs)e);
        card.GestureRecognizers.Add(tapGesture);

        return card;
    }

    private async Task LoadNonCarburantProductsAsync(HttpClient httpClient)
    {
        try
        {
            var json = await httpClient.GetStringAsync(ApiConfig.Produits);
            var allProducts = JsonConvert.DeserializeObject<List<ProduitDto>>(json) ?? [];

            // Filter non-carburant products (products that have stock tracking)
            // Exclude CARBURANT category (CategorieID = 1 based on seeded data)
            var nonCarburantProducts = allProducts
                .Where(p => (p.Stock.HasValue || p.StockMinimum.HasValue) && 
                           !string.Equals(p.CategorieNom, "CARBURANT", StringComparison.OrdinalIgnoreCase))
                .ToList();

            // Group products by category
            var productsByCategory = nonCarburantProducts
                .GroupBy(p => p.CategorieNom ?? "Autres")
                .OrderBy(g => g.Key)
                .ToList();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                ProductsStockContainer.Children.Clear();

                if (nonCarburantProducts.Count == 0)
                {
                    ProductsStockContainer.Children.Add(new Label
                    {
                        Text = "Aucun produit avec stock configuré",
                        TextColor = Color.FromArgb("#8B939E"),
                        FontSize = 12
                    });
                    return;
                }

                // Create a section for each category
                foreach (var categoryGroup in productsByCategory)
                {
                    var categorySection = CreateCategorySection(categoryGroup.Key, categoryGroup.ToList());
                    ProductsStockContainer.Children.Add(categorySection);
                }
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[HomePage] Error loading products: {ex.Message}");
            MainThread.BeginInvokeOnMainThread(() =>
            {
                ProductsStockContainer.Children.Clear();
                ProductsStockContainer.Children.Add(new Label
                {
                    Text = "Erreur de chargement des produits",
                    TextColor = Color.FromArgb("#C62828"),
                    FontSize = 12
                });
            });
        }
    }

    private VerticalStackLayout CreateCategorySection(string categoryName, List<ProduitDto> products)
    {
        var section = new VerticalStackLayout
        {
            Spacing = 10,
            Margin = new Thickness(0, 0, 0, 15)
        };

        // Category header
        var headerLabel = new Label
        {
            Text = categoryName.ToUpperInvariant(),
            FontSize = 13,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#5A6068"),
            Margin = new Thickness(0, 0, 0, 5)
        };
        section.Children.Add(headerLabel);

        // Products row (FlexLayout for wrapping)
        var productsRow = new FlexLayout
        {
            Wrap = Microsoft.Maui.Layouts.FlexWrap.Wrap,
            JustifyContent = Microsoft.Maui.Layouts.FlexJustify.Start,
            AlignItems = Microsoft.Maui.Layouts.FlexAlignItems.Start
        };

        foreach (var product in products)
        {
            var card = CreateProductStockCard(product);
            productsRow.Children.Add(card);
        }

        section.Children.Add(productsRow);
        return section;
    }

    private Border CreateProductStockCard(ProduitDto product)
    {
        var stock = product.Stock ?? 0;
        var stockMinimum = product.StockMinimum ?? 0;
        var isLowStock = stockMinimum > 0 && stock <= stockMinimum;

        var card = new Border
        {
            BackgroundColor = Color.FromArgb("#e8e8e8"),
            StrokeThickness = 0,
            Padding = new Thickness(12),
            Margin = new Thickness(0, 0, 10, 10),
            WidthRequest = 160,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(8) },
            Shadow = new Shadow
            {
                Brush = new SolidColorBrush(Color.FromArgb("#D1D5DB")),
                Offset = new Point(3, 3),
                Radius = 8,
                Opacity = 0.15f
            }
        };

        var cardStack = new VerticalStackLayout
        {
            Spacing = 6,
            HorizontalOptions = LayoutOptions.Center
        };

        // Product name (no icon)
        cardStack.Children.Add(new Label
        {
            Text = product.Description ?? product.NumeroProduit,
            FontSize = 12,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#2E2E2E"),
            HorizontalOptions = LayoutOptions.Center,
            HorizontalTextAlignment = TextAlignment.Center,
            MaxLines = 2,
            LineBreakMode = LineBreakMode.TailTruncation
        });

        // Stock value
        cardStack.Children.Add(new Label
        {
            Text = stock.ToString(),
            FontSize = 18,
            FontAttributes = FontAttributes.Bold,
            TextColor = isLowStock ? Color.FromArgb("#C62828") : Color.FromArgb("#667eea"),
            HorizontalOptions = LayoutOptions.Center
        });

        // Unit label
        cardStack.Children.Add(new Label
        {
            Text = "en stock",
            FontSize = 10,
            TextColor = Color.FromArgb("#8B939E"),
            HorizontalOptions = LayoutOptions.Center
        });

        // Low stock indicator (no emoji)
        if (isLowStock)
        {
            var lowStockBorder = new Border
            {
                BackgroundColor = Color.FromArgb("#FFEBEE"),
                StrokeThickness = 0,
                Padding = new Thickness(6, 2),
                HorizontalOptions = LayoutOptions.Center,
                StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(4) }
            };
            lowStockBorder.Content = new Label
            {
                Text = "Stock bas",
                FontSize = 9,
                TextColor = Color.FromArgb("#C62828"),
                HorizontalOptions = LayoutOptions.Center
            };
            cardStack.Children.Add(lowStockBorder);
        }

        card.Content = cardStack;

        // Add tap gesture to navigate to products page
        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += (s, e) => OnProduitsTapped(s, (TappedEventArgs)e);
        card.GestureRecognizers.Add(tapGesture);

        return card;
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

            var items = JsonConvert.DeserializeObject<List<object>>(json);
            return items?.Count ?? 0;
        }
        catch
        {
            return 0;
        }
    }

    private void CheckAlerts(List<ReservoirDto> reservoirs)
    {
        var alerts = new List<string>();

        if (reservoirs.Count == 0)
        {
            alerts.Add("Aucun réservoir configuré");
        }
        else
        {
            // Check for low fuel levels
            var lowFuelReservoirs = reservoirs
                .Where(r => r.Capacite > 0 && (r.NiveauDeCarburant / r.Capacite) <= 0.20m)
                .ToList();

            if (lowFuelReservoirs.Count > 0)
            {
                alerts.Add($"{lowFuelReservoirs.Count} réservoir(s) niveau critique");
            }
        }

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