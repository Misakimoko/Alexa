using System.Threading.Tasks;
using Lexa.Entities;

namespace Lexa.Security;

public interface ITokenService
{
    Task<(string Token, System.DateTime ExpiresAt)> CreateTokenAsync(User user, CancellationToken ct = default);
}
