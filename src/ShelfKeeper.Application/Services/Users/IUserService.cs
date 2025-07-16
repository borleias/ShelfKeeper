// <copyright file="IUserService.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

using ShelfKeeper.Application.Services.Users.Models;
using ShelfKeeper.Shared.Common;

namespace ShelfKeeper.Application.Services.Users
{
    /// <summary>
    /// Defines the interface for a user management service.
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Creates a new user account asynchronously.
        /// </summary>
        /// <param name="command">The command containing user registration details.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task operationResult contains a <see cref="OperationResult{TValue}"/> with the new user's details.</returns>
        Task<OperationResult<CreateUserResponse>> CreateUserAsync(CreateUserCommand command, CancellationToken cancellationToken);

        /// <summary>
        /// Authenticates a user and generates a JWT token upon successful login asynchronously.
        /// </summary>
        /// <param name="query">The query containing user login credentials.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task operationResult contains a <see cref="OperationResult{TValue}"/> with user details and a JWT token.</returns>
        Task<OperationResult<LoginUserResponse>> LoginUserAsync(LoginUserQuery query, CancellationToken cancellationToken);

        /// <summary>
        /// Changes a user's password asynchronously.
        /// </summary>
        /// <param name="command">The command containing the user's ID, old password, and new password.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task operationResult contains a <see cref="OperationResult"/> indicating success or failure.</returns>
        Task<OperationResult> ChangePasswordAsync(ChangePasswordCommand command, CancellationToken cancellationToken);

        /// <summary>
        /// Initiates the password reset process for a user asynchronously.
        /// </summary>
        /// <param name="command">The command containing the user's email address.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task operationResult contains a <see cref="OperationResult"/> indicating success or failure.</returns>
        Task<OperationResult> ForgotPasswordAsync(ForgotPasswordCommand command, CancellationToken cancellationToken);

        /// <summary>
        /// Resets a user's password using a reset token asynchronously.
        /// </summary>
        /// <param name="command">The command containing the reset token, email, and new password.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task operationResult contains a <see cref="OperationResult"/> indicating success or failure.</returns>
        Task<OperationResult> ResetPasswordWithTokenAsync(ResetPasswordWithTokenCommand command, CancellationToken cancellationToken);

        /// <summary>
        /// Deletes a user account asynchronously.
        /// </summary>
        /// <param name="command">The command containing the ID of the user to delete.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task operationResult contains a <see cref="OperationResult"/> indicating success or failure.</returns>
        Task<OperationResult> DeleteUserAsync(DeleteUserCommand command, CancellationToken cancellationToken);
    }
}
