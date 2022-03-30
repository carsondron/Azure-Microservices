using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TokenAuth.Exceptions;

namespace TokenAuth
{
    public class TokenAuthHandler : AuthenticationHandler<TokenAuthSchemeOptions>
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;

        public TokenAuthHandler(IOptionsMonitor<TokenAuthSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IConfiguration configuration) : base(options, logger, encoder, clock)
        {
            _configuration = configuration;
            _logger = logger.CreateLogger("TokenAuthHandler");
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            try
            {
                var headerName = _configuration["Auth:TokenHeader"] ?? throw new SettingsPropertyNotFoundException("TokenHeader not defined in configuration");
                if (Request.Headers.TryGetValue(headerName, out var rawToken))
                {
                    return await GenAuthTicket(_configuration, rawToken[0]);
                } else
                {
                    return AuthenticateResult.NoResult();
                }
            }
            catch (Exception ex)
            {
                return AuthenticateResult.Fail(ex.Message);
            }
        }

        private async Task<AuthenticateResult> GenAuthTicket(IConfiguration configuration, string rawToken)
        {
            var claims = new List<Claim> { new Claim(ClaimTypes.Name, "Auth User") };

            var headerName = configuration["Auth:UserIdHeader"] ?? "x-user-id";

            if (Request.Headers.TryGetValue(headerName, out var userName))
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, userName[0]));
            }
            else
            {
                var claimName = configuration["Auth:UserIdClaim"] ?? "user-id";
                _logger.LogDebug($"Unable to find '{headerName}', so extracting user claim '{claimName}' from raw token");
                // Try to extract from the token
                claims.Add(new Claim(ClaimTypes.NameIdentifier, ExtractClaimFromToken(rawToken, claimName)));
            }

            var claimsIdentity = new ClaimsIdentity(claims, nameof(TokenAuthHandler));
            var ticket = new AuthenticationTicket(new ClaimsPrincipal(claimsIdentity), this.Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }

        private string ExtractClaimFromToken(string rawToken, string claimName)
        {
            JwtSecurityTokenHandler jwtHandler = new JwtSecurityTokenHandler();
            bool isTokenReadable = jwtHandler.CanReadToken(rawToken);

            if (!isTokenReadable)
            {
                throw new TokenHeaderInvalidException("Unauthorized, unable to read auth token");
            }

            JwtSecurityToken token = jwtHandler.ReadJwtToken(rawToken);
            Claim claimValue = token.Claims.First(c => c.Type == claimName);

            if (string.IsNullOrEmpty(claimValue.Value))
            {
                throw new TokenHeaderInvalidException($"Unauthorized, not able to extract {claimName} from claims");
            }
            return claimValue.Value;
        }
    }
}