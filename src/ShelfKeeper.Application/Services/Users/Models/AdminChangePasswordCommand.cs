// <copyright file="AdminChangePasswordCommand.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

namespace ShelfKeeper.Application.Services.Users.Models
{
    /// <summary>
    /// Command to change a user's password by an administrator.
    /// </summary>
    /// <param name="UserId">The ID of the user whose password is to be changed.</param>
    /// <param name="NewPassword">The new password for the user.</param>
    public record AdminChangePasswordCommand(Guid UserId, string NewPassword);
}
