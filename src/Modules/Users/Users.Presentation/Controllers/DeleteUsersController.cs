using CoeurApi.Modules.Users.Application.UseCases.Delete;
using Microsoft.AspNetCore.Mvc;


namespace CoeurApi.Modules.Users.Presentation.Controllers;

[ApiController]
[Route("api/v1/users")]
public sealed class DeleteUsersController(DeleteUserUseCase useCase) : ControllerBase
{
    [HttpDelete("{id:guid}")]
    [EndpointSummary("Deleta um Usuário")]
    [EndpointDescription("Exclui um usuário identificado pelo ID")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await useCase.ExecuteAsync(id, cancellationToken);
        return NoContent();
    }
}
