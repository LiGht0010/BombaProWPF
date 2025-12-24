using BombaProMax.Models;

namespace BombaProMax.Services;

public interface ILoginRepository
{
    Task<UserDto?> Login(string email, string password);
}
