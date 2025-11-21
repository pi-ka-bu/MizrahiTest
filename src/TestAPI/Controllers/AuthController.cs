using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TestAPI.Interfaces;

namespace TestAPI.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IJwtTokenGenerator _tokenGenerator;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IJwtTokenGenerator tokenGenerator, ILogger<AuthController> logger)
        {
            _tokenGenerator = tokenGenerator;
            _logger = logger;
        }

        [HttpPost("token")]
        public IActionResult GenerateToken([FromBody] TokenRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Username))
            {
                return BadRequest(new { Error = "Username is required" });
            }

            try
            {
                var token = _tokenGenerator.GenerateToken(request.Username);
                _logger.LogInformation("Generated token for user: {Username}", request.Username);

                return Ok(
                    new TokenResponse
                    {
                        Token = token,
                        TokenType = "Bearer",
                        ExpiresIn = 3600,
                        Username = request.Username,
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error generating token for user: {Username}",
                    request.Username
                );
                return StatusCode(500, new { Error = "Failed to generate token" });
            }
        }
    }

    public class TokenRequest
    {
        public string Username { get; set; } = string.Empty;
    }

    public class TokenResponse
    {
        public string Token { get; set; } = string.Empty;

        public string TokenType { get; set; } = string.Empty;

        public int ExpiresIn { get; set; }

        public string Username { get; set; } = string.Empty;
    }
}
