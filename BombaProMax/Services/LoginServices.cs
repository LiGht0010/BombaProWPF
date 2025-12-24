using BombaProMax.Models;
using System.Net.Http.Json;

namespace BombaProMax.Services;

public class LoginServices : ILoginRepository
{
    public async Task<UserDto?> Login(string email, string password)
    {
        try
        {
            var client = new HttpClient();
            string loginUrl = $"{ApiConfig.Users}/Login/{email}/{password}";
            client.BaseAddress = new Uri(loginUrl);
            HttpResponseMessage response = await client.GetAsync(client.BaseAddress);
            if (response.IsSuccessStatusCode)
            {
                var user = await response.Content.ReadFromJsonAsync<UserDto>();
                return user;
            }
            else
            {
                if (response.ReasonPhrase == "Not Found")
                {
                    await Shell.Current.DisplayAlert("Error", response.ReasonPhrase, "Ok!");
                }
                return null;
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
            return null;
        }
    }
}