// <copyright file="Location.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

using ShelfKeeper.Domain.Common;
using ShelfKeeper.Shared.Common;
using System.Collections.Generic;
using System.Linq;

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

        /// <summary>
        /// Validates the location entity properties.
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
                errors.Add(new OperationError("Location title cannot be empty.", OperationErrorType.ValidationError));
            }

            if (errors.Any())
            {
                return OperationResult.Failure(errors);
            }

            return OperationResult.Success();
        }
    }
}
