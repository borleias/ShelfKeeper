// <copyright file="Author.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

using ShelfKeeper.Domain.Common;
using ShelfKeeper.Shared.Common;
using System.Collections.Generic;
using System.Linq;

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

        /// <summary>
        /// Validates the author entity properties.
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

            if (string.IsNullOrWhiteSpace(Name))
            {
                errors.Add(new OperationError("Author name cannot be empty.", OperationErrorType.ValidationError));
            }

            if (errors.Any())
            {
                return OperationResult.Failure(errors);
            }

            return OperationResult.Success();
        }
    }
}