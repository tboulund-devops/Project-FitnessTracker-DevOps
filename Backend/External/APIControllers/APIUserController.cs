using Backend.Application.Service.Interfaces;
using Backend.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Backend.External.APIControllers;

[ApiController]
[Route("api/user/[controller]")]
public class APIUserController : ControllerBase
{
    private readonly IUserService _userService;

    public APIUserController(IUserService userService)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
    }

    [HttpGet("GetUserInformation/{UserID}")]
    public async Task<ActionResult<UserInformationDTO>> GetWorkoutsForUser(int userID)
    {
        if (userID <= 0)
            return BadRequest("User id must be a positive number");

        var workout = await _userService.GetUserInformation(userID);

        if (workout == null)
            return NotFound($"User with ID {userID} not found");

        return Ok(workout);
    }

    [HttpGet("GetProfileInfo/{UserID}")]
    public async Task<ActionResult<User>> GetProfileInfo(int userID)
    {
        if (userID <= 0)
        {
            return BadRequest("User id must be a positive number");
        }

        var user = await _userService.GetProfileInfo(userID);

        if (user == null)
        {
            return NotFound($"User with ID {userID} not found");
        }

        return Ok(user);

    }

    [HttpPut("UpdateEmail")]
    public async Task<ActionResult> UpdateEmail(int userID, string newEmail)
    {
        if (userID <= 0)
        {
            return BadRequest("User id must be a positive number");
        }

        var updateResult = await _userService.UpdateUserEmail(userID, newEmail);
        if (!updateResult)
        {
            return NotFound($"User with ID {userID} not found");
        }

        return Ok("Email updated successfully");
    }
}