// <copyright file="MediaImage.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

using ShelfKeeper.Domain.Common;

namespace ShelfKeeper.Domain.Entities
{
    /// <summary>
    /// Represents an image associated with a media item (e.g., cover art).
    /// </summary>
    public class MediaImage : AuditableEntity
    {
        /// <summary>
        /// Gets or sets the ID of the media item this image belongs to.
        /// </summary>
        public Guid MediaItemId { get; set; }

        /// <summary>
        /// Gets or sets the associated media item.
        /// </summary>
        public MediaItem MediaItem { get; set; }

        /// <summary>
        /// Gets or sets the URL of the image.
        /// </summary>
        public string ImageUrl { get; set; }
    }
}