
using FH.Customers.Application.Abstractions;
using FH.Customers.Application.Common;
using FH.Customers.Application.Cqrs;
using FH.Customers.Application.CustomerPlans;
using FH.Customers.Application.CustomerPlans.Abstractions;
using FH.Customers.Application.CustomerPlans.DTOs;
using FH.Customers.Domain.CustomerPlans;
using FH.Customers.Domain.CustomerPlans.ValueObjects;
using FH.Customers.Domain.Exceptions;

namespace FH.Customers.Application.CustomerPlans.Commands.SubscribeCustomer;

public class SubscribeCustomerCommandHandler : ICommandHandler<SubscribeCustomerCommand, SubscriptionDto>
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly ICustomerExistenceChecker _customerExistenceChecker;
    private readonly IFinancialsPlanGateway _financialsPlanGateway;
    private readonly IUnitOfWork _unitOfWork;

    public SubscribeCustomerCommandHandler(
        ISubscriptionRepository subscriptionRepository,
        ICustomerExistenceChecker customerExistenceChecker,
        IFinancialsPlanGateway financialsPlanGateway,
        IUnitOfWork unitOfWork)
    {
        _subscriptionRepository = subscriptionRepository;
        _customerExistenceChecker = customerExistenceChecker;
        _financialsPlanGateway = financialsPlanGateway;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<SubscriptionDto>> Handle(
        SubscribeCustomerCommand request,
        CancellationToken cancellationToken)
    {
        // RN-06: El cliente debe existir y estar activo
        var customerExists = await _customerExistenceChecker.ExistsActiveAsync(request.CustomerId, cancellationToken);
        if (!customerExists)
        {
            return Result<SubscriptionDto>.NotFound("El cliente especificado no existe o no se encuentra activo.");
        }

        // RN-08: El plan debe existir en Financials
        var planExists = await _financialsPlanGateway.PlanExistsAsync(request.ExternalPlanId, cancellationToken);
        if (!planExists)
        {
            return Result<SubscriptionDto>.UnprocessableEntity("El plan financiero especificado no existe o no es válido.");
        }

        // RN-07: No puede haber dos suscripciones ACTIVE del mismo cliente y mismo external_plan_id con periodos solapados
        var activeSubscriptions = await _subscriptionRepository.GetActiveByCustomerAndPlanAsync(
            request.CustomerId,
            request.ExternalPlanId,
            cancellationToken);

        var requestedPeriod = new SubscriptionPeriod(request.StartDate, request.EndDate);

        var hasOverlap = activeSubscriptions.Any(s => s.Period.OverlapsWith(requestedPeriod));
        if (hasOverlap)
        {
            return Result<SubscriptionDto>.Conflict("El cliente ya cuenta con una suscripción activa para este plan en un período que se solapa.");
        }

        Subscription subscription;
        try
        {
            subscription = Subscription.Create(
                request.CustomerId,
                request.ExternalPlanId,
                new MaxBeneficiaries(request.MaxBeneficiaries),
                request.StartDate,
                request.EndDate);
        }
        catch (BusinessRuleException ex)
        {
            return Result<SubscriptionDto>.Failure(ex.Message, "BusinessRule", 422);
        }

        await _subscriptionRepository.AddAsync(subscription, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result<SubscriptionDto>.Success(SubscriptionDto.FromDomain(subscription), 201);
    }
}
