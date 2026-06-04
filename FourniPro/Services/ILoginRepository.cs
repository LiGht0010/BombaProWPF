using FourniPro.Models;

namespace FourniPro.Services;

public interface ILoginRepository
{
    Task<UserDto?> Login(string email, string password);
}
