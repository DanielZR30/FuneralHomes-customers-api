
using FH.Customers.Domain.Entities;
using FH.Customers.Domain.Enums;
using FH.Customers.Domain.Events;
using FH.Customers.Domain.Exceptions;

namespace FH.Customers.Tests.Domain;

public class BeneficiaryEntityTests
{
    private static readonly Guid SubscriptionId = Guid.NewGuid();

    private static Member NewHuman() =>
        Member.Create(SubjectType.Human, "Luis", new DateOnly(1990, 1, 1));

    [Fact]
    public void Enroll_ShouldCreateActiveBeneficiaryAndRaiseAddedEvent()
    {
        var member = NewHuman();

        var beneficiary = Beneficiary.Enroll(
            SubscriptionId, member, BeneficiaryType.Associated, RelationshipType.Spouse);

        Assert.Equal(BeneficiaryStatus.Active, beneficiary.Status);
        Assert.Equal(member.Id, beneficiary.MemberId);
        Assert.Equal(SubscriptionId, beneficiary.SubscriptionId);

        var evt = Assert.IsType<BeneficiaryAddedDomainEvent>(Assert.Single(beneficiary.DomainEvents));
        Assert.Equal(member.Id, evt.MemberId);
        Assert.Equal(member.DerivedAge, evt.DerivedAge);
    }

    [Fact]
    public void Enroll_PetWithNonPetRelationship_ShouldThrowBusinessRule()
    {
        var pet = Member.Create(SubjectType.Pet, "Firulais", new DateOnly(2020, 1, 1));

        Assert.Throws<BusinessRuleException>(() => Beneficiary.Enroll(
            SubscriptionId, pet, BeneficiaryType.Associated, RelationshipType.Spouse));
    }

    [Fact]
    public void Enroll_PetWithPetRelationship_ShouldSucceed()
    {
        var pet = Member.Create(SubjectType.Pet, "Firulais", new DateOnly(2020, 1, 1));

        var beneficiary = Beneficiary.Enroll(
            SubscriptionId, pet, BeneficiaryType.Associated, RelationshipType.Pet);

        Assert.Equal(BeneficiaryStatus.Active, beneficiary.Status);
    }

    [Fact]
    public void Create_WithEmptyIds_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => Beneficiary.Create(
            Guid.Empty, Guid.NewGuid(), BeneficiaryType.Principal, RelationshipType.Titular));
        Assert.Throws<ArgumentException>(() => Beneficiary.Create(
            Guid.NewGuid(), Guid.Empty, BeneficiaryType.Principal, RelationshipType.Titular));
    }

    [Fact]
    public void Remove_ShouldMarkRemovedAndRaiseRemovedEvent()
    {
        var beneficiary = Beneficiary.Enroll(
            SubscriptionId, NewHuman(), BeneficiaryType.Associated, RelationshipType.Child);
        beneficiary.ClearDomainEvents();

        beneficiary.Remove();

        Assert.Equal(BeneficiaryStatus.Removed, beneficiary.Status);
        Assert.NotNull(beneficiary.RemovedAt);
        Assert.IsType<BeneficiaryRemovedDomainEvent>(Assert.Single(beneficiary.DomainEvents));
    }

    [Fact]
    public void Remove_WhenAlreadyRemoved_ShouldThrowBusinessRule()
    {
        var beneficiary = Beneficiary.Enroll(
            SubscriptionId, NewHuman(), BeneficiaryType.Associated, RelationshipType.Child);
        beneficiary.Remove();

        Assert.Throws<BusinessRuleException>(() => beneficiary.Remove());
    }

    [Fact]
    public void Reactivate_ShouldRestoreActiveStatus()
    {
        var beneficiary = Beneficiary.Enroll(
            SubscriptionId, NewHuman(), BeneficiaryType.Associated, RelationshipType.Child);
        beneficiary.Remove();

        beneficiary.Reactivate();

        Assert.Equal(BeneficiaryStatus.Active, beneficiary.Status);
        Assert.Null(beneficiary.RemovedAt);
    }

    [Fact]
    public void UpdateMemberDetails_WhenMemberNotLoaded_ShouldThrowInvalidOperation()
    {
        var beneficiary = Beneficiary.Enroll(
            SubscriptionId, NewHuman(), BeneficiaryType.Associated, RelationshipType.Child);

        // Enroll no carga la navegación Member (solo el id), como pasa al consultar sin Include.
        Assert.Throws<InvalidOperationException>(() => beneficiary.UpdateMemberDetails(
            "Nuevo", new DateOnly(1991, 1, 1), null, null, null, null, null));
    }

    [Fact]
    public void UpdateMemberDetails_WhenRemoved_ShouldThrowBusinessRule()
    {
        var beneficiary = Beneficiary.Enroll(
            SubscriptionId, NewHuman(), BeneficiaryType.Associated, RelationshipType.Child);
        beneficiary.Remove();

        Assert.Throws<BusinessRuleException>(() => beneficiary.UpdateMemberDetails(
            "Nuevo", new DateOnly(1991, 1, 1), null, null, null, null, null));
    }
}
