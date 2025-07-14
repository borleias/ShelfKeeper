// <copyright file="MediaTag.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

using ShelfKeeper.Domain.Common;
using ShelfKeeper.Shared.Common;
using System.Collections.Generic;
using System.Linq;

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

        /// <summary>
        /// Validates the media tag entity properties.
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

            if (UserId == Guid.Empty)
            {
                errors.Add(new OperationError("User ID cannot be empty.", OperationErrorType.ValidationError));
            }

            if (string.IsNullOrWhiteSpace(Name))
            {
                errors.Add(new OperationError("Media tag name cannot be empty.", OperationErrorType.ValidationError));
            }

            if (errors.Any())
            {
                return OperationResult.Failure(errors);
            }

            return OperationResult.Success();
        }
    }
}