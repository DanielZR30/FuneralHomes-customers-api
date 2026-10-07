using Moq;
using FH.Customers.Application.Abstractions;
using FH.Customers.Application.Audit.Commands.MarkAsPublished;
using FH.Customers.Application.Audit.Commands.RecordAuditLog;
using FH.Customers.Application.Audit.EventHandlers;
using FH.Customers.Domain.Audit.Repositories;
using FH.Customers.Domain.Entities;
using FH.Customers.Domain.Enums;
using FH.Customers.Domain.Events;

namespace FH.Customers.Tests.Application;

public class AuditHandlerTests
{
    private readonly Mock<IBeneficiaryAuditLogRepository> _auditLogs = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    [Fact]
    public async Task Record_WithEmptyIds_ShouldFailAndNotCommit()
    {
        var handler = new RecordAuditLogCommandHandler(_auditLogs.Object, _unitOfWork.Object);

        var result = await handler.Handle(
            new RecordAuditLogCommand(Guid.Empty, Guid.NewGuid(), AuditAction.BeneficiaryAdded),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Record_WithValidData_ShouldSaveAndReturn201()
    {
        var handler = new RecordAuditLogCommandHandler(_auditLogs.Object, _unitOfWork.Object);

        var result = await handler.Handle(
            new RecordAuditLogCommand(Guid.NewGuid(), Guid.NewGuid(), AuditAction.BeneficiaryRemoved),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(201, result.StatusCode);
        _auditLogs.Verify(r => r.AddAsync(It.IsAny<BeneficiaryAuditLog>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MarkAsPublished_WhenLogDoesNotExist_ShouldReturnNotFound()
    {
        var handler = new MarkAsPublishedCommandHandler(_auditLogs.Object, _unitOfWork.Object);

        var result = await handler.Handle(new MarkAsPublishedCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(404, result.StatusCode);
        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task MarkAsPublished_WhenLogExists_ShouldFlagItAndCommit()
    {
        var log = BeneficiaryAuditLog.Create(Guid.NewGuid(), Guid.NewGuid(), AuditAction.BeneficiaryAdded);
        _auditLogs
            .Setup(r => r.GetByIdAsync(log.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(log);
        var handler = new MarkAsPublishedCommandHandler(_auditLogs.Object, _unitOfWork.Object);

        var result = await handler.Handle(new MarkAsPublishedCommand(log.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(log.EventPublished);
        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddedEventHandler_ShouldRegisterAuditLogWithAddedAction()
    {
        BeneficiaryAuditLog? saved = null;
        _auditLogs
            .Setup(r => r.AddAsync(It.IsAny<BeneficiaryAuditLog>(), It.IsAny<CancellationToken>()))
            .Callback<BeneficiaryAuditLog, CancellationToken>((l, _) => saved = l)
            .Returns(Task.CompletedTask);
        var handler = new BeneficiaryAddedAuditHandler(_auditLogs.Object);
        var subscriptionId = Guid.NewGuid();
        var memberId = Guid.NewGuid();

        await handler.Handle(
            new BeneficiaryAddedDomainEvent(
                subscriptionId, memberId, SubjectType.Human, 30,
                RelationshipType.Spouse, BeneficiaryType.Associated),
            CancellationToken.None);

        Assert.NotNull(saved);
        Assert.Equal(AuditAction.BeneficiaryAdded, saved!.Action);
        Assert.Equal(subscriptionId, saved.SubscriptionId);
        Assert.Equal(memberId, saved.MemberId);
    }
}
