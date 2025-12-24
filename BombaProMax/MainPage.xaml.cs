namespace BombaProMax;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private void OnButtonTapped(object sender, EventArgs e)
    {
        // Handle button tap
        DisplayAlert("Button Clicked", "Neumorphic button was tapped!", "OK");
    }
}