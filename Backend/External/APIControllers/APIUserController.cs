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
    
    
    
}