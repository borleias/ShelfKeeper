// <copyright file="AdminController.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using ShelfKeeper.Application.Services.Users;
using ShelfKeeper.Application.Services.Users.Models;
using ShelfKeeper.Shared.Common;
using ShelfKeeper.Domain.Common;

namespace ShelfKeeper.WebApi.Controllers
{
    /// <summary>
    /// API controller for administrator-level user management.
    /// </summary>
    [ApiController]
    [Authorize(Policy = "AdminOnly")] // Only administrators can access this controller
    [Asp.Versioning.ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/admin/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminUserService _adminUserService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdminController"/> class.
        /// </summary>
        /// <param name="adminUserService">The admin user service.</param>
        public AdminController(IAdminUserService adminUserService)
        {
            _adminUserService = adminUserService;
        }

        /// <summary>
        /// Retrieves a list of all users.
        /// </summary>
        /// <returns>A list of user details.</returns>
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            OperationResult<List<UserDto>> operationResult = await _adminUserService.GetAllUsersAsync(CancellationToken.None);
            if (operationResult.IsFailure)
            {
                return BadRequest(operationResult.Errors);
            }
            return Ok(operationResult.Value);
        }

        /// <summary>
        /// Retrieves details of a specific user by ID.
        /// </summary>
        /// <param name="id">The ID of the user.</param>
        /// <returns>User details.</returns>
        [HttpGet("users/{id}")]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            OperationResult<UserDto> operationResult = await _adminUserService.GetUserByIdAsync(id, CancellationToken.None);
            if (operationResult.IsFailure)
            {
                if (operationResult.Errors.Any(e => e.Type == OperationErrorType.NotFoundError))
                {
                    return NotFound(operationResult.Errors);
                }
                return BadRequest(operationResult.Errors);
            }
            return Ok(operationResult.Value);
        }

        /// <summary>
        /// Updates the role of a specific user.
        /// </summary>
        /// <param name="id">The ID of the user to update.</param>
        /// <param name="command">The command containing the new role.</param>
        /// <returns>No content.</returns>
        [HttpPut("users/{id}/role")]
        public async Task<IActionResult> UpdateUserRole(Guid id, [FromBody] UpdateUserRoleCommand command)
        {
            if (id != command.UserId)
            {
                return BadRequest();
            }

            if (!Enum.TryParse(typeof(UserRole), command.NewRole, true, out object? newRoleEnum) || newRoleEnum == null)
            {
                return BadRequest(OperationResult.Failure("Invalid role specified.", OperationErrorType.ValidationError).Errors);
            }

            OperationResult operationResult = await _adminUserService.UpdateUserRoleAsync(id, (UserRole)newRoleEnum, CancellationToken.None);
            if (operationResult.IsFailure)
            {
                if (operationResult.Errors.Any(e => e.Type == OperationErrorType.NotFoundError))
                {
                    return NotFound(operationResult.Errors);
                }
                return BadRequest(operationResult.Errors);
            }
            return NoContent();
        }

        /// <summary>
        /// Deletes a specific user by ID.
        /// </summary>
        /// <param name="id">The ID of the user to delete.</param>
        /// <returns>No content.</returns>
        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            OperationResult operationResult = await _adminUserService.DeleteUserAsAdminAsync(id, CancellationToken.None);
            if (operationResult.IsFailure)
            {
                if (operationResult.Errors.Any(e => e.Type == OperationErrorType.NotFoundError))
                {
                    return NotFound(operationResult.Errors);
                }
                return BadRequest(operationResult.Errors);
            }
            return NoContent();
        }

        /// <summary>
        /// Updates a user's data as an administrator.
        /// </summary>
        /// <param name="id">The ID of the user to update.</param>
        /// <param name="command">The command containing the user's updated details.</param>
        /// <returns>No content.</returns>
        [HttpPut("users/{id}")]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserCommand command)
        {
            if (id != command.UserId)
            {
                return BadRequest();
            }

            OperationResult operationResult = await _adminUserService.UpdateUserAsAdminAsync(command, CancellationToken.None);
            if (operationResult.IsFailure)
            {
                if (operationResult.Errors.Any(e => e.Type == OperationErrorType.NotFoundError))
                {
                    return NotFound(operationResult.Errors);
                }
                return BadRequest(operationResult.Errors);
            }
            return NoContent();
        }

        /// <summary>
        /// Resets a user's password as an administrator.
        /// </summary>
        /// <param name="id">The ID of the user whose password is to be reset.</param>
        /// <param name="command">The command containing the user's new password.</param>
        /// <returns>No content.</returns>
        [HttpPost("users/{id}/reset-password")]
        public async Task<IActionResult> AdminResetPassword(Guid id, [FromBody] AdminResetPasswordCommand command)
        {
            if (id != command.UserId)
            {
                return BadRequest();
            }

            OperationResult operationResult = await _adminUserService.AdminResetPasswordAsync(command, CancellationToken.None);
            if (operationResult.IsFailure)
            {
                if (operationResult.Errors.Any(e => e.Type == OperationErrorType.NotFoundError))
                {
                    return NotFound(operationResult.Errors);
                }
                return BadRequest(operationResult.Errors);
            }
            return NoContent();
        }

        /// <summary>
        /// Changes a user's password as an administrator.
        /// </summary>
        /// <param name="id">The ID of the user whose password is to be changed.</param>
        /// <param name="command">The command containing the user's new password.</param>
        /// <returns>No content.</returns>
        [HttpPut("users/{id}/password")]
        public async Task<IActionResult> ChangeUserPassword(Guid id, [FromBody] AdminChangePasswordCommand command)
        {
            if (id != command.UserId)
            {
                return BadRequest();
            }
            
            OperationResult result = await _adminUserService.ChangeUserPasswordAsAdminAsync(command, CancellationToken.None);
            
            if (result.IsFailure)
            {
                if (result.Errors.Any(e => e.Type == OperationErrorType.NotFoundError))
                {
                    return NotFound(result.Errors);
                }
                return BadRequest(result.Errors);
            }
            
            return NoContent();
        }
    }
}
