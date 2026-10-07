using FH.Shared.Application.Cqrs;
using FH.Shared.Domain.Enums;

namespace FH.Modules.Customer.Application.Commands.ChangeCustomerStatus;

public record ChangeCustomerStatusCommand(Guid Id, CustomerStatus Status) : ICommand;
