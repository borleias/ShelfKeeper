// <copyright file="MediaTag.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

using ShelfKeeper.Domain.Common;

namespace ShelfKeeper.Domain.Entities
{
    /// <summary>
    /// Represents a user-defined tag for media items.
    /// </summary>
    public class MediaTag : AuditableEntity
    {
        /// <summary>
        /// Gets or sets the ID of the user who owns this media tag.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Gets or sets the user who owns this media tag.
        /// </summary>
        public User User { get; set; }

        /// <summary>
        /// Gets or sets the name of the media tag.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the collection of media item tags associated with this media tag.
        /// </summary>
        public ICollection<MediaItemTag> MediaItemTags { get; set; }
    }
}