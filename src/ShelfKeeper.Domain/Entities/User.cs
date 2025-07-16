// <copyright file="User.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

using ShelfKeeper.Domain.Common;
using ShelfKeeper.Shared.Common;
using System.Collections.Generic;
using System.Linq;

namespace ShelfKeeper.Domain.Entities
{
    /// <summary>
    /// Represents a user of the ShelfKeeper application.
    /// </summary>
    public class User : AuditableEntity
    {
        /// <summary>
        /// Gets or sets the email address of the user.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the hashed password of the user.
        /// </summary>
        public string PasswordHash { get; set; }

        /// <summary>
        /// Gets or sets the name of the user.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the password reset token.
        /// </summary>
        public string? PasswordResetToken { get; set; }

        /// <summary>
        /// Gets or sets the expiration date of the password reset token.
        /// </summary>
        public DateTime? PasswordResetTokenExpiration { get; set; }

        /// <summary>
        /// Gets or sets the collection of media items owned by this user.
        /// </summary>
        public ICollection<MediaItem> MediaItems { get; set; }

        /// <summary>
        /// Gets or sets the collection of subscriptions associated with this user.
        /// </summary>
        public ICollection<Subscription> Subscriptions { get; set; }

        /// <summary>
        /// Gets or sets the collection of locations defined by this user.
        /// </summary>
        public ICollection<Location> Locations { get; set; }

        /// <summary>
        /// Gets or sets the collection of media tags defined by this user.
        /// </summary>
        public ICollection<MediaTag> MediaTags { get; set; }

        /// <summary>
        /// Validates the user entity properties.
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

            if (string.IsNullOrWhiteSpace(Email))
            {
                errors.Add(new OperationError("User email cannot be empty.", OperationErrorType.ValidationError));
            }

            if (string.IsNullOrWhiteSpace(PasswordHash))
            {
                errors.Add(new OperationError("User password hash cannot be empty.", OperationErrorType.ValidationError));
            }

            if (string.IsNullOrWhiteSpace(Name))
            {
                errors.Add(new OperationError("User name cannot be empty.", OperationErrorType.ValidationError));
            }

            if (errors.Any())
            {
                return OperationResult.Failure(errors);
            }

            return OperationResult.Success();
        }
    }
}
