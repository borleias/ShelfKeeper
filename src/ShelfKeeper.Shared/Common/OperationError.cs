// <copyright file="OperationError.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

namespace ShelfKeeper.Shared.Common;

/// <summary>
/// Represents an error that occurred during an operation.
/// </summary>
/// <param name="Message">The error message.</param>
/// <param name="Type">The type of the error (e.g., "ValidationError", "NotFoundError").</param>
public record OperationError(string Message, OperationErrorType Type);