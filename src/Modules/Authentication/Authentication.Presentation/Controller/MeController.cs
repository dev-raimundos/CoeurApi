using CoeurApi.Application.Abstractions;
using CoeurApi.Modules.Authentication.Application.UseCases;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace CoeurApi.Modules.Authentication.Presentation.Controller;

[ApiController]
[Route("api/v1/auth")]
[EndpointGroupName("Perfil do Usuário Logado")]
public class MeController(ICurrentUser user) : ControllerBase
{
    [HttpGet("me")]
    [EndpointSummary("Dados do usuário autenticado")]
    [EndpointDescription("Retorna id, nome e email do usuário dono do token JWT enviado no header Authorization.")]
    public ActionResult<MeResponse> Me()
    {
        return Ok(new MeResponse(user.Id, user.Name, user.Email));
    }
}
