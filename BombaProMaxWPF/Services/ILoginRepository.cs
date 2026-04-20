using BombaProMaxWPF.Models;

namespace BombaProMaxWPF.Services;

public interface ILoginRepository
{
    Task<UserDto?> Login(string email, string password);
}
