using Microsoft.AspNetCore.Mvc;
using FH.Customers.Application.Common;
using FH.Customers.Application.Mediator;

namespace FH.Customers.Api.Controllers;

/// <summary>
/// Base común de los controladores: expone el mediador y traduce un Result fallido
/// al código HTTP que trae (400, 404, 409, 422...) con el cuerpo { message }.
/// </summary>
[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    private IMediator? _mediator;

    protected IMediator Mediator =>
        _mediator ??= HttpContext.RequestServices.GetRequiredService<IMediator>();

    protected IActionResult ToError(Result result) =>
        StatusCode(result.StatusCode, new { message = result.ErrorMessage });
}
