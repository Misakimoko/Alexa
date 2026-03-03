using System.Threading.Tasks;
using WebApplication1.Entities;

namespace WebApplication1.Security;

public interface ITokenService
{
    Task<(string Token, System.DateTime ExpiresAt)> CreateTokenAsync(User user, CancellationToken ct = default);
}
