// <copyright file="MediaImage.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

using ShelfKeeper.Domain.Common;
using ShelfKeeper.Shared.Common;
using System.Collections.Generic;
using System.Linq;

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

        /// <summary>
        /// Validates the media image entity properties.
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

            if (MediaItemId == Guid.Empty)
            {
                errors.Add(new OperationError("Media item ID cannot be empty.", OperationErrorType.ValidationError));
            }

            if (string.IsNullOrWhiteSpace(ImageUrl))
            {
                errors.Add(new OperationError("Image URL cannot be empty.", OperationErrorType.ValidationError));
            }

            if (errors.Any())
            {
                return OperationResult.Failure(errors);
            }

            return OperationResult.Success();
        }
    }
}
