using BombaProMax.Models;
using BombaProMax.Services;
using System.Diagnostics;

namespace BombaProMax
{
    public partial class App : Application
    {
        public static UserDto? user;
        public static UserDto? CurrentUser { get; set; }
        
        private static readonly string CrashLogPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "BombaProMax_CrashLog.txt");

        public App(IServiceProvider serviceProvider)
        {
            // Set up global exception handlers FIRST
            SetupExceptionHandlers();
            
            InitializeComponent();

            // Initialize API configuration from saved preferences
            ApiConfig.Initialize();

            // Force light theme at MAUI level
            Application.Current!.UserAppTheme = AppTheme.Light;

            // Keep AppShell as MainPage, pass serviceProvider for DI
            MainPage = new AppShell(serviceProvider);
            
            LogMessage("App initialized successfully");
        }

        private void SetupExceptionHandlers()
        {
            // Handle exceptions from all threads
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                var ex = args.ExceptionObject as Exception;
                LogCrash("AppDomain.UnhandledException", ex);
            };

            // Handle exceptions from async void methods
            TaskScheduler.UnobservedTaskException += (sender, args) =>
            {
                LogCrash("TaskScheduler.UnobservedTaskException", args.Exception);
                args.SetObserved(); // Prevent app termination
            };

            // Handle first-chance exceptions (for debugging)
            AppDomain.CurrentDomain.FirstChanceException += (sender, args) =>
            {
                // Only log critical exceptions to avoid spam
                if (args.Exception is OutOfMemoryException or 
                    StackOverflowException or 
                    AccessViolationException)
                {
                    LogCrash("FirstChanceException (Critical)", args.Exception);
                }
            };
        }

        private static void LogCrash(string source, Exception? ex)
        {
            try
            {
                var message = $"""
                    
                    =====================================
                    CRASH: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
                    Source: {source}
                    =====================================
                    Exception Type: {ex?.GetType().FullName}
                    Message: {ex?.Message}
                    
                    Stack Trace:
                    {ex?.StackTrace}
                    
                    Inner Exception:
                    {ex?.InnerException?.Message}
                    {ex?.InnerException?.StackTrace}
                    =====================================
                    
                    """;

                File.AppendAllText(CrashLogPath, message);
                Debug.WriteLine(message);
            }
            catch
            {
                // Can't even log - give up silently
            }
        }

        public static void LogMessage(string message)
        {
            try
            {
                var logEntry = $"[{DateTime.Now:HH:mm:ss}] {message}\n";
                File.AppendAllText(CrashLogPath, logEntry);
                Debug.WriteLine(logEntry);
            }
            catch
            {
                // Ignore logging failures
            }
        }

        public static string GetCrashLogPath() => CrashLogPath;
    }
}