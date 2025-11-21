using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TestAPI.Configuration;

namespace TestAPI.Security
{
    public class BearerAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public const string SchemeName = "Bearer";

        private readonly JwtSettings _jwtSettings;

        public BearerAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            IOptions<JwtSettings> jwtSettings
        )
            : base(options, logger, encoder)
        {
            _jwtSettings = jwtSettings.Value;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
            {
                Logger.LogWarning("Authorization header missing");
                return AuthenticateResult.Fail("Missing Authorization Header");
            }

            try
            {
                var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
                var token = authHeader.Parameter;

                if (string.IsNullOrWhiteSpace(token))
                {
                    Logger.LogWarning("Authorization token is empty");
                    return AuthenticateResult.Fail("Empty Authorization Token");
                }

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidAudience = _jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.FromMinutes(5),
                };

                var claimsPrincipal = tokenHandler.ValidateToken(
                    token,
                    validationParameters,
                    out var validatedToken
                );

                Logger.LogInformation(
                    "Token validated successfully for user: {User}",
                    claimsPrincipal.Identity?.Name
                );
                return AuthenticateResult.Success(
                    new AuthenticationTicket(claimsPrincipal, Scheme.Name)
                );
            }
            catch (SecurityTokenExpiredException ex)
            {
                Logger.LogWarning(ex, "Token has expired");
                return AuthenticateResult.Fail("Token has expired");
            }
            catch (SecurityTokenException ex)
            {
                Logger.LogWarning(ex, "Token validation failed");
                return AuthenticateResult.Fail($"Invalid token: {ex.Message}");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error validating authorization token");
                return AuthenticateResult.Fail($"Authentication failed: {ex.Message}");
            }
        }
    }
}
