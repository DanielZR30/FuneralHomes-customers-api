
using FH.Customers.Application.Cqrs;
using FH.Customers.Domain.Common.Enums;

namespace FH.Customers.Application.Customers.Commands.ChangeCustomerStatus;

public record ChangeCustomerStatusCommand(Guid Id, CustomerStatus Status) : ICommand;
