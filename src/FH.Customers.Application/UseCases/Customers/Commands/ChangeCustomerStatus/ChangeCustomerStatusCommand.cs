using FH.Customers.Application.UseCases.Common;
using FH.Customers.Domain.Common.Enums;

namespace FH.Customers.Application.UseCases.Customers.Commands.ChangeCustomerStatus;

public record ChangeCustomerStatusCommand(Guid Id, CustomerStatus Status) : ICommand;
