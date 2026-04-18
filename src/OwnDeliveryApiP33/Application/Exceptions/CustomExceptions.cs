namespace OwnDeliveryApiP33.Application.Exceptions;

/// <summary>Thrown when an entity is not found</summary>
public class EntityNotFoundException : Exception
{
    public EntityNotFoundException(string message) : base(message) { }
    
    public EntityNotFoundException(string entityName, Guid id) 
        : base($"{entityName} with ID {id} not found.") { }
}

/// <summary>Thrown when an entity already exists</summary>
public class DuplicateEntityException : Exception
{
    public DuplicateEntityException(string message) : base(message) { }
}

/// <summary>Thrown when operation is not allowed</summary>
public class InvalidOperationException : Exception
{
    public InvalidOperationException(string message) : base(message) { }
}

/// <summary>Thrown when user is not authorized</summary>
public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message) : base(message) { }
}

/// <summary>Thrown when payment operation fails</summary>
public class PaymentException : Exception
{
    public PaymentException(string message) : base(message) { }
}

/// <summary>Thrown when business validation fails</summary>
public class ValidationException : Exception
{
    public ValidationException(string message) : base(message) { }
    
    public ValidationException(IEnumerable<FluentValidation.Results.ValidationFailure> errors)
        : base("One or more validation failures have occurred.")
    {
        Errors = errors.ToList();
    }

    public List<FluentValidation.Results.ValidationFailure> Errors { get; }
}
