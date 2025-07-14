// <copyright file="MediaItemTag.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

namespace ShelfKeeper.Domain.Entities
{
    /// <summary>
    /// Represents the many-to-many relationship between a media item and a media tag.
    /// </summary>
    public class MediaItemTag
    {
        /// <summary>
        /// Gets or sets the ID of the media item.
        /// </summary>
        public Guid MediaItemId { get; set; }

        /// <summary>
        /// Gets or sets the associated media item.
        /// </summary>
        public MediaItem MediaItem { get; set; }

        /// <summary>
        /// Gets or sets the ID of the media tag.
        /// </summary>
        public Guid MediaTagId { get; set; }

        /// <summary>
        /// Gets or sets the associated media tag.
        /// </summary>
        public MediaTag MediaTag { get; set; }
    }
}