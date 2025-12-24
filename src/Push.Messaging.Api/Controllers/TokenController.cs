using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Push.Messaging.Application.Interfaces;

namespace Push.Messaging.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private readonly ITokenService _tokenService;
        public TokenController(ITokenService tokenService)
        {
            _tokenService = tokenService;
        }

        [HttpPost("GenerateToken")]
        public async Task<IActionResult> GenerateTokenAsync([FromBody] Push.Messaging.Shared.Requests.GenerateTokenRequest request)
        {
            var response = await _tokenService.GenerateToken(request);

            if (!response.IsSuccess)
                return Unauthorized();

            return Ok(response);
        }
    }
}
