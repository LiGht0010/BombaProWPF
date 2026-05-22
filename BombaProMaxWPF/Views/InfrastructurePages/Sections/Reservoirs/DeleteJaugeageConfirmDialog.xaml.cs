using BombaProMaxWPF.Localization;
using System.Windows;
using Wpf.Ui.Controls;

namespace BombaProMaxWPF.Views.InfrastructurePages.Sections.Reservoirs;

public partial class DeleteJaugeageConfirmDialog : FluentWindow
{
    public DeleteJaugeageConfirmDialog(string jaugeageNumero)
    {
        InitializeComponent();

        var lang = LanguageManager.Instance;
        Apply(lang, jaugeageNumero);

        lang.LanguageChanged += (_, _) => Apply(lang, jaugeageNumero);
    }

    private void Apply(LanguageManager lang, string numero)
    {
        Title = lang["JaugDeleteConfirmTitle"];
        TitleText.Text = lang["JaugDeleteConfirmTitle"];
        MessageText.Text = string.Format(lang["JaugDeleteConfirmMessage"], numero);
        CancelButton.Content = lang["JaugDeleteConfirmNo"];
        ConfirmButton.Content = lang["JaugDeleteConfirmYes"];
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void OnConfirmClick(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }
}
