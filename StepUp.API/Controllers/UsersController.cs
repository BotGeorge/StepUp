using Microsoft.AspNetCore.Mvc;
using StepUp.Application.DTOs.User;
using StepUp.Application.Interfaces;
using StepUp.Domain.Enums;

namespace StepUp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : BaseController
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    private async Task<UserDto> RequireAdminAsync(Guid userId, CancellationToken cancellationToken)
    {
        if (userId == Guid.Empty)
        {
            throw new InvalidOperationException("ID utilizator invalid.");
        }

        var user = await _userService.GetUserByIdAsync(userId, cancellationToken);
        if (user == null || user.Role != Role.Admin)
        {
            throw new UnauthorizedAccessException("Nu ai dreptul să accesezi această resursă.");
        }

        return user;
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserDto createUserDto, CancellationToken cancellationToken)
    {
        var userDto = await _userService.CreateUserAsync(createUserDto, cancellationToken);
        return CreatedAtAction(nameof(GetUserById), new { id = userDto.Id }, userDto);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers(CancellationToken cancellationToken)
    {
        var users = await _userService.GetAllUsersAsync(cancellationToken);
        return Ok(users);
    }

    [HttpGet("admin")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsersForAdmin([FromQuery] Guid userId, CancellationToken cancellationToken)
    {
        try
        {
            await RequireAdminAsync(userId, cancellationToken);
            var users = await _userService.GetAllUsersAsync(cancellationToken);
            return Ok(users);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUserById(Guid id, CancellationToken cancellationToken)
    {
        var user = await _userService.GetUserByIdAsync(id, cancellationToken);
        
        if (user == null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<UserDto>> UpdateUser(Guid id, [FromBody] UpdateUserDto updateUserDto, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userService.UpdateUserAsync(id, updateUserDto, cancellationToken);
            return Ok(new
            {
                success = true,
                message = "User updated successfully.",
                user = user
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while updating the user.",
                error = ex.Message
            });
        }
    }

    [HttpPut("admin/{id}/role")]
    public async Task<ActionResult> UpdateUserRole(Guid id, [FromQuery] Guid userId, [FromBody] AdminUpdateUserRoleDto updateDto, CancellationToken cancellationToken)
    {
        try
        {
            if (id == userId)
            {
                return BadRequest(new { success = false, message = "Nu îți poți schimba propriul rol." });
            }

            await RequireAdminAsync(userId, cancellationToken);
            var user = await _userService.UpdateUserRoleAsync(id, updateDto.Role, cancellationToken);
            return Ok(new { success = true, user });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpPut("admin/{id}/suspend")]
    public async Task<ActionResult> UpdateUserSuspension(Guid id, [FromQuery] Guid userId, [FromBody] AdminUpdateUserSuspensionDto updateDto, CancellationToken cancellationToken)
    {
        try
        {
            if (id == userId)
            {
                return BadRequest(new { success = false, message = "Nu îți poți suspenda propriul cont." });
            }

            await RequireAdminAsync(userId, cancellationToken);
            var user = await _userService.UpdateUserSuspensionAsync(id, updateDto.IsSuspended, cancellationToken);
            return Ok(new { success = true, user });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("{id}/change-password")]
    public async Task<ActionResult> ChangePassword(Guid id, [FromBody] ChangePasswordDto changePasswordDto, CancellationToken cancellationToken)
    {
        try
        {
            await _userService.ChangePasswordAsync(id, changePasswordDto, cancellationToken);
            return Ok(new
            {
                success = true,
                message = "Password changed successfully."
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while changing the password.",
                error = ex.Message
            });
        }
    }
}

