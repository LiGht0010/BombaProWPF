using BombaProMax.Models;
using CommunityToolkit.Maui.Views;

namespace BombaProMax.Views.JaugeageViews;

public partial class JaugeageEditPopup : Popup
{
    private readonly JaugeageDto _jaugeage;

    public JaugeageEditPopup(JaugeageDto jaugeage)
    {
        InitializeComponent();
        _jaugeage = jaugeage;

        // Update header with jaugeage numero
        HeaderLabel.Text = $"Modifier: {jaugeage.NumeroJaugeage}";
    }

    private void OnCancelClicked(object sender, EventArgs e)
    {
        Close(null);
    }

    private void OnSaveClicked(object sender, EventArgs e)
    {
        // TODO: Implement save logic
        Close(null);
    }
}