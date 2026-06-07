using FourniPro.Models;
using System.Windows;
using Wpf.Ui.Controls;

namespace FourniPro.Views.InfrastructurePages.Sections.Avoirs;

public partial class DetailAvoirDialog : FluentWindow
{
    public bool ShouldEdit { get; private set; }

    public DetailAvoirDialog(AvoirDto dto)
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
