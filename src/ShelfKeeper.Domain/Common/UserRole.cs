// <copyright file="UserRole.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

namespace ShelfKeeper.Domain.Common
{
    /// <summary>
    /// Defines the roles a user can have within the application.
    /// </summary>
    public enum UserRole
    {
        /// <summary>
        /// Standard user with limited access.
        /// </summary>
        User = 0,

        /// <summary>
        /// Administrator with full access.
        /// </summary>
        Admin = 1,
    }
}
