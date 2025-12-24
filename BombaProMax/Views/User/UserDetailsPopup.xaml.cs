using BombaProMax.Models;
using CommunityToolkit.Maui.Views;

namespace BombaProMax.Views.User;

public partial class UserDetailsPopup : Popup
{
    public UserDetailsPopup(UserDto user)
    {
        InitializeComponent();
        
        // Set UserDto as BindingContext - all display properties are computed on UserDto
        BindingContext = user;
    }

    private void OnCloseClicked(object sender, EventArgs e)
    {
        Close();
    }
}