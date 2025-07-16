// <copyright file="DummyEmailService.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

using ShelfKeeper.Application.Interfaces;
using ShelfKeeper.Shared.Common;
using Microsoft.Extensions.Logging;

namespace ShelfKeeper.Infrastructure.Services
{
    /// <summary>
    /// A dummy email service that logs email attempts instead of sending them.
    /// </summary>
    public class DummyEmailService : IEmailService
    {
        private readonly ILogger<DummyEmailService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DummyEmailService"/> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        public DummyEmailService(ILogger<DummyEmailService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Sends an email asynchronously (logs the email instead).
        /// </summary>
        public Task<OperationResult> SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Sending email to: {To}, Subject: {Subject}, Body: {Body}", to, subject, body);
            return Task.FromResult(OperationResult.Success());
        }
    }
}
