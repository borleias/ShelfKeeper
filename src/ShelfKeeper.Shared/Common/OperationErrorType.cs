namespace ShelfKeeper.Shared.Common;

/// <summary>
/// Defines the types of errors that can occur in the application.
/// </summary>
public enum OperationErrorType
{
    /// <summary>
    /// Indicates a validation error.
    /// </summary>
    ValidationError,

    /// <summary>
    /// Indicates that a requested resource was not found.
    /// </summary>
    NotFoundError,

    /// <summary>
    /// Indicates an unauthorized access error.
    /// </summary>
    UnauthorizedError,

    /// <summary>
    /// Indicates a conflict error (e.g., resource already exists).
    /// </summary>
    ConflictError,

    /// <summary>
    /// Indicates an internal server error.
    /// </summary>
    InternalServerError
}