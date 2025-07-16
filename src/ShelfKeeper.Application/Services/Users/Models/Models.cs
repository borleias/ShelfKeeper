// <copyright file="Models.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

namespace ShelfKeeper.Application.Services.Users.Models
{
    /// <summary>
    /// Represents a command to create a new user.
    /// </summary>
    /// <param name="Email">The email address of the user.</param>
    /// <param name="Password">The password for the user account.</param>
    /// <param name="Name">The name of the user.</param>
    public record CreateUserCommand(string Email, string Password, string Name);

    /// <summary>
    /// Represents the response after creating a user.
    /// </summary>
    /// <param name="UserId">The unique identifier of the newly created user.</param>
    /// <param name="Email">The email address of the newly created user.</param>
    /// <param name="Name">The name of the newly created user.</param>
    public record CreateUserResponse(Guid UserId, string Email, string Name);

    /// <summary>
    /// Represents a query to log in a user.
    /// </summary>
    /// <param name="Email">The email address of the user.</param>
    /// <param name="Password">The password for the user account.</param>
    public record LoginUserQuery(string Email, string Password);

    /// <summary>
    /// Represents the response after a successful user login.
    /// </summary>
    /// <param name="UserId">The unique identifier of the logged-in user.</param>
    /// <param name="Email">The email address of the logged-in user.</param>
    /// <param name="Name">The name of the logged-in user.</param>
    /// <param name="Token">The JWT token for authentication.</param>
    public record LoginUserResponse(Guid UserId, string Email, string Name, string Token);

    /// <summary>
    /// Represents a command to reset a user's password.
    /// </summary>
    /// <param name="Email">The email address of the user whose password is to be reset.</param>
    /// <param name="NewPassword">The new password for the user account.</param>
    public record ResetPasswordCommand(string Email, string NewPassword);

    /// <summary>
    /// Represents a command to delete a user account.
    /// </summary>
    /// <param name="UserId">The unique identifier of the user to delete.</param>
    public record DeleteUserCommand(Guid UserId);

    /// <summary>
    /// Represents a data transfer object for user information.
    /// </summary>
    /// <param name="UserId">The unique identifier of the user.</param>
    /// <param name="Email">The email address of the user.</param>
    /// <param name="Name">The name of the user.</param>
    /// <param name="Role">The role of the user.</param>
    public record UserDto(Guid UserId, string Email, string Name, string Role);

    /// <summary>
    /// Represents a command to update a user's role.
    /// </summary>
    /// <param name="UserId">The ID of the user whose role is to be updated.</param>
    /// <param name="NewRole">The new role for the user.</param>
    public record UpdateUserRoleCommand(Guid UserId, string NewRole);

    /// <summary>
    /// Represents a command to update a user's data as an administrator.
    /// </summary>
    /// <param name="UserId">The ID of the user to update.</param>
    /// <param name="Email">The new email address of the user.</param>
    /// <param name="Name">The new name of the user.</param>
    public record UpdateUserCommand(Guid UserId, string Email, string Name);

    /// <summary>
    /// Represents a command to reset a user's password as an administrator.
    /// </summary>
    /// <param name="UserId">The ID of the user whose password is to be reset.</param>
    /// <param name="NewPassword">The new password for the user.</param>
    public record AdminResetPasswordCommand(Guid UserId, string NewPassword);
}