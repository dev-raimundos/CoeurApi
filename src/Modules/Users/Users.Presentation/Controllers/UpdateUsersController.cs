using CoeurApi.Modules.Users.Application.UseCases;
using CoeurApi.Modules.Users.Application.UseCases.Update;
using Microsoft.AspNetCore.Mvc;

namespace CoeurApi.Modules.Users.Presentation.Controllers;

[ApiController]
[Route("api/v1/users")]
public sealed class UpdateUsersController(UpdateUserUseCase useCase) : ControllerBase
{
    [HttpPut("{id:guid}")]
    [EndpointSummary("Edita um Usuário")]
    [EndpointDescription("Atualiza um usuário identificado pelo ID")]
    [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> Update(
        Guid id,
        [FromBody] UpdateUserRequest request,
        CancellationToken cancellationToken)
    {
        var user = await useCase.ExecuteAsync(id, request, cancellationToken);
        return Ok(user);
    }
}
