using BombaProMax.Models;

namespace BombaProMax
{
    public partial class App : Application
    {
        public static UserDto? user;
        public static UserDto? CurrentUser { get; set; }

        public App(IServiceProvider serviceProvider)
        {
            InitializeComponent();

            // Force light theme
            //Application.Current!.UserAppTheme = AppTheme.Light;

            // Keep AppShell as MainPage, pass serviceProvider for DI
            MainPage = new AppShell(serviceProvider);
        }
    }
}