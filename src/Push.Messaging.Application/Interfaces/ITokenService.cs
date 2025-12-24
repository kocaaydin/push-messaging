using Push.Messaging.Shared.Requests;
using Push.Messaging.Shared.Responses;

namespace Push.Messaging.Application.Interfaces;

public interface ITokenService
{
    public Task<GenerateTokenResponse> GenerateToken(GenerateTokenRequest request);
}