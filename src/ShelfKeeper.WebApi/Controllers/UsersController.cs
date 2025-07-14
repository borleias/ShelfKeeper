// <copyright file="UsersController.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using ShelfKeeper.Application.Services.Users;
using ShelfKeeper.Application.Services.Users.Models;
using Microsoft.AspNetCore.Authorization;
using ShelfKeeper.Shared.Common;

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
        /// <param name="command">The command containing user registration details.</param>
        /// <returns>An <see cref="IActionResult"/> representing the operationResult of the operation.</returns>
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
        /// <param name="query">The query containing user login credentials.</param>
        /// <returns>An <see cref="IActionResult"/> representing the operationResult of the operation.</returns>
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
        /// Resets a user's password.
        /// </summary>
        /// <param name="command">The command containing the user's email and new password.</param>
        /// <returns>An <see cref="IActionResult"/> representing the operationResult of the operation.</returns>
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command)
        {
            OperationResult operationResult = await _userService.ResetPasswordAsync(command, CancellationToken.None);
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
        /// Deletes a user account.
        /// </summary>
        /// <param name="id">The ID of the user to delete.</param>
        /// <returns>An <see cref="IActionResult"/> representing the operationResult of the operation.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            OperationResult operationResult = await _userService.DeleteUserAsync(new DeleteUserCommand(id), CancellationToken.None);
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
    }
}
