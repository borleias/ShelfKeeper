// <copyright file="User.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

using ShelfKeeper.Domain.Common;

namespace ShelfKeeper.Domain.Entities
{
    /// <summary>
    /// Represents a user of the ShelfKeeper application.
    /// </summary>
    public class User : AuditableEntity
    {
        /// <summary>
        /// Gets or sets the email address of the user.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the hashed password of the user.
        /// </summary>
        public string PasswordHash { get; set; }

        /// <summary>
        /// Gets or sets the name of the user.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the collection of media items owned by this user.
        /// </summary>
        public ICollection<MediaItem> MediaItems { get; set; }

        /// <summary>
        /// Gets or sets the collection of subscriptions associated with this user.
        /// </summary>
        public ICollection<Subscription> Subscriptions { get; set; }

        /// <summary>
        /// Gets or sets the collection of locations defined by this user.
        /// </summary>
        public ICollection<Location> Locations { get; set; }

        /// <summary>
        /// Gets or sets the collection of media tags defined by this user.
        /// </summary>
        public ICollection<MediaTag> MediaTags { get; set; }
    }
}