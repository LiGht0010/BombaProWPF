using FourniPro.Models;
using Wpf.Ui.Controls;

namespace FourniPro.Views.InfrastructurePages.Sections.Employes;

public partial class DetailEmployeDialog : FluentWindow
{
    public bool ShouldEdit { get; private set; }

    public DetailEmployeDialog(EmployeDto dto)
    {
        InitializeComponent();
        DataContext = dto;
    }

    private void OnCloseClick(object sender, System.Windows.RoutedEventArgs e) => Close();

    private void OnEditClick(object sender, System.Windows.RoutedEventArgs e)
    {
        ShouldEdit = true;
        Close();
    }
}
