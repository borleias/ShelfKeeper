// <copyright file="UserService.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using ShelfKeeper.Application.Interfaces;
using ShelfKeeper.Domain.Entities;
using ShelfKeeper.Application.Services.Users.Models;
using ShelfKeeper.Shared.Common;

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
        /// <param name="command">The command containing user registration details.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task operationResult contains a <see cref="OperationResult{TValue}"/> with the new user's details.</returns>
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
        /// <param name="query">The query containing user login credentials.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task operationResult contains a <see cref="OperationResult{TValue}"/> with user details and a JWT token.</returns>
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
        /// Resets a user's password.
        /// </summary>
        /// <param name="command">The command containing the user's email and new password.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task operationResult contains a <see cref="OperationResult"/> indicating success or failure.</returns>
        public async Task<OperationResult> ResetPasswordAsync(ResetPasswordCommand command, CancellationToken cancellationToken)
        {
            User user = await _context.Users.FirstOrDefaultAsync(u => u.Email == command.Email, cancellationToken);

            if (user == null)
            {
                return OperationResult.Failure("User not found.", OperationErrorType.NotFoundError); // For security reasons, do not reveal if user exists or not
            }

            user.PasswordHash = _passwordHasher.HashPassword(command.NewPassword);
            user.LastUpdatedAt = DateTime.UtcNow;

            OperationResult validationOperationResult = user.Validate();
            if (validationOperationResult.IsFailure)
            {
                return validationOperationResult;
            }

            await _context.SaveChangesAsync(cancellationToken);

            return OperationResult.Success();
        }

        /// <summary>
        /// Deletes a user account.
        /// </summary>
        /// <param name="command">The command containing the ID of the user to delete.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task operationResult contains a <see cref="OperationResult"/> indicating success or failure.</returns>
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
}
