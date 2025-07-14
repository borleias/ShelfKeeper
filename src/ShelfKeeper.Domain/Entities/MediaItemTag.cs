// <copyright file="MediaItemTag.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

using ShelfKeeper.Shared.Common;
using System.Collections.Generic;
using System.Linq;

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

        /// <summary>
        /// Validates the media item tag entity properties.
        /// </summary>
        /// <returns>A <see cref="OperationResult"/> indicating the success or failure of the validation.</returns>
        public OperationResult Validate()
        {
            List<OperationError> errors = new List<OperationError>();

            if (MediaItemId == Guid.Empty)
            {
                errors.Add(new OperationError("Media item ID cannot be empty.", OperationErrorType.ValidationError));
            }

            if (MediaTagId == Guid.Empty)
            {
                errors.Add(new OperationError("Media tag ID cannot be empty.", OperationErrorType.ValidationError));
            }

            if (errors.Any())
            {
                return OperationResult.Failure(errors);
            }

            return OperationResult.Success();
        }
    }
}
