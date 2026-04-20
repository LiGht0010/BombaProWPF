using System.ComponentModel;
using System.Globalization;
using System.Threading;
using BombaProMaxWPF.Resources;

namespace BombaProMaxWPF.Localization
{
    /// <summary>
    /// App-wide language manager. Holds the current culture and notifies all
    /// <see cref="TrExtension"/> bindings when the language changes so every
    /// translated UI element refreshes at once.
    /// </summary>
    public sealed class LanguageManager : INotifyPropertyChanged
    {
        public static LanguageManager Instance { get; } = new();

        private CultureInfo _culture = new("fr");

        private LanguageManager() { }

        public CultureInfo CurrentCulture => _culture;

        /// <summary>
        /// XAML-friendly indexer used by <see cref="TrExtension"/> to fetch a
        /// localized string for the current culture.
        /// </summary>
        public string this[string key]
        {
            get
            {
                if (string.IsNullOrEmpty(key))
                {
                    return string.Empty;
                }

                return Strings.ResourceManager.GetString(key, _culture) ?? $"!{key}!";
            }
        }

        public void SetLanguage(string cultureCode)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(cultureCode);

            var newCulture = new CultureInfo(cultureCode);
            if (string.Equals(newCulture.Name, _culture.Name, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            _culture = newCulture;
            Strings.Culture = newCulture;
            Thread.CurrentThread.CurrentUICulture = newCulture;
            CultureInfo.DefaultThreadCurrentUICulture = newCulture;

            // "Item[]" is the WPF convention to invalidate all indexer bindings.
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
            LanguageChanged?.Invoke(this, EventArgs.Empty);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler? LanguageChanged;
    }
}
