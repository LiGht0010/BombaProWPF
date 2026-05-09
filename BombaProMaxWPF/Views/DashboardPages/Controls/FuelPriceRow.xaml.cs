using System.Windows;
using System.Windows.Controls;
using BombaProMaxWPF.Models.Forecourt;

namespace BombaProMaxWPF.Views.DashboardPages.Controls;

/// <summary>
/// Single price row used inside the Market Pricing card.
/// </summary>
public partial class FuelPriceRow : UserControl
{
    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(nameof(Label), typeof(string), typeof(FuelPriceRow),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty PriceProperty =
        DependencyProperty.Register(nameof(Price), typeof(decimal), typeof(FuelPriceRow),
            new PropertyMetadata(0m));

    public static readonly DependencyProperty DeltaProperty =
        DependencyProperty.Register(nameof(Delta), typeof(string), typeof(FuelPriceRow),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty DeltaToneProperty =
        DependencyProperty.Register(nameof(DeltaTone), typeof(DeltaTone), typeof(FuelPriceRow),
            new PropertyMetadata(DeltaTone.Neutral));

    public FuelPriceRow()
    {
        InitializeComponent();
    }

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public decimal Price
    {
        get => (decimal)GetValue(PriceProperty);
        set => SetValue(PriceProperty, value);
    }

    public string Delta
    {
        get => (string)GetValue(DeltaProperty);
        set => SetValue(DeltaProperty, value);
    }

    public DeltaTone DeltaTone
    {
        get => (DeltaTone)GetValue(DeltaToneProperty);
        set => SetValue(DeltaToneProperty, value);
    }
}
