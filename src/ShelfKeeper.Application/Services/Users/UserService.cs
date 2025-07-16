// <copyright file="UserService.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using ShelfKeeper.Application.Interfaces;
using ShelfKeeper.Domain.Entities;
using ShelfKeeper.Application.Services.Users.Models;
using ShelfKeeper.Shared.Common;
using System.Security.Cryptography;

namespace ShelfKeeper.Application.Services.Users
{
    /// <summary>
    /// Provides services for user management, including creation, login, password reset, and deletion.
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IApplicationDbContext _context;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtService _jwtService;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserService"/> class.
        /// </summary>
        /// <param name="context">The application database context.</param>
        /// <param name="passwordHasher">The password hashing service.</param>
        /// <param name="jwtService">The JWT token generation service.</param>
        public UserService(IApplicationDbContext context, IPasswordHasher passwordHasher, IJwtService jwtService)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _jwtService = jwtService;
        }

        /// <summary>
        /// Creates a new user account.
        /// </summary>
        public async Task<OperationResult<CreateUserResponse>> CreateUserAsync(CreateUserCommand command, CancellationToken cancellationToken)
        {
            User user = new User
            {
                Email = command.Email,
                PasswordHash = _passwordHasher.HashPassword(command.Password),
                Name = command.Name,
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow
            };

            OperationResult validationOperationResult = user.Validate();
            if (validationOperationResult.IsFailure)
            {
                return OperationResult<CreateUserResponse>.Failure(validationOperationResult.Errors);
            }

            _context.Users.Add(user);
            await _context.SaveChangesAsync(cancellationToken);

            return OperationResult<CreateUserResponse>.Success(new CreateUserResponse(user.Id, user.Email, user.Name));
        }

        /// <summary>
        /// Authenticates a user and generates a JWT token upon successful login.
        /// </summary>
        public async Task<OperationResult<LoginUserResponse>> LoginUserAsync(LoginUserQuery query, CancellationToken cancellationToken)
        {
            User user = await _context.Users.FirstOrDefaultAsync(u => u.Email == query.Email, cancellationToken);

            if (user == null || !_passwordHasher.VerifyPassword(query.Password, user.PasswordHash))
            {
                return OperationResult<LoginUserResponse>.Failure("Invalid credentials.", OperationErrorType.UnauthorizedError);
            }

            string token = _jwtService.GenerateToken(user.Id, user.Email, user.Name);

            return OperationResult<LoginUserResponse>.Success(new LoginUserResponse(user.Id, user.Email, user.Name, token));
        }

        /// <summary>
        /// Changes a user's password.
        /// </summary>
        public async Task<OperationResult> ChangePasswordAsync(ChangePasswordCommand command, CancellationToken cancellationToken)
        {
            User user = await _context.Users.FindAsync(new object[] { command.UserId }, cancellationToken);

            if (user == null)
            {
                return OperationResult.Failure("User not found.", OperationErrorType.NotFoundError);
            }

            if (!_passwordHasher.VerifyPassword(command.OldPassword, user.PasswordHash))
            {
                return OperationResult.Failure("Invalid old password.", OperationErrorType.UnauthorizedError);
            }

            user.PasswordHash = _passwordHasher.HashPassword(command.NewPassword);
            user.LastUpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return OperationResult.Success();
        }

        /// <summary>
        /// Initiates the password reset process for a user.
        /// </summary>
        public async Task<OperationResult> ForgotPasswordAsync(ForgotPasswordCommand command, CancellationToken cancellationToken)
        {
            User user = await _context.Users.FirstOrDefaultAsync(u => u.Email == command.Email, cancellationToken);

            if (user != null)
            {
                user.PasswordResetToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
                user.PasswordResetTokenExpiration = DateTime.UtcNow.AddHours(1); // Token valid for 1 hour
                await _context.SaveChangesAsync(cancellationToken);
            }

            // For security reasons, always return success to prevent email enumeration
            return OperationResult.Success();
        }

        /// <summary>
        /// Resets a user's password using a reset token.
        /// </summary>
        public async Task<OperationResult> ResetPasswordWithTokenAsync(ResetPasswordWithTokenCommand command, CancellationToken cancellationToken)
        {
            User user = await _context.Users.FirstOrDefaultAsync(u => u.Email == command.Email && u.PasswordResetToken == command.Token, cancellationToken);

            if (user == null || user.PasswordResetTokenExpiration <= DateTime.UtcNow)
            {
                return OperationResult.Failure("Invalid token.", OperationErrorType.ValidationError);
            }

            user.PasswordHash = _passwordHasher.HashPassword(command.NewPassword);
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiration = null;
            user.LastUpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return OperationResult.Success();
        }

        /// <summary>
        /// Deletes a user account.
        /// </summary>
        public async Task<OperationResult> DeleteUserAsync(DeleteUserCommand command, CancellationToken cancellationToken)
        {
            User user = await _context.Users.FindAsync(new object[] { command.UserId }, cancellationToken);

            if (user == null)
            {
                return OperationResult.Failure("User not found.", OperationErrorType.NotFoundError);
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync(cancellationToken);

            return OperationResult.Success();
        }
    }

    public record ChangePasswordCommand(Guid UserId, string OldPassword, string NewPassword);
    public record ForgotPasswordCommand(string Email);
    public record ResetPasswordWithTokenCommand(string Token, string Email, string NewPassword);
}
