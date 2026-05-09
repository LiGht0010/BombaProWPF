using System.Windows;
using System.Windows.Controls;
using BombaProMaxWPF.Models.Forecourt;

namespace BombaProMaxWPF.Views.DashboardPages.Controls;

/// <summary>
/// Compact card showing a pump's name, status indicator dot and badge label.
/// </summary>
public partial class PumpStatusTile : UserControl
{
    public static readonly DependencyProperty PumpNameProperty =
        DependencyProperty.Register(nameof(PumpName), typeof(string), typeof(PumpStatusTile),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty StatusProperty =
        DependencyProperty.Register(nameof(Status), typeof(PumpStatus), typeof(PumpStatusTile),
            new PropertyMetadata(PumpStatus.Idle));

    public PumpStatusTile()
    {
        InitializeComponent();
    }

    public string PumpName
    {
        get => (string)GetValue(PumpNameProperty);
        set => SetValue(PumpNameProperty, value);
    }

    public PumpStatus Status
    {
        get => (PumpStatus)GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }
}
