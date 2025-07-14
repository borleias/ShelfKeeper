// <copyright file="Location.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

using ShelfKeeper.Domain.Common;

namespace ShelfKeeper.Domain.Entities
{
    /// <summary>
    /// Represents a physical location where media items are stored.
    /// </summary>
    public class Location : AuditableEntity
    {
        /// <summary>
        /// Gets or sets the ID of the user who owns this location.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Gets or sets the user who owns this location.
        /// </summary>
        public User User { get; set; }

        /// <summary>
        /// Gets or sets the title of the location (e.g., "Living Room Shelf").
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the description of the location.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the collection of media items stored at this location.
        /// </summary>
        public ICollection<MediaItem> MediaItems { get; set; }
    }
}