// <copyright file="MediaItem.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

using ShelfKeeper.Domain.Common;
using ShelfKeeper.Shared.Common;
using System.Collections.Generic;
using System.Linq;

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

        /// <summary>
        /// Validates the media item entity properties.
        /// </summary>
        /// <returns>A <see cref="OperationResult"/> indicating the success or failure of the validation.</returns>
        public new OperationResult Validate()
        {
            List<OperationError> errors = new List<OperationError>();

            OperationResult baseValidation = base.Validate();
            if (baseValidation.IsFailure)
            {
                errors.AddRange(baseValidation.Errors);
            }

            if (string.IsNullOrWhiteSpace(Title))
            {
                errors.Add(new OperationError("Media item title cannot be empty.", OperationErrorType.ValidationError));
            }

            if (string.IsNullOrWhiteSpace(Type))
            {
                errors.Add(new OperationError("Media item type cannot be empty.", OperationErrorType.ValidationError));
            }

            if (errors.Any())
            {
                return OperationResult.Failure(errors);
            }

            return OperationResult.Success();
        }
    }
}
