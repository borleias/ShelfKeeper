// <copyright file="UsersController.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using ShelfKeeper.Application.Services.Users;
using ShelfKeeper.Application.Services.Users.Models;
using Microsoft.AspNetCore.Authorization;

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
        /// <returns>An <see cref="IActionResult"/> representing the result of the operation.</returns>
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] CreateUserCommand command)
        {
            CreateUserResponse response = await _userService.CreateUserAsync(command, CancellationToken.None);
            return CreatedAtAction(nameof(Register), new { id = response.UserId }, response);
        }

        /// <summary>
        /// Logs in a user and returns a JWT token.
        /// </summary>
        /// <param name="query">The query containing user login credentials.</param>
        /// <returns>An <see cref="IActionResult"/> representing the result of the operation.</returns>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginUserQuery query)
        {
            LoginUserResponse response = await _userService.LoginUserAsync(query, CancellationToken.None);
            return Ok(response);
        }

        /// <summary>
        /// Resets a user's password.
        /// </summary>
        /// <param name="command">The command containing the user's email and new password.</param>
        /// <returns>An <see cref="IActionResult"/> representing the result of the operation.</returns>
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command)
        {
            await _userService.ResetPasswordAsync(command, CancellationToken.None);
            return NoContent();
        }

        /// <summary>
        /// Deletes a user account.
        /// </summary>
        /// <param name="id">The ID of the user to delete.</param>
        /// <returns>An <see cref="IActionResult"/> representing the result of the operation.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _userService.DeleteUserAsync(new DeleteUserCommand(id), CancellationToken.None);
            return NoContent();
        }
    }
}