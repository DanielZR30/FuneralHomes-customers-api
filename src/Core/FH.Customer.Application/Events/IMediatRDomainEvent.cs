using FH.Shared.Domain.Common;
using MediatR;

namespace FH.Customer.Application.Events;

public interface IMediatRDomainEvent : IDomainEvent, INotification
{
}
