// <copyright file="AdminUserService.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using ShelfKeeper.Application.Interfaces;
using ShelfKeeper.Domain.Entities;
using ShelfKeeper.Application.Services.Users.Models;
using ShelfKeeper.Shared.Common;
using ShelfKeeper.Domain.Common;

namespace ShelfKeeper.Application.Services.Users
{
    /// <summary>
    /// Provides administrator-level services for user management.
    /// </summary>
    public class AdminUserService : IAdminUserService
    {
        private readonly IApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdminUserService"/> class.
        /// </summary>
        /// <param name="context">The application database context.</param>
        public AdminUserService(IApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves a list of all users asynchronously.
        /// </summary>
        public async Task<OperationResult<List<UserDto>>> GetAllUsersAsync(CancellationToken cancellationToken)
        {
            List<UserDto> users = await _context.Users
                .Select(u => new UserDto(u.Id, u.Email, u.Name, u.Role.ToString()))
                .ToListAsync(cancellationToken);

            return OperationResult<List<UserDto>>.Success(users);
        }

        /// <summary>
        /// Retrieves details of a specific user by ID asynchronously.
        /// </summary>
        public async Task<OperationResult<UserDto>> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            User user = await _context.Users.FindAsync(new object[] { userId }, cancellationToken);

            if (user == null)
            {
                return OperationResult<UserDto>.Failure("User not found.", OperationErrorType.NotFoundError);
            }

            return OperationResult<UserDto>.Success(new UserDto(user.Id, user.Email, user.Name, user.Role.ToString()));
        }

        /// <summary>
        /// Updates the role of a specific user asynchronously.
        /// </summary>
        public async Task<OperationResult> UpdateUserRoleAsync(Guid userId, UserRole newRole, CancellationToken cancellationToken)
        {
            User user = await _context.Users.FindAsync(new object[] { userId }, cancellationToken);

            if (user == null)
            {
                return OperationResult.Failure("User not found.", OperationErrorType.NotFoundError);
            }

            user.Role = newRole;
            user.LastUpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return OperationResult.Success();
        }

        /// <summary>
        /// Deletes a specific user by ID asynchronously.
        /// </summary>
        public async Task<OperationResult> DeleteUserAsAdminAsync(Guid userId, CancellationToken cancellationToken)
        {
            User user = await _context.Users.FindAsync(new object[] { userId }, cancellationToken);

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
