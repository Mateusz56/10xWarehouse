using System.Runtime.Serialization;

namespace _10xWarehouseNet.Exceptions;

/// <summary>
/// Exception thrown when pagination parameters are invalid
/// </summary>
public class InvalidPaginationException : ArgumentException
{
    public InvalidPaginationException(string message) : base(message) { }
    public InvalidPaginationException(string message, string paramName) : base(message, paramName) { }
}

/// <summary>
/// Exception thrown when user ID is invalid or not found
/// </summary>
public class InvalidUserIdException : ArgumentException
{
    public InvalidUserIdException()
    {
    }

    public InvalidUserIdException(string message) : base(message) { }
    public InvalidUserIdException(string message, string paramName) : base(message, paramName) { }

    public InvalidUserIdException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    public InvalidUserIdException(string? message, string? paramName, Exception? innerException) : base(message, paramName, innerException)
    {
    }

    protected InvalidUserIdException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}

/// <summary>
/// Exception thrown when database operations fail
/// </summary>
public class DatabaseOperationException : Exception
{
    public DatabaseOperationException(string message) : base(message) { }
    public DatabaseOperationException(string message, Exception innerException) : base(message, innerException) { }
}
