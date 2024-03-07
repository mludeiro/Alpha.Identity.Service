using Alpha.Common.Token;
using Refit;

namespace Alpha.Identity.Services;
    
public interface IRestTokenService
{
    [Post("/api/token")]
    public Task<ApiResponse<TokenGeneration?>> PostAsync([Body]List<ClaimValue> claimValues);
}
