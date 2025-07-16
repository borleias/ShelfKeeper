// <copyright file="UsersController.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using ShelfKeeper.Application.Services.Users;
using ShelfKeeper.Application.Services.Users.Models;
using Microsoft.AspNetCore.Authorization;
using ShelfKeeper.Shared.Common;
using System.Security.Claims;

namespace ShelfKeeper.WebApi.Controllers
{
    /// <summary>
    /// API controller for user management.
    /// </summary>
    [ApiController]
    [Authorize]
    [Asp.Versioning.ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        /// <summary>
        /// Initializes a new instance of the <see cref="UsersController"/> class.
        /// </summary>
        /// <param name="userService">The user service.</param>
        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Registers a new user.
        /// </summary>
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] CreateUserCommand command)
        {
            OperationResult<CreateUserResponse> operationResult = await _userService.CreateUserAsync(command, CancellationToken.None);
            if (operationResult.IsFailure)
            {
                return BadRequest(operationResult.Errors);
            }
            return CreatedAtAction(nameof(Register), new { id = operationResult.Value.UserId }, operationResult.Value);
        }

        /// <summary>
        /// Logs in a user and returns a JWT token.
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginUserQuery query)
        {
            OperationResult<LoginUserResponse> operationResult = await _userService.LoginUserAsync(query, CancellationToken.None);
            if (operationResult.IsFailure)
            {
                if (operationResult.Errors.Any(e => e.Type == OperationErrorType.UnauthorizedError))
                {
                    return Unauthorized(operationResult.Errors);
                }
                return BadRequest(operationResult.Errors);
            }
            return Ok(operationResult.Value);
        }

        /// <summary>
        /// Changes the current user's password.
        /// </summary>
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordCommand command)
        {
            var userId = GetUserId();
            if (userId == Guid.Empty || userId != command.UserId)
            {
                return Unauthorized();
            }

            OperationResult operationResult = await _userService.ChangePasswordAsync(command, CancellationToken.None);
            if (operationResult.IsFailure)
            {
                return BadRequest(operationResult.Errors);
            }
            return NoContent();
        }

        /// <summary>
        /// Initiates the password reset process for a user.
        /// </summary>
        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand command)
        {
            await _userService.ForgotPasswordAsync(command, CancellationToken.None);
            return NoContent(); // Always return success
        }

        /// <summary>
        /// Resets a user's password using a reset token.
        /// </summary>
        [HttpPost("reset-password-with-token")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPasswordWithToken([FromBody] ResetPasswordWithTokenCommand command)
        {
            OperationResult operationResult = await _userService.ResetPasswordWithTokenAsync(command, CancellationToken.None);
            if (operationResult.IsFailure)
            {
                return BadRequest(operationResult.Errors);
            }
            return NoContent();
        }

        /// <summary>
        /// Deletes the currently authenticated user's account.
        /// </summary>
        [HttpDelete("me")]
        public async Task<IActionResult> Delete()
        {
            var userId = GetUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized();
            }

            OperationResult operationResult = await _userService.DeleteUserAsync(new DeleteUserCommand(userId), CancellationToken.None);
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

        private Guid GetUserId()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == "sub");
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out Guid userId))
            {
                return userId;
            }
            return Guid.Empty;
        }
    }
}
