namespace FH.Customers.Application.Abstractions;

/// <summary>
/// La base de datos rechazó el guardado por un conflicto (por ejemplo, clave única duplicada).
/// Persistence traduce la excepción técnica de EF a esta, para que Application no dependa de EF.
/// </summary>
public class DataConflictException : Exception
{
    public DataConflictException(string message, Exception? innerException = null)
        : base(message, innerException) { }
}
