using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using CoeurApi.Modules.Authentication.Application.UseCases.Login;
using CoeurApi.Modules.Authentication.Application.Settings;
using Microsoft.AspNetCore.Routing;

namespace CoeurApi.Modules.Authentication.Presentation.Controller;

[ApiController]
[Route("api/v1/auth")]
[Tags("Autenticação via Cookie JWT")]
public class AuthController(LoginUseCase login, IOptions<JwtSettings> jwtSettings, IHostEnvironment environment) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("login")]

    [EndpointSummary("Login")]
    [EndpointDescription("Realiza login do usuário e devolver um cookie jwt para autenticação.")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await login.ExecuteAsync(request, cancellationToken);
        var response = result.Response;
        var token = result.Token;

        Response.Cookies.Append("access_token", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = environment.IsProduction(),
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddHours(jwtSettings.Value.ExpirationHours)
        });

        return Ok(response);
    }

    [HttpPost("logout")]
    [AllowAnonymous]

    [EndpointSummary("Logout")]
    [EndpointDescription("Revoga cookie de login e faz logout do usuário.")]
    public ActionResult Logout()
    {
        Response.Cookies.Delete("access_token");
        return NoContent();
    }
}
