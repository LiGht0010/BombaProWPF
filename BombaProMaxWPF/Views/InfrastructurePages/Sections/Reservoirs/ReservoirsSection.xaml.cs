using System.Windows.Controls;
using BombaProMaxWPF.ViewModels;

namespace BombaProMaxWPF.Views.InfrastructurePages.Sections.Reservoirs;

public partial class ReservoirsSection : UserControl
{
    public ReservoirsSection()
    {
        InitializeComponent();
        DataContext = new ReservoirsSectionViewModel();
    }
}

