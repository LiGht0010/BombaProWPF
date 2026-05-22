using BombaProMaxWPF.Localization;
using System.Windows;
using Wpf.Ui.Controls;

namespace BombaProMaxWPF.Views.InfrastructurePages.Sections.Reservoirs;

public partial class DeleteReservoirConfirmDialog : FluentWindow
{
    public DeleteReservoirConfirmDialog(string reservoirNumero)
    {
        InitializeComponent();

        var lang = LanguageManager.Instance;
        Title = lang["ResDeleteConfirmTitle"];
        TitleText.Text = lang["ResDeleteConfirmTitle"];
        MessageText.Text = string.Format(lang["ResDeleteConfirmMessage"], reservoirNumero);
        CancelButton.Content = lang["ResDeleteConfirmNo"];
        ConfirmButton.Content = lang["ResDeleteConfirmYes"];

        // Re-apply strings when language changes while dialog is open
        lang.LanguageChanged += (_, _) =>
        {
            Title = lang["ResDeleteConfirmTitle"];
            TitleText.Text = lang["ResDeleteConfirmTitle"];
            MessageText.Text = string.Format(lang["ResDeleteConfirmMessage"], reservoirNumero);
            CancelButton.Content = lang["ResDeleteConfirmNo"];
            ConfirmButton.Content = lang["ResDeleteConfirmYes"];
        };
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
