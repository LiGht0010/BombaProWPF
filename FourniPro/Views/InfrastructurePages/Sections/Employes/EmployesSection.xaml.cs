using FourniPro.ViewModels;
using System.Windows.Controls;

namespace FourniPro.Views.InfrastructurePages.Sections.Employes;

public partial class EmployesSection : UserControl
{
    public EmployesSectionViewModel ViewModel { get; } = new();

    public EmployesSection()
    {
        InitializeComponent();
        DataContext = ViewModel;
    }
}
