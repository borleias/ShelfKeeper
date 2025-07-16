// <copyright file="IEmailService.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

using ShelfKeeper.Shared.Common;

namespace ShelfKeeper.Application.Interfaces
{
    /// <summary>
    /// Defines the interface for an email sending service.
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Sends an email asynchronously.
        /// </summary>
        /// <param name="to">The recipient's email address.</param>
        /// <param name="subject">The subject of the email.</param>
        /// <param name="body">The body of the email.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains a <see cref="OperationResult"/> indicating success or failure.</returns>
        Task<OperationResult> SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken);
    }
}
