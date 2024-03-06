using Alpha.Common.Token;
using Refit;

namespace Alpha.Identity.Services;
    
public interface IRestTokenService
{
    [Post("/api/token")]
    public Task<TokenGeneration> PostAsync([Body]List<ClaimValue> claimValues);
}