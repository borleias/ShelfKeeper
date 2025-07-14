// <copyright file="IPasswordHasher.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

namespace ShelfKeeper.Application.Interfaces
{
    /// <summary>
    /// Defines the interface for a password hashing service.
    /// </summary>
    public interface IPasswordHasher
    {
        /// <summary>
        /// Hashes a plain-text password.
        /// </summary>
        /// <param name="password">The plain-text password to hash.</param>
        /// <returns>The hashed password string.</returns>
        string HashPassword(string password);

        /// <summary>
        /// Verifies a plain-text password against a hashed password.
        /// </summary>
        /// <param name="password">The plain-text password to verify.</param>
        /// <param name="hashedPassword">The hashed password to compare against.</param>
        /// <returns><c>true</c> if the password matches the hashed password; otherwise, <c>false</c>.</returns>
        bool VerifyPassword(string password, string hashedPassword);
    }
}