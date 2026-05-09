using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BombaProMaxWPF.Views.DashboardPages.Controls;

/// <summary>
/// Donut-style ring chart drawn with a single <see cref="System.Windows.Shapes.Path"/>
/// and an <see cref="System.Windows.Media.ArcSegment"/>. The arc end-point is recomputed
/// whenever <see cref="Percent"/> changes.
/// </summary>
public partial class RingProgress : UserControl
{
    public static readonly DependencyProperty PercentProperty =
        DependencyProperty.Register(nameof(Percent), typeof(double), typeof(RingProgress),
            new PropertyMetadata(0.0, OnGeometryChanged));

    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(nameof(Label), typeof(string), typeof(RingProgress),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty RingBrushProperty =
        DependencyProperty.Register(nameof(RingBrush), typeof(Brush), typeof(RingProgress),
            new PropertyMetadata(null));

    public static readonly DependencyProperty IsLowProperty =
        DependencyProperty.Register(nameof(IsLow), typeof(bool), typeof(RingProgress),
            new PropertyMetadata(false));

    private static readonly DependencyPropertyKey ArcEndPointPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(ArcEndPoint), typeof(Point), typeof(RingProgress),
            new PropertyMetadata(new Point(60, 8)));

    public static readonly DependencyProperty ArcEndPointProperty = ArcEndPointPropertyKey.DependencyProperty;

    private static readonly DependencyPropertyKey IsLargeArcPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(IsLargeArc), typeof(bool), typeof(RingProgress),
            new PropertyMetadata(false));

    public static readonly DependencyProperty IsLargeArcProperty = IsLargeArcPropertyKey.DependencyProperty;

    public RingProgress()
    {
        InitializeComponent();
        UpdateArc();
    }

    public double Percent
    {
        get => (double)GetValue(PercentProperty);
        set => SetValue(PercentProperty, value);
    }

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public Brush? RingBrush
    {
        get => (Brush?)GetValue(RingBrushProperty);
        set => SetValue(RingBrushProperty, value);
    }

    public bool IsLow
    {
        get => (bool)GetValue(IsLowProperty);
        set => SetValue(IsLowProperty, value);
    }

    public Point ArcEndPoint
    {
        get => (Point)GetValue(ArcEndPointProperty);
        private set => SetValue(ArcEndPointPropertyKey, value);
    }

    public bool IsLargeArc
    {
        get => (bool)GetValue(IsLargeArcProperty);
        private set => SetValue(IsLargeArcPropertyKey, value);
    }

    private static void OnGeometryChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((RingProgress)d).UpdateArc();
    }

    private void UpdateArc()
    {
        // Geometry assumes a 120×120 box, ring centered at (60,60), radius 56.
        const double cx = 60;
        const double cy = 60;
        const double radius = 56;

        var clamped = Math.Max(0, Math.Min(100, Percent));
        // 0% => start point (top); avoid a full closed arc rendering as nothing.
        var angleDeg = clamped >= 100 ? 359.999 : clamped * 3.6;
        var angleRad = (angleDeg - 90) * Math.PI / 180.0;

        var x = cx + radius * Math.Cos(angleRad);
        var y = cy + radius * Math.Sin(angleRad);

        ArcEndPoint = new Point(x, y);
        IsLargeArc = clamped > 50;
    }
}
