// <copyright file="OperationResult.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

namespace ShelfKeeper.Shared.Common
{
    /// <summary>
    /// Represents the operationResult of an operation, indicating success or failure.
    /// </summary>
    public class OperationResult
    {
        /// <summary>
        /// Gets a value indicating whether the operation was successful.
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// Gets a value indicating whether the operation failed.
        /// </summary>
        public bool IsFailure => !IsSuccess;

        /// <summary>
        /// Gets the collection of errors if the operation failed.
        /// </summary>
        public IEnumerable<OperationError> Errors { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OperationResult"/> class for a successful operation.
        /// </summary>
        protected OperationResult()
        {
            IsSuccess = true;
            Errors = Enumerable.Empty<OperationError>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OperationResult"/> class for a failed operation.
        /// </summary>
        /// <param name="errors">The errors that occurred during the operation.</param>
        protected OperationResult(IEnumerable<OperationError> errors)
        {
            IsSuccess = false;
            Errors = errors;
        }

        /// <summary>
        /// Creates a successful operationResult.
        /// </summary>
        /// <returns>A new instance of <see cref="OperationResult"/> indicating success.</returns>
        public static OperationResult Success() => new OperationResult();

        /// <summary>
        /// Creates a failed operationResult with the specified error message and type.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="type">The type of the error.</param>
        /// <returns>A new instance of <see cref="OperationResult"/> indicating failure.</returns>
        public static OperationResult Failure(string message, OperationErrorType type) => new OperationResult(new[] { new OperationError(message, type) });

        /// <summary>
        /// Creates a failed operationResult with the specified error objects.
        /// </summary>
        /// <param name="errors">The error objects.</param>
        /// <returns>A new instance of <see cref="OperationResult"/> indicating failure.</returns>
        public static OperationResult Failure(IEnumerable<OperationError> errors) => new OperationResult(errors);
    }

    /// <summary>
    /// Represents the operationResult of an operation with a value, indicating success or failure.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    public class OperationResult<TValue> : OperationResult
    {
        private readonly TValue? _value;

        /// <summary>
        /// Gets the value of the operationResult if the operation was successful.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if <see cref="IsFailure"/> is true.</exception>
        public TValue Value
        {
            get
            {
                if (IsFailure)
                {
                    throw new InvalidOperationException("Cannot access value when operationResult is a failure.");
                }
                return _value!;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OperationOperationResult{TValue}"/> class for a successful operation with a value.
        /// </summary>
        /// <param name="value">The value of the operationResult.</param>
        private OperationResult(TValue value)
        {
            _value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OperationOperationResult{TValue}"/> class for a failed operation.
        /// </summary>
        /// <param name="errors">The errors that occurred during the operation.</param>
        private OperationResult(IEnumerable<OperationError> errors) : base(errors)
        {
            _value = default; // Set to default value, which is null for reference types
        }

        /// <summary>
        /// Creates a successful operationResult with a value.
        /// </summary>
        /// <param name="value">The value of the operationResult.</param>
        /// <returns>A new instance of <see cref="OperationOperationResult{TValue}"/> indicating success.</returns>
        public static OperationResult<TValue> Success(TValue value) => new OperationResult<TValue>(value);

        /// <summary>
        /// Creates a failed operationResult with the specified error message and type.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="type">The type of the error.</param>
        /// <returns>A new instance of <see cref="OperationOperationResult{TValue}"/> indicating failure.</returns>
        public static new OperationResult<TValue> Failure(string message, OperationErrorType type) => new OperationResult<TValue>(new[] { new OperationError(message, type) });

        /// <summary>
        /// Creates a failed operationResult with the specified error objects.
        /// </summary>
        /// <param name="errors">The error objects.</param>
        /// <returns>A new instance of <see cref="OperationOperationResult{TValue}"/> indicating failure.</returns>
        public static new OperationResult<TValue> Failure(IEnumerable<OperationError> errors) => new OperationResult<TValue>(errors);
    }
}