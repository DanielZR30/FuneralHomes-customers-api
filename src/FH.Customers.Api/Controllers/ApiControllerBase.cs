using Microsoft.AspNetCore.Mvc;
using FH.Customers.Application.Utilities.Mediator;
using FH.Customers.Application.Utilities.Results;

namespace FH.Customers.Api.Controllers;

/// <summary>
/// Base común de los controladores: expone el mediador y traduce un Result fallido
/// al código HTTP que trae (400, 404, 409, 422...) con el cuerpo { message }.
/// </summary>
[ApiController]
public abstract class ApiControllerBase(IMediator mediator) : ControllerBase
{
    protected IMediator Mediator { get; } = mediator;

    protected IActionResult ToError(Result result) =>
        StatusCode(result.StatusCode, new { message = result.ErrorMessage });
}
