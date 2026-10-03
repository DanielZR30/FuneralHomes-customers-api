using System.Collections.Concurrent;
using FH.Shared.Domain.Entities;
using FH.Shared.Domain.Enums;
using FH.Shared.Domain.Services;

namespace FH.Shared.MockData;

public class InMemoryCustomerStore
{
    private readonly ConcurrentDictionary<Guid, Customer> _customers = new();
    private readonly ConcurrentDictionary<Guid, CustomerSubscription> _subscriptions = new();
    private readonly ConcurrentDictionary<Guid, Member> _members = new();
    private readonly ConcurrentDictionary<Guid, Beneficiary> _beneficiaries = new();
    private readonly ConcurrentBag<BeneficiaryAuditLog> _auditLogs = new();

    public InMemoryCustomerStore()
    {
        SeedMockData();
    }

    private void SeedMockData()
    {
        // 1. Cliente Persona Natural
        var customer1 = new Customer(
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            CustomerType.Individual,
            "Carlos Alberto Rodríguez Pérez",
            "CC",
            "1020304050",
            "carlos.rodriguez@example.com",
            "+573001234567",
            "Calle 100 # 15-20, Bogotá");

        // 2. Cliente Corporativo
        var customer2 = new Customer(
            Guid.Parse("22222222-2222-2222-2222-222222222222"),
            CustomerType.Corporate,
            "Tech Solutions Corp S.A.S.",
            "NIT",
            "900123456-1",
            "corporativo@techsolutions.com",
            "+576017894561",
            "Carrera 7 # 71-21 Torre B, Bogotá");

        _customers.TryAdd(customer1.Id, customer1);
        _customers.TryAdd(customer2.Id, customer2);

        // 3. Suscripción a Plan Funerario (Plan Familiar con cupo para 4)
        var subscription1 = new CustomerSubscription(
            Guid.Parse("33333333-3333-3333-3333-333333333333"),
            customer1.Id,
            Guid.Parse("99999999-9999-9999-9999-999999999999"), // ExternalPlanId de Financials
            maxBeneficiaries: 4,
            startDate: new DateOnly(2026, 1, 15));

        _subscriptions.TryAdd(subscription1.Id, subscription1);

        // 4. Miembro Titular (Humano)
        var member1 = new Member(
            Guid.Parse("44444444-4444-4444-4444-444444444441"),
            SubjectType.Human,
            "Carlos Alberto",
            new DateOnly(1985, 4, 12),
            "Rodríguez Pérez",
            "CC",
            "1020304050",
            "carlos.rodriguez@example.com",
            "+573001234567");

        // 5. Miembro Cónyuge (Humano)
        var member2 = new Member(
            Guid.Parse("44444444-4444-4444-4444-444444444442"),
            SubjectType.Human,
            "Laura Sofía",
            new DateOnly(1988, 8, 24),
            "Mendoza Gómez",
            "CC",
            "52987654",
            "laura.mendoza@example.com",
            "+573105559876");

        // 6. Miembro Mascota (Pet)
        var member3 = new Member(
            Guid.Parse("44444444-4444-4444-4444-444444444443"),
            SubjectType.Pet,
            "Max (Golden Retriever)",
            new DateOnly(2022, 6, 10));

        _members.TryAdd(member1.Id, member1);
        _members.TryAdd(member2.Id, member2);
        _members.TryAdd(member3.Id, member3);

        // 7. Vínculos de Beneficiarios
        var ben1 = new Beneficiary(
            Guid.Parse("55555555-5555-5555-5555-555555555551"),
            subscription1.Id,
            member1.Id,
            BeneficiaryType.Principal,
            RelationshipType.Titular);

        var ben2 = new Beneficiary(
            Guid.Parse("55555555-5555-5555-5555-555555555552"),
            subscription1.Id,
            member2.Id,
            BeneficiaryType.Associated,
            RelationshipType.Spouse);

        var ben3 = new Beneficiary(
            Guid.Parse("55555555-5555-5555-5555-555555555553"),
            subscription1.Id,
            member3.Id,
            BeneficiaryType.Associated,
            RelationshipType.Pet);

        _beneficiaries.TryAdd(ben1.Id, ben1);
        _beneficiaries.TryAdd(ben2.Id, ben2);
        _beneficiaries.TryAdd(ben3.Id, ben3);

        // 8. Bitácora de Auditoría inicial
        _auditLogs.Add(new BeneficiaryAuditLog(Guid.NewGuid(), subscription1.Id, member1.Id, AuditAction.BeneficiaryAdded, true));
        _auditLogs.Add(new BeneficiaryAuditLog(Guid.NewGuid(), subscription1.Id, member2.Id, AuditAction.BeneficiaryAdded, true));
        _auditLogs.Add(new BeneficiaryAuditLog(Guid.NewGuid(), subscription1.Id, member3.Id, AuditAction.BeneficiaryAdded, true));
    }

    // --- Métodos de Clientes ---
    public IEnumerable<Customer> GetCustomers() =>
        _customers.Values.OrderByDescending(c => c.CreatedAt);

    public Customer? GetCustomerById(Guid id) =>
        _customers.TryGetValue(id, out var customer) ? customer : null;

    public Customer? GetCustomerByIdentification(string type, string number) =>
        _customers.Values.FirstOrDefault(c =>
            c.IdentificationType.Equals(type, StringComparison.OrdinalIgnoreCase) &&
            c.IdentificationNumber.Equals(number, StringComparison.OrdinalIgnoreCase));

    public bool ExistsCustomerIdentification(string type, string number) =>
        GetCustomerByIdentification(type, number) is not null;

    public void AddCustomer(Customer customer) =>
        _customers.TryAdd(customer.Id, customer);

    // --- Métodos de Suscripciones ---
    public IEnumerable<CustomerSubscription> GetSubscriptions() =>
        _subscriptions.Values.OrderByDescending(s => s.StartDate);

    public CustomerSubscription? GetSubscriptionById(Guid id) =>
        _subscriptions.TryGetValue(id, out var sub) ? sub : null;

    public IEnumerable<CustomerSubscription> GetSubscriptionsByCustomer(Guid customerId) =>
        _subscriptions.Values.Where(s => s.CustomerId == customerId).OrderByDescending(s => s.StartDate);

    public void AddSubscription(CustomerSubscription subscription) =>
        _subscriptions.TryAdd(subscription.Id, subscription);

    // --- Métodos de Beneficiarios ---
    public IEnumerable<(Beneficiary Beneficiary, Member Member)> GetBeneficiariesBySubscription(Guid subscriptionId)
    {
        var list = new List<(Beneficiary, Member)>();
        var subBens = _beneficiaries.Values.Where(b => b.SubscriptionId == subscriptionId).OrderBy(b => b.JoinedAt);

        foreach (var b in subBens)
        {
            if (_members.TryGetValue(b.MemberId, out var m))
            {
                list.Add((b, m));
            }
        }

        return list;
    }

    public int GetActiveBeneficiariesCount(Guid subscriptionId) =>
        _beneficiaries.Values.Count(b => b.SubscriptionId == subscriptionId && b.Status == BeneficiaryStatus.Active);

    public Beneficiary? GetBeneficiary(Guid subscriptionId, Guid memberId) =>
        _beneficiaries.Values.FirstOrDefault(b => b.SubscriptionId == subscriptionId && b.MemberId == memberId);

    public void AddBeneficiary(Member member, Beneficiary beneficiary, BeneficiaryAuditLog auditLog)
    {
        _members.TryAdd(member.Id, member);
        _beneficiaries.TryAdd(beneficiary.Id, beneficiary);
        _auditLogs.Add(auditLog);
    }

    public void AddAuditLog(BeneficiaryAuditLog auditLog) =>
        _auditLogs.Add(auditLog);

    // --- Métodos de Auditoría ---
    public IEnumerable<BeneficiaryAuditLog> GetAuditLogsBySubscription(Guid subscriptionId) =>
        _auditLogs.Where(a => a.SubscriptionId == subscriptionId).OrderByDescending(a => a.CreatedAt);

    public IEnumerable<BeneficiaryAuditLog> GetAuditLogsByMember(Guid memberId) =>
        _auditLogs.Where(a => a.MemberId == memberId).OrderByDescending(a => a.CreatedAt);
}
