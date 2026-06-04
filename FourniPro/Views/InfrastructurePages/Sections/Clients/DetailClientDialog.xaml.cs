using FourniPro.Models;
using System.Windows;
using Wpf.Ui.Controls;

namespace FourniPro.Views.InfrastructurePages.Sections.Clients;

public partial class DetailClientDialog : FluentWindow
{
    /// <summary>
    /// Set to <see langword="true"/> when the user clicks "Modifier".
    /// The caller opens <see cref="EditClientDialog"/> immediately after this dialog closes.
    /// </summary>
    public bool ShouldEdit { get; private set; }

    public DetailClientDialog(ClientDto dto)
    {
        InitializeComponent();
        DataContext = dto;
    }

    private void OnCloseClick(object sender, RoutedEventArgs e) => Close();

    private void OnEditClick(object sender, RoutedEventArgs e)
    {
        ShouldEdit = true;
        Close();
    }
}
