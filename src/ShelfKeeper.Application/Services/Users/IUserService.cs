// <copyright file="IUserService.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

using ShelfKeeper.Application.Services.Users.Models;

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
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains the response with the new user's details.</returns>
        Task<CreateUserResponse> CreateUserAsync(CreateUserCommand command, CancellationToken cancellationToken);

        /// <summary>
        /// Authenticates a user and generates a JWT token upon successful login asynchronously.
        /// </summary>
        /// <param name="query">The query containing user login credentials.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains the response with user details and a JWT token.</returns>
        Task<LoginUserResponse> LoginUserAsync(LoginUserQuery query, CancellationToken cancellationToken);

        /// <summary>
        /// Resets a user's password asynchronously.
        /// </summary>
        /// <param name="command">The command containing the user's email and new password.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task ResetPasswordAsync(ResetPasswordCommand command, CancellationToken cancellationToken);

        /// <summary>
        /// Deletes a user account asynchronously.
        /// </summary>
        /// <param name="command">The command containing the ID of the user to delete.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task DeleteUserAsync(DeleteUserCommand command, CancellationToken cancellationToken);
    }
}