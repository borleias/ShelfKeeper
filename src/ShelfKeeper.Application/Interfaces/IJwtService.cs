// <copyright file="IJwtService.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

namespace ShelfKeeper.Application.Interfaces
{
    /// <summary>
    /// Defines the interface for a JSON Web Token (JWT) service.
    /// </summary>
    public interface IJwtService
    {
        /// <summary>
        /// Generates a JWT token for the specified user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="email">The email of the user.</param>
        /// <param name="name">The name of the user.</param>
        /// <param name="role">The role of the user.</param>
        /// <returns>A string representing the generated JWT token.</returns>
        string GenerateToken(Guid userId, string email, string name, string role);
    }
}