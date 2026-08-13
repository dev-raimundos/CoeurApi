using CoeurApi.Modules.Authentication.Application.UseCases.Me;
using CoeurApi.Modules.Users.Application.UseCases;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace CoeurApi.Modules.Authentication.Presentation.Controller;

[ApiController]
[Route("api/v1/auth")]
[EndpointGroupName("Perfil do Usuário Logado")]
public class MeController(MeUseCase me) : ControllerBase
{
    [HttpGet("me")]
    [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("Dados do usuário autenticado")]
    [EndpointDescription("Retorna os dados completos do usuário dono do token JWT (cookie ou header Authorization).")]
    public async Task<ActionResult<UserResponse>> Me(CancellationToken cancellationToken)
    {
        var response = await me.ExecuteAsync(cancellationToken);
        return Ok(response);
    }
}
