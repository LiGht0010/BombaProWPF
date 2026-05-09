using System.Windows.Controls;
using BombaProMaxWPF.ViewModels;

namespace BombaProMaxWPF.Views.DashboardPages;

public partial class DashboardView : UserControl
{
    public DashboardView()
    {
        InitializeComponent();
        DataContext = new ForecourtDashboardViewModel();
    }
}
