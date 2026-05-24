using BombaProMaxWPF.Models;
using System.Windows;
using Wpf.Ui.Controls;

namespace BombaProMaxWPF.Views.InfrastructurePages.Sections.Services;

public partial class DetailServiceDialog : FluentWindow
{
    /// <summary>
    /// Set to true when the user clicks "Modifier" — the caller (ServicesSectionViewModel)
    /// will open the EditServiceDialog immediately after this dialog closes.
    /// </summary>
    public bool ShouldEdit { get; private set; }

    public DetailServiceDialog(ServiceDto service)
    {
        InitializeComponent();
        DataContext = service;
    }

    private void OnCloseClick(object sender, RoutedEventArgs e) => Close();

    private void OnEditClick(object sender, RoutedEventArgs e)
    {
        ShouldEdit = true;
        Close();
    }
}
