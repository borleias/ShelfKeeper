// <copyright file="IAdminUserService.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

using ShelfKeeper.Application.Services.Users.Models;
using ShelfKeeper.Shared.Common;
using ShelfKeeper.Domain.Common;

namespace ShelfKeeper.Application.Services.Users
{
    /// <summary>
    /// Defines the interface for administrator-level user management services.
    /// </summary>
    public interface IAdminUserService
    {
        /// <summary>
        /// Retrieves a list of all users asynchronously.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains a <see cref="OperationResult{TValue}"/> with a list of all users.</returns>
        Task<OperationResult<List<UserDto>>> GetAllUsersAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves details of a specific user by ID asynchronously.
        /// </summary>
        /// <param name="userId">The ID of the user to retrieve.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains a <see cref="OperationResult{TValue}"/> with the user's details, or null if not found.</returns>
        Task<OperationResult<UserDto>> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken);

        /// <summary>
        /// Updates the role of a specific user asynchronously.
        /// </summary>
        /// <param name="userId">The ID of the user to update.</param>
        /// <param name="newRole">The new role for the user.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains a <see cref="OperationResult"/> indicating success or failure.</returns>
        Task<OperationResult> UpdateUserRoleAsync(Guid userId, UserRole newRole, CancellationToken cancellationToken);

        /// <summary>
        /// Deletes a specific user by ID asynchronously.
        /// </summary>
        /// <param name="userId">The ID of the user to delete.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains a <see cref="OperationResult"/> indicating success or failure.</returns>
        Task<OperationResult> DeleteUserAsAdminAsync(Guid userId, CancellationToken cancellationToken);

        /// <summary>
        /// Updates a user's data as an administrator asynchronously.
        /// </summary>
        /// <param name="command">The command containing the user's updated details.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains a <see cref="OperationResult"/> indicating success or failure.</returns>
        Task<OperationResult> UpdateUserAsAdminAsync(UpdateUserCommand command, CancellationToken cancellationToken);

        /// <summary>
        /// Resets a user's password as an administrator asynchronously.
        /// </summary>
        /// <param name="command">The command containing the user's ID and new password.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains a <see cref="OperationResult"/> indicating success or failure.</returns>
        Task<OperationResult> AdminResetPasswordAsync(AdminResetPasswordCommand command, CancellationToken cancellationToken);

        /// <summary>
        /// Changes a user's password as an administrator asynchronously.
        /// </summary>
        /// <param name="command">The command containing the user's ID and new password.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains a <see cref="OperationResult"/> indicating success or failure.</returns>
        Task<OperationResult> ChangeUserPasswordAsAdminAsync(AdminChangePasswordCommand command, CancellationToken cancellationToken);
    }
}
