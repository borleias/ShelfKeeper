// <copyright file="MediaItem.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

using ShelfKeeper.Domain.Common;

namespace ShelfKeeper.Domain.Entities
{
    /// <summary>
    /// Represents a media item in the user's library.
    /// </summary>
    public class MediaItem : AuditableEntity
    {
        /// <summary>
        /// Gets or sets the ID of the user who owns this media item.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Gets or sets the user who owns this media item.
        /// </summary>
        public User User { get; set; }

        /// <summary>
        /// Gets or sets the title of the media item.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the type of the media item (e.g., Book, CD, DVD).
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the publication or release year of the media item.
        /// </summary>
        public int? Year { get; set; }

        /// <summary>
        /// Gets or sets the ISBN or UPC of the media item.
        /// </summary>
        public string? IsbnUpc { get; set; }

        /// <summary>
        /// Gets or sets personal notes about the media item.
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// Gets or sets the current progress (e.g., page number for a book).
        /// </summary>
        public string? Progress { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the media item was added.
        /// </summary>
        public DateTime AddedAt { get; set; }

        /// <summary>
        /// Gets or sets the optional ID of the physical location of the media item.
        /// </summary>
        public Guid? LocationId { get; set; }

        /// <summary>
        /// Gets or sets the physical location of the media item.
        /// </summary>
        public Location Location { get; set; }

        /// <summary>
        /// Gets or sets the optional ID of the author or artist of the media item.
        /// </summary>
        public Guid? AuthorId { get; set; }

        /// <summary>
        /// Gets or sets the author or artist of the media item.
        /// </summary>
        public Author Author { get; set; }

        /// <summary>
        /// Gets or sets the collection of images associated with this media item.
        /// </summary>
        public ICollection<MediaImage> MediaImages { get; set; }

        /// <summary>
        /// Gets or sets the collection of media item tags associated with this media item.
        /// </summary>
        public ICollection<MediaItemTag> MediaItemTags { get; set; }
    }
}