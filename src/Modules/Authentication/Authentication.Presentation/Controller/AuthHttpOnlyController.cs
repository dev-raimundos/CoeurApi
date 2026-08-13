using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using CoeurApi.Modules.Authentication.Application.UseCases.Login;

namespace CoeurApi.Modules.Authentication.Presentation.Controller;

[ApiController]
[Route("api/v1/auth")]
[Tags("Autenticação")]
public class AuthHttpOnlyController(LoginUseCase login) : ControllerBase
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
            Domain = ".coeur.app.br",
            Expires = DateTimeOffset.UtcNow.AddHours(2)
        });
        return Ok(response.User);
    }
}
