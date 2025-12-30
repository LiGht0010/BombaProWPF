using BombaProMax.Views;
using BombaProMax.Views.AchatViews;
using BombaProMax.Views.CamionViews;
using BombaProMax.Views.ChauffeurViews;
using BombaProMax.Views.CiterneViews;
using BombaProMax.Views.ClientViews;
using BombaProMax.Views.DashboardViews;
using BombaProMax.Views.DepenseViews;
using BombaProMax.Views.EmployeViews;
using BombaProMax.Views.FactureViews;
using BombaProMax.Views.FournisseurViews;
using BombaProMax.Views.JaugeageViews;
using BombaProMax.Views.PeriodeViews;
using BombaProMax.Views.PompeViews;
using BombaProMax.Views.ProduitViews;
using BombaProMax.Views.ReservoirViews;
using BombaProMax.Views.User;
using BombaProMax.Views.VenteLubEtArticles;
using BombaProMax.Services;
using Microsoft.Maui.Controls.Shapes;
using BombaProMax.Views.ReservoirCalibrationViews;
using BombaProMax.Views.RapportViews;
using BombaProMax.Views.ServiceViews;
using BombaProMax.Views.VenteServiceViews;

namespace BombaProMax
{
    public partial class AppShell : Shell
    {
        // Navigation history stack for back button
        private readonly Stack<string> _navigationHistory = new();
        private string? _currentRoute;
        private bool _isNavigatingBack;
        
        // Journée navigation service
        private readonly JourneeNavigationService _journeeService;

        // User display labels (created in code for dynamic updates)
        private Label _userInitialsLabel = null!;
        private Label _userNameLabel = null!;
        private Label _userRoleLabel = null!;

        public AppShell(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            
            // Create the flyout footer with user info
            CreateFlyoutFooter();
            
            // Get the JourneeNavigationService from DI
            _journeeService = serviceProvider.GetRequiredService<JourneeNavigationService>();

            // Register routes for navigation
            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
            Routing.RegisterRoute(nameof(HomePage), typeof(HomePage));
            Routing.RegisterRoute(nameof(User), typeof(User));
            Routing.RegisterRoute(nameof(AchatPage), typeof(AchatPage));
            Routing.RegisterRoute(nameof(ClientPage), typeof(ClientPage));
            Routing.RegisterRoute(nameof(ProduitPage), typeof(ProduitPage));
            Routing.RegisterRoute(nameof(ReservoirPage), typeof(ReservoirPage));
            Routing.RegisterRoute(nameof(ChauffeurPage), typeof(ChauffeurPage));
            Routing.RegisterRoute(nameof(CamionPage), typeof(CamionPage));
            Routing.RegisterRoute(nameof(CiternePage), typeof(CiternePage));
            Routing.RegisterRoute(nameof(FournisseurPage), typeof(FournisseurPage));
            Routing.RegisterRoute(nameof(EmployePage), typeof(EmployePage));
            Routing.RegisterRoute(nameof(PompePage), typeof(PompePage));
            Routing.RegisterRoute(nameof(PeriodePage), typeof(PeriodePage));
            Routing.RegisterRoute(nameof(VenteLubrifiantsEtArticlesPage), typeof(VenteLubrifiantsEtArticlesPage));
            Routing.RegisterRoute(nameof(DepensePage), typeof(DepensePage));
            Routing.RegisterRoute(nameof(FacturationPage), typeof(FacturationPage));
            Routing.RegisterRoute(nameof(FactureEtBL), typeof(FactureEtBL));
            Routing.RegisterRoute(nameof(AboutPage), typeof(AboutPage));
            Routing.RegisterRoute(nameof(ContactPage), typeof(ContactPage));
            Routing.RegisterRoute(nameof(ClientCreditManagement), typeof(ClientCreditManagement));
            Routing.RegisterRoute(nameof(ReservoirCalibrationPage), typeof(ReservoirCalibrationPage));
            Routing.RegisterRoute(nameof(JaugeagePage), typeof(JaugeagePage));
            Routing.RegisterRoute(nameof(DashboardPage), typeof(DashboardPage));
            Routing.RegisterRoute(nameof(RapportPage), typeof(RapportPage));
            Routing.RegisterRoute(nameof(ServicePage), typeof(ServicePage));
            Routing.RegisterRoute(nameof(VenteServicePage), typeof(VenteServicePage));

            // Set ShellContent pages from DI
            LoginShellContent.Content = serviceProvider.GetRequiredService<LoginPage>();
            HomeShellContent.Content = serviceProvider.GetRequiredService<HomePage>();
            UsersShellContent.Content = serviceProvider.GetRequiredService<User>();
            AchatShellContent.Content = serviceProvider.GetRequiredService<AchatPage>();
            ClientShellContent.Content = serviceProvider.GetRequiredService<ClientPage>();
            ProductsShellContent.Content = serviceProvider.GetRequiredService<ProduitPage>();
            ReservoirShellContent.Content = serviceProvider.GetRequiredService<ReservoirPage>();
            ChauffeurShellContent.Content = serviceProvider.GetRequiredService<ChauffeurPage>();
            CamionShellContent.Content = serviceProvider.GetRequiredService<CamionPage>();
            CiterneShellContent.Content = serviceProvider.GetRequiredService<CiternePage>();
            FournisseurShellContent.Content = serviceProvider.GetRequiredService<FournisseurPage>();
            EmployeShellContent.Content = serviceProvider.GetRequiredService<EmployePage>();
            PompeShellContent.Content = serviceProvider.GetRequiredService<PompePage>();
            PeriodeShellContent.Content = serviceProvider.GetRequiredService<PeriodePage>();
            VenteLubShellContent.Content = serviceProvider.GetRequiredService<VenteLubrifiantsEtArticlesPage>();
            DepenseShellContent.Content = serviceProvider.GetRequiredService<DepensePage>();
            ReservoirCalibrationContent.Content = serviceProvider.GetRequiredService<ReservoirCalibrationPage>();
            JaugeageShellContent.Content = serviceProvider.GetRequiredService<JaugeagePage>();
            DashboardShellContent.Content = serviceProvider.GetRequiredService<DashboardPage>();
            RapportShellContent.Content = serviceProvider.GetRequiredService<RapportPage>();
            ServiceShellContent.Content = serviceProvider.GetRequiredService<ServicePage>();
            VenteServiceShellContent.Content = serviceProvider.GetRequiredService<VenteServicePage>();

            // Navigate to LoginPage on startup
            Dispatcher.Dispatch(async () =>
            {
                await GoToAsync("//LoginPage");
            });
        }

        #region Flyout Footer Creation

        /// <summary>
        /// Creates the flyout footer with user info programmatically.
        /// </summary>
        private void CreateFlyoutFooter()
        {
            // User initials label
            _userInitialsLabel = new Label
            {
                Text = "U",
                FontSize = 16,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center
            };

            // User avatar border with gradient background
            var avatarBorder = new Border
            {
                WidthRequest = 44,
                HeightRequest = 44,
                StrokeThickness = 0,
                StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(22) },
                Background = new LinearGradientBrush
                {
                    EndPoint = new Point(1, 1),
                    GradientStops = new GradientStopCollection
                    {
                        new GradientStop { Color = Color.FromArgb("#818CF8"), Offset = 0.0f },
                        new GradientStop { Color = Color.FromArgb("#A855F7"), Offset = 1.0f }
                    }
                },
                Content = _userInitialsLabel
            };

            // User name label
            _userNameLabel = new Label
            {
                Text = "Utilisateur",
                TextColor = Color.FromArgb("#334155"),
                FontSize = 14,
                FontAttributes = FontAttributes.Bold
            };

            // User role label
            _userRoleLabel = new Label
            {
                Text = "Connecté",
                TextColor = Color.FromArgb("#64748B"),
                FontSize = 12
            };

            // User details stack
            var userDetailsStack = new VerticalStackLayout
            {
                VerticalOptions = LayoutOptions.Center,
                Spacing = 2,
                Children = { _userNameLabel, _userRoleLabel }
            };

            // User info grid
            var userInfoGrid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Star }
                },
                ColumnSpacing = 12,
                Padding = new Thickness(4)
            };
            userInfoGrid.Add(avatarBorder, 0);
            userInfoGrid.Add(userDetailsStack, 1);

            // Logout button
            var logoutButton = new Button
            {
                Text = "Déconnexion",
                BackgroundColor = Colors.Transparent,
                TextColor = Color.FromArgb("#EF4444"),
                FontSize = 14,
                FontAttributes = FontAttributes.Bold
            };
            logoutButton.Clicked += OnLogoutClicked;

            // Logout button border
            var logoutBorder = new Border
            {
                Padding = 0,
                HeightRequest = 48,
                BackgroundColor = Color.FromArgb("#FEF2F2"),
                StrokeThickness = 1,
                Stroke = Color.FromArgb("#FECACA"),
                StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(12) },
                Content = logoutButton
            };

            // Separator
            var separator = new BoxView
            {
                Color = Color.FromArgb("#E2E8F0"),
                HeightRequest = 1,
                Margin = new Thickness(0, 0, 0, 4)
            };

            // Main footer stack
            var footerStack = new VerticalStackLayout
            {
                Padding = new Thickness(16, 20),
                Spacing = 16,
                BackgroundColor = Color.FromArgb("#F8FAFC"),
                Children = { separator, userInfoGrid, logoutBorder }
            };

            FlyoutFooter = footerStack;
        }

        #endregion

        #region User Display Update

        /// <summary>
        /// Updates the user display in the flyout footer.
        /// Call this method after login to refresh the user information.
        /// </summary>
        public void UpdateUserDisplay()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var currentUser = App.CurrentUser;
                if (currentUser != null)
                {
                    // Get initials from user name
                    var initials = GetInitials(currentUser.Name);
                    var role = currentUser.IsSuperAdmin ? "Super Admin" : 
                               currentUser.IsAdmin ? "Administrateur" : "Utilisateur";

                    // Update the labels directly
                    _userInitialsLabel.Text = initials;
                    _userNameLabel.Text = currentUser.Name ?? "Utilisateur";
                    _userRoleLabel.Text = role;
                }
                else
                {
                    // Reset to default values
                    _userInitialsLabel.Text = "U";
                    _userNameLabel.Text = "Utilisateur";
                    _userRoleLabel.Text = "Connecté";
                }
            });
        }

        private static string GetInitials(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "U";

            var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
                return $"{parts[0][0]}{parts[1][0]}".ToUpper();
            
            return name.Length >= 2 ? name[..2].ToUpper() : name.ToUpper();
        }

        #endregion

        #region Navigation History Management

        /// <summary>
        /// Handles Shell navigation events to track navigation history.
        /// </summary>
        private void OnShellNavigated(object? sender, ShellNavigatedEventArgs e)
        {
            var newRoute = e.Current?.Location?.ToString() ?? "";

            // Don't track if navigating back or if it's the same route
            if (_isNavigatingBack)
            {
                _isNavigatingBack = false;
                _currentRoute = newRoute;
                return;
            }

            // Don't add login page to history
            if (newRoute.Contains("LoginPage", StringComparison.OrdinalIgnoreCase))
            {
                _navigationHistory.Clear();
                _currentRoute = newRoute;
                return;
            }

            // Push current route to history before changing
            if (!string.IsNullOrEmpty(_currentRoute) && 
                !_currentRoute.Contains("LoginPage", StringComparison.OrdinalIgnoreCase))
            {
                _navigationHistory.Push(_currentRoute);
            }

            _currentRoute = newRoute;

            // Limit history to prevent memory issues
            while (_navigationHistory.Count > 20)
            {
                var tempStack = new Stack<string>();
                while (_navigationHistory.Count > 1)
                {
                    tempStack.Push(_navigationHistory.Pop());
                }
                _navigationHistory.Pop();
                while (tempStack.Count > 0)
                {
                    _navigationHistory.Push(tempStack.Pop());
                }
            }

            System.Diagnostics.Debug.WriteLine($"[Navigation] Current: {newRoute}, History count: {_navigationHistory.Count}");
        }

        /// <summary>
        /// Handles Shell navigating events to protect against manual navigation during journée
        /// and to fix navigation issues when coming from sub-pages.
        /// </summary>
        private async void OnShellNavigating(object? sender, ShellNavigatingEventArgs e)
        {
            // Guard against null service (can happen during initialization)
            if (_journeeService == null)
                return;

            var targetRoute = e.Target?.Location?.ToString() ?? "";
            var currentRoute = _currentRoute ?? "";
            
            // Detect if this is an absolute route navigation (from flyout menu)
            // and if we're currently on a sub-page (like ClientCreditManagement)
            bool isAbsoluteNavigation = targetRoute.StartsWith("//");
            bool isOnSubPage = currentRoute.Contains("ClientCreditManagement") || 
                              currentRoute.Contains("FactureEtBL") ||
                              currentRoute.Contains("FacturationPage");
            
            // If navigating from a sub-page to a flyout item, we need to pop the navigation stack first
            if (isAbsoluteNavigation && isOnSubPage && e.Source == ShellNavigationSource.ShellItemChanged)
            {
                System.Diagnostics.Debug.WriteLine($"[Navigation] Detected flyout navigation from sub-page. Clearing stack.");
                
                // Cancel the current navigation
                e.Cancel();
                
                // Pop to root first, then navigate to the target
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    try
                    {
                        // Pop all pages from the navigation stack
                        await Shell.Current.Navigation.PopToRootAsync(false);
                        
                        // Small delay to ensure stack is cleared
                        await Task.Delay(50);
                        
                        // Now navigate to the target
                        await Shell.Current.GoToAsync(targetRoute);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[Navigation] Error during stack clearing: {ex.Message}");
                        // Fallback: try direct navigation
                        await Shell.Current.GoToAsync(targetRoute);
                    }
                });
                
                return;
            }

            // If journée is active, intercept navigation attempts
            if (_journeeService.IsJourneeActive)
            {
                // Allow the journée service to control navigation
                if (!_journeeService.IsRouteAllowedDuringJournee(targetRoute))
                {
                    // Cancel the navigation
                    e.Cancel();
                    
                    // Show a message to the user
                    await DisplayAlert(
                        "Journée en cours",
                        "Vous devez terminer la journée en cours avant de naviguer vers d'autres pages. Utilisez les boutons Suivant/Passer pour progresser.",
                        "OK");
                }
            }
        }

        /// <summary>
        /// Navigate back to the previous page in history.
        /// </summary>
        public async Task NavigateBackAsync()
        {
            if (_navigationHistory.Count > 0)
            {
                _isNavigatingBack = true;
                var previousRoute = _navigationHistory.Pop();
                System.Diagnostics.Debug.WriteLine($"[Navigation] Going back to: {previousRoute}");
                await GoToAsync(previousRoute);
            }
            else
            {
                _isNavigatingBack = true;
                await GoToAsync("//HomePage");
            }
        }

        public bool CanNavigateBack => _navigationHistory.Count > 0;
        public string? PeekPreviousRoute => _navigationHistory.Count > 0 ? _navigationHistory.Peek() : null;

        public void ClearNavigationHistory()
        {
            _navigationHistory.Clear();
            _currentRoute = null;
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the "Démarrer la journée" menu item click.
        /// </summary>
        private async void OnStartJourneeClicked(object? sender, EventArgs e)
        {
            // Hide the flyout
            Shell.Current.FlyoutIsPresented = false;
            
            // Start the journée workflow
            await _journeeService.StartJourneeAsync();
        }

        private async void OnUserClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync($"//{nameof(User)}");
        }

        private async void OnLogoutClicked(object? sender, EventArgs e)
        {
            bool confirm = await DisplayAlert("Déconnexion", "Voulez-vous vraiment vous déconnecter?", "Oui", "Non");
            if (confirm)
            {
                App.CurrentUser = null;
                UpdateUserDisplay(); // Reset the user display
                ClearNavigationHistory();
                Shell.Current.FlyoutIsPresented = false;
                Shell.SetNavBarIsVisible(this, false);
                await Shell.Current.GoToAsync("//LoginPage");
            }
        }

        #endregion
    }
}