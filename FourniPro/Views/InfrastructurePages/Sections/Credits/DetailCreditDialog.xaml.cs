using FourniPro.Models;
using System.Windows;
using Wpf.Ui.Controls;

namespace FourniPro.Views.InfrastructurePages.Sections.Credits;

public partial class DetailCreditDialog : FluentWindow
{
    /// <summary>
    /// Set to <see langword="true"/> when the user clicks "Modifier".
    /// The caller opens <see cref="EditCreditDialog"/> immediately after this dialog closes.
    /// </summary>
    public bool ShouldEdit { get; private set; }

    public DetailCreditDialog(CreditDto credit)
    {
        InitializeComponent();
        DataContext = credit;
    }

    private void OnCloseClick(object sender, RoutedEventArgs e) => Close();

    private void OnEditClick(object sender, RoutedEventArgs e)
    {
        ShouldEdit = true;
        Close();
    }
}
