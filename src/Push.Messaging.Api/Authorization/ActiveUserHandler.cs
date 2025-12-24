using System;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Push.Messaging.Application.Interfaces;

namespace Push.Messaging.Api.Authorization;

public class ActiveUserHandler 
    : AuthorizationHandler<ActiveUserRequirement>
{
    private readonly IUserService _userService;

    public ActiveUserHandler(IUserService userService)
    {
        _userService = userService;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ActiveUserRequirement requirement)
    {
        var userName = context.User.FindFirst(JwtRegisteredClaimNames.UniqueName)?.Value;
        
        if (string.IsNullOrEmpty(userName))
            return;

        if (await _userService.IsActiveAsync(userName))
            context.Succeed(requirement);
    }
}
