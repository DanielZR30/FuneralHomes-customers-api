using Moq;
using FH.Customers.Application.Abstractions;
using FH.Customers.Application.Beneficiaries.Commands.AddBeneficiary;
using FH.Customers.Application.Beneficiaries.Commands.RemoveBeneficiary;
using FH.Customers.Domain.Beneficiaries.Repositories;
using FH.Customers.Domain.CustomerPlans;
using FH.Customers.Domain.CustomerPlans.ValueObjects;
using FH.Customers.Domain.Entities;
using FH.Customers.Domain.Enums;
using FH.Customers.Domain.Repositories;

namespace FH.Customers.Tests.Application;

public class BeneficiaryHandlerTests
{
    private readonly Mock<IRepository<Subscription, Guid>> _subscriptions = new();
    private readonly Mock<IBeneficiaryRepository> _beneficiaries = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private static Subscription NewSubscription(int max = 2) =>
        Subscription.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new MaxBeneficiaries(max),
            DateOnly.FromDateTime(DateTime.UtcNow));

    private AddBeneficiaryCommandHandler AddHandler() =>
        new(_subscriptions.Object, _beneficiaries.Object, _unitOfWork.Object);

    private static AddBeneficiaryCommand AddCommand(Guid subscriptionId, string? docType = "CC", string? docNumber = "123") =>
        new(subscriptionId, SubjectType.Human, "Luis", new DateOnly(1990, 1, 1),
            BeneficiaryType.Associated, RelationshipType.Spouse,
            "Pérez", docType, docNumber);

    private void GivenSubscription(Subscription subscription) =>
        _subscriptions
            .Setup(r => r.GetByIdAsync(subscription.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);

    // ---------- AddBeneficiary ----------

    [Fact]
    public async Task Add_WhenSubscriptionDoesNotExist_ShouldReturnNotFound()
    {
        var result = await AddHandler().Handle(AddCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(404, result.StatusCode);
        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Add_WhenSubscriptionIsNotActive_ShouldReturn422()
    {
        var subscription = NewSubscription();
        subscription.Suspend();
        GivenSubscription(subscription);

        var result = await AddHandler().Handle(AddCommand(subscription.Id), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(422, result.StatusCode);
    }

    [Fact]
    public async Task Add_WhenCapacityIsFull_ShouldReturnConflict()
    {
        var subscription = NewSubscription(max: 1);
        GivenSubscription(subscription);
        _beneficiaries
            .Setup(r => r.CountActiveAsync(subscription.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await AddHandler().Handle(AddCommand(subscription.Id), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(409, result.StatusCode);
        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Add_WhenDocumentAlreadyEnrolled_ShouldReturnConflict()
    {
        var subscription = NewSubscription();
        GivenSubscription(subscription);
        _beneficiaries
            .Setup(r => r.ExistsActiveByIdentificationAsync(
                subscription.Id, "CC", "123", It.IsAny<CancellationToken>(), It.IsAny<Guid?>()))
            .ReturnsAsync(true);

        var result = await AddHandler().Handle(AddCommand(subscription.Id), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(409, result.StatusCode);
        _beneficiaries.Verify(r => r.AddAsync(
            It.IsAny<Member>(), It.IsAny<Beneficiary>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Add_WithHalfDocument_ShouldReturn400()
    {
        var subscription = NewSubscription();
        GivenSubscription(subscription);

        var result = await AddHandler().Handle(
            AddCommand(subscription.Id, docType: "CC", docNumber: null), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(400, result.StatusCode);
    }

    [Fact]
    public async Task Add_WithValidData_ShouldPersistAndReturn201()
    {
        var subscription = NewSubscription();
        GivenSubscription(subscription);

        var result = await AddHandler().Handle(AddCommand(subscription.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(201, result.StatusCode);
        _beneficiaries.Verify(r => r.AddAsync(
            It.IsAny<Member>(), It.IsAny<Beneficiary>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ---------- RemoveBeneficiary ----------

    [Fact]
    public async Task Remove_WhenNotLinked_ShouldReturnNotFound()
    {
        var handler = new RemoveBeneficiaryCommandHandler(_beneficiaries.Object, _unitOfWork.Object);

        var result = await handler.Handle(
            new RemoveBeneficiaryCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(404, result.StatusCode);
        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Remove_WhenActive_ShouldCommitAndReturn204()
    {
        var subscriptionId = Guid.NewGuid();
        var member = Member.Create(SubjectType.Human, "Luis", new DateOnly(1990, 1, 1));
        var beneficiary = Beneficiary.Enroll(subscriptionId, member, BeneficiaryType.Associated, RelationshipType.Child);
        _beneficiaries
            .Setup(r => r.GetAsync(subscriptionId, member.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(beneficiary);
        var handler = new RemoveBeneficiaryCommandHandler(_beneficiaries.Object, _unitOfWork.Object);

        var result = await handler.Handle(
            new RemoveBeneficiaryCommand(subscriptionId, member.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(204, result.StatusCode);
        Assert.Equal(BeneficiaryStatus.Removed, beneficiary.Status);
        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Remove_WhenAlreadyRemoved_ShouldReturnConflict()
    {
        var subscriptionId = Guid.NewGuid();
        var member = Member.Create(SubjectType.Human, "Luis", new DateOnly(1990, 1, 1));
        var beneficiary = Beneficiary.Enroll(subscriptionId, member, BeneficiaryType.Associated, RelationshipType.Child);
        beneficiary.Remove();
        _beneficiaries
            .Setup(r => r.GetAsync(subscriptionId, member.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(beneficiary);
        var handler = new RemoveBeneficiaryCommandHandler(_beneficiaries.Object, _unitOfWork.Object);

        var result = await handler.Handle(
            new RemoveBeneficiaryCommand(subscriptionId, member.Id), CancellationToken.None);

        Assert.Equal(409, result.StatusCode);
        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
