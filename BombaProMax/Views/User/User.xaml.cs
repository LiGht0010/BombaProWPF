using BombaProMax.Models;
using BombaProMax.Services;
using BombaProMax.ViewModels;

namespace BombaProMax.Views.User;

public partial class User : ContentPage
{
    private readonly UserViewModel _viewModel;

    public User(UserViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadUsersCommand.ExecuteAsync(null);
    }

    private async void OnAddUserClicked(object sender, EventArgs e)
    {
        await _viewModel.AddUserCommand.ExecuteAsync(null);
    }

    private async void OnEditUserClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is UserDto user)
        {
            await _viewModel.EditUserCommand.ExecuteAsync(user);
        }
    }

    private async void OnDetailsClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is UserDto selectedUser)
        {
            await _viewModel.ShowUserDetailsCommand.ExecuteAsync(selectedUser);
        }
    }

    private async void OnDeleteUserClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is UserDto user)
        {
            await _viewModel.DeleteUserCommand.ExecuteAsync(user);
        }
    }

    private async void OnDetailsSwipeInvoked(object sender, EventArgs e)
    {
        if (sender is SwipeItem swipeItem && swipeItem.CommandParameter is UserDto user)
        {
            await _viewModel.ShowUserDetailsCommand.ExecuteAsync(user);
        }
    }

    private async void OnEditSwipeInvoked(object sender, EventArgs e)
    {
        if (sender is SwipeItem swipeItem && swipeItem.CommandParameter is UserDto user)
        {
            await _viewModel.EditUserCommand.ExecuteAsync(user);
        }
    }

    private async void OnDeleteSwipeInvoked(object sender, EventArgs e)
    {
        if (sender is SwipeItem swipeItem && swipeItem.CommandParameter is UserDto user)
        {
            await _viewModel.DeleteUserCommand.ExecuteAsync(user);
        }
    }

    private async void OnRowTapped(object sender, TappedEventArgs e)
    {
        if (sender is Grid grid && grid.BindingContext is UserDto user)
        {
            await _viewModel.ShowUserDetailsCommand.ExecuteAsync(user);
        }
    }
}