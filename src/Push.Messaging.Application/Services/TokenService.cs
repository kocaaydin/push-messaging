using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Push.Messaging.Application.Interfaces;
using Push.Messaging.Shared.Interfaces;
using Push.Messaging.Shared.Requests;
using Push.Messaging.Shared.Responses;

namespace Push.Messaging.Application.Services;

public class TokenService : ITokenService
{
    private readonly IUserCache _userCache;
    private readonly string _secret;

    public TokenService(IUserCache userCache, IConfiguration config)
    {
        _userCache = userCache;
        _secret = config["Jwt:Secret"]!;
    }

    public async Task<GenerateTokenResponse> GenerateToken(GenerateTokenRequest request)
    {
        var user = await _userCache.GetAsync(request.UserName);
        if (user is null || !user.IsActive)
            return new GenerateTokenResponse { IsSuccess = false };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName)
            },
            expires: DateTime.UtcNow.AddYears(10),
            signingCredentials: creds
        );

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);

        return new GenerateTokenResponse
        {
            AccessToken = jwt,
            IsSuccess = true
        };
    }
}