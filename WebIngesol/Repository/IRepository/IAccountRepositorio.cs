using WebIngesol.Models;
using System.Threading.Tasks;

namespace WebIngesol.Repository.IRepository
{
    public interface IAccountRepositorio
    {
        Task<LoginResponse> LoginAsync(string url, LoginRequest loginRequest);
        Task<bool> RegisterAsync(string url, RegisterDto usuario);

        // Ahora recibe el token JWT para autorización
        Task<Usuario?> ObtenerUsuarioPorUserName(string url, string userName, string jwtToken);
    }
}
