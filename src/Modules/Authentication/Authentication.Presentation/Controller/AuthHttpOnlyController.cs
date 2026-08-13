using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using CoeurApi.Modules.Authentication.Application.Settings;
using CoeurApi.Modules.Authentication.Application.UseCases.Login;

namespace CoeurApi.Modules.Authentication.Presentation.Controller;

[ApiController]
[Route("api/v1/auth")]
[Tags("Autenticação")]
public class AuthHttpOnlyController(LoginUseCase login, IOptions<CookieSettings> cookieSettings) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("login")]

    [EndpointSummary("Login com email e senha")]
    [EndpointDescription("Valida as credenciais e devolve o token JWT da API.")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var response = await login.ExecuteAsync(request, cancellationToken);

        Response.Cookies.Append("token", response.Token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Domain = CookieDomain,
            Expires = DateTimeOffset.UtcNow.AddHours(6)
        });

        return Ok(response.User);
    }

    [HttpPost("logout")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [EndpointSummary("Logout")]
    [EndpointDescription("Expira o cookie HttpOnly do token.")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("token", new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Domain = CookieDomain
        });

        return NoContent();
    }

    // Domain precisa ser idêntico ao usado no Append pra o browser reconhecer e expirar o mesmo cookie.
    private string? CookieDomain =>
        string.IsNullOrWhiteSpace(cookieSettings.Value.Domain) ? null : cookieSettings.Value.Domain;
}
