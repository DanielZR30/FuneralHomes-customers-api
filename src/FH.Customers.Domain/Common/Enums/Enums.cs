namespace FH.Customers.Domain.Common.Enums;

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

/// <summary>Tipos de documento de identidad permitidos (Colombia).</summary>
public enum IdentificationType
{
    CC,   // Cédula de ciudadanía
    CE,   // Cédula de extranjería
    TI,   // Tarjeta de identidad
    RC,   // Registro civil
    PA,   // Pasaporte
    NIT   // Número de identificación tributaria (empresas)
}
