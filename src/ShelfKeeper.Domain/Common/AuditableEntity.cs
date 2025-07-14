// <copyright file="AuditableEntity.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

using ShelfKeeper.Shared.Common;
using System.Collections.Generic;
using System.Linq;

namespace ShelfKeeper.Domain.Common
{
    /// <summary>
    /// Represents an abstract base class for auditable entities, providing common properties for tracking creation and update times.
    /// </summary>
    public abstract class AuditableEntity
    {
        /// <summary>
        /// Gets or sets the unique identifier for the entity.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the entity was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the entity was last updated.
        /// </summary>
        public DateTime LastUpdatedAt { get; set; }

        /// <summary>
        /// Validates the auditable entity properties.
        /// </summary>
        /// <returns>A <see cref="OperationResult"/> indicating the success or failure of the validation.</returns>
        public OperationResult Validate()
        {
            List<OperationError> errors = new List<OperationError>();

            if (Id == Guid.Empty)
            {
                errors.Add(new OperationError("Entity Id cannot be empty.", OperationErrorType.ValidationError));
            }

            if (CreatedAt == default(DateTime))
            {
                errors.Add(new OperationError("CreatedAt date cannot be default.", OperationErrorType.ValidationError));
            }

            if (LastUpdatedAt == default(DateTime))
            {
                errors.Add(new OperationError("LastUpdatedAt date cannot be default.", OperationErrorType.ValidationError));
            }

            if (LastUpdatedAt < CreatedAt)
            {
                errors.Add(new OperationError("LastUpdatedAt cannot be before CreatedAt.", OperationErrorType.ValidationError));
            }

            if (errors.Any())
            {
                return OperationResult.Failure(errors);
            }

            return OperationResult.Success();
        }
    }
}