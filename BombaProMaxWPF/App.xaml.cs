using BombaProMaxWPF.Models;
using BombaProMaxWPF.Services;
using System.Windows;

namespace BombaProMaxWPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static UserDto? user;
        public static UserDto? CurrentUser { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ApiConfig.Initialize();
        }
    }
}
