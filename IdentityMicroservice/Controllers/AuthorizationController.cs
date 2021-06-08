using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using App.Metrics;
using IdentityCore.Constants;
using IdentityCore.Interfaces.Services;
using IdentityCore.Models;
using IdentityCore.Models.ServiceResponses;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using Metrics = IdentityApi.StartupConfig.Metrics;

namespace IdentityApi.Controllers
{
    [ApiVersionNeutral]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class AuthorizationController : ControllerBase
    {
        private readonly IAuthenticateService _authorizeService;
        private static IMetrics _metrics;

        public AuthorizationController(IAuthenticateService authorizeService, IMetrics metrics)
        {
            _authorizeService = authorizeService;
            _metrics = metrics;
        }

        [HttpPost("~/token")]
        public Task<IActionResult> Exchange()
        {
            var tokenRequest = HttpContext.GetOpenIddictServerRequest();
            using (_metrics.Measure.Timer.Time(Metrics.TokenEndpoint))
            {
                return tokenRequest?.GrantType switch
                {
                    GrantTypes.Password => PasswordRequest(tokenRequest),
                    GrantTypes.RefreshToken => RefreshTokenRequest(),
                    GrantTypes.TwoFactor => TwoFactorRequest(tokenRequest),
                    GrantTypes.PersonalAccessToken => PersonalAccessTokenRequest(tokenRequest),

                    _ => Task.FromResult<IActionResult>(BadRequest(new OpenIddictResponse
                    {
                        Error = OpenIddictConstants.Errors.UnsupportedGrantType,
                        ErrorDescription = "The specified grant type is not supported."
                    }))
                };
            }
        }

        private async Task<IActionResult> PersonalAccessTokenRequest(OpenIddictRequest tokenRequest)
        {
            var patData = new PersonalAccessTokenMinimum((string)tokenRequest.GetParameter("personalAccessTokenName"), (string)tokenRequest.GetParameter("personalAccessTokenDescription"));
           
            var response = await _authorizeService.PersonalAccessTokenFlow(tokenRequest.Username, tokenRequest.Password, patData);

            if (!response.IsSuccess)
                return BadRequest(response.ErrorResponse);

            if (!string.IsNullOrEmpty(response.PasswordToken))
                return Ok(new TwoFactorFirstResponse(response.PasswordToken, response.TwoFAType));

            var ticket = response.Ticket;
            var signin = SignIn(ticket.Principal, ticket.Properties, ticket.AuthenticationScheme);

            return signin;
        }

        private async Task<IActionResult> PasswordRequest(OpenIddictRequest tokenRequest)
        {
            bool.TryParse((string)tokenRequest.GetParameter("extendTokenDuration"), out var extendedDuration);

            var response = await _authorizeService.PasswordFlow(tokenRequest.Username, tokenRequest.Password, extendedDuration);

            if (!response.IsSuccess)
                return BadRequest(response.ErrorResponse);

            if (!string.IsNullOrEmpty(response.PasswordToken))
                return Ok(new TwoFactorFirstResponse(response.PasswordToken, response.TwoFAType));

            var ticket = response.Ticket;
            return SignIn(ticket.Principal, ticket.Properties, ticket.AuthenticationScheme);
        }

        private async Task<IActionResult> RefreshTokenRequest()
        {
            var info = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            var response = await _authorizeService.RefreshTokenFlow(info);

            if (!response.IsSuccess)
                return BadRequest(response.ErrorResponse);

            var ticket = response.Ticket;
            return SignIn(ticket.Principal, ticket.Properties, ticket.AuthenticationScheme);
        }

        private async Task<IActionResult> TwoFactorRequest(OpenIddictRequest tokenRequest)
        {
            var username = tokenRequest.Username;
            var passwordToken = (string)tokenRequest.GetParameter("passwordToken");
            var twoFactorToken = (string)tokenRequest.GetParameter("token");
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(passwordToken) || string.IsNullOrEmpty(twoFactorToken))
            {
                return BadRequest(new OpenIddictResponse
                {
                    Error = OpenIddictConstants.Errors.InvalidRequest,
                    ErrorDescription = "The mandatory 'username' and/or 'token' and/or 'passwordToken' parameters are missing."
                });
            }

            var response = await _authorizeService.TwoFactorFlow(username, passwordToken, twoFactorToken);
            if (!response.IsSuccess)
                return BadRequest(response.ErrorResponse);

            var ticket = response.Ticket;
            return SignIn(ticket.Principal, ticket.Properties, ticket.AuthenticationScheme);
        }
    }
}
