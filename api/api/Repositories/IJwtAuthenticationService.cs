using System.Threading.Tasks;

namespace api.Repositories
{
    public interface IJwtAuthenticationService
    {
        void JwtAuthenticationService(string key);
        string Authenticate(string username, string password);
    }
}
