namespace FH.Customers.Domain.Enums;

public enum CustomerType
{
    Individual,
    Corporate
}

public enum CustomerStatus
{
    Active,
    Inactive,
    Suspended
}

public enum SubscriptionStatus
{
    Active,
    Suspended,
    Cancelled,
    Expired
}

public enum SubjectType
{
    Human,
    Pet
}

public enum BeneficiaryType
{
    Principal,
    Associated
}

public enum RelationshipType
{
    Titular,
    Spouse,
    Child,
    Parent,
    Employee,
    Pet,
    Other
}

public enum BeneficiaryStatus
{
    Active,
    Removed
}

public enum AuditAction
{
    BeneficiaryAdded,
    BeneficiaryRemoved,
    BeneficiaryUpdated
}
