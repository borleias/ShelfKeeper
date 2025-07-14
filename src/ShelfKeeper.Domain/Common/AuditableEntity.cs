// <copyright file="AuditableEntity.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

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
    }
}