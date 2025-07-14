// <copyright file="Author.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

using ShelfKeeper.Domain.Common;

namespace ShelfKeeper.Domain.Entities
{
    /// <summary>
    /// Represents an author or artist of a media item.
    /// </summary>
    public class Author : AuditableEntity
    {
        /// <summary>
        /// Gets or sets the name of the author or artist.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the collection of media items associated with this author.
        /// </summary>
        public ICollection<MediaItem> MediaItems { get; set; }
    }
}