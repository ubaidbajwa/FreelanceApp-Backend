using FreelanceApp.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FreelanceApp.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]   // Default: all endpoints require auth
public class UserController(ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet("me")]
    public IActionResult GetMe()
    {
        return Ok(new
        {
            userId = currentUser.UserId,
            email = currentUser.Email,
            role = currentUser.Role,
            isCnicVerified = currentUser.IsCnicVerified,
            isAuthenticated = currentUser.IsAuthenticated
        });
    }

    [HttpGet("me/FreelancerApp-area")]
    [Authorize(Policy = "FreelancerAppOnly")]
    public IActionResult FreelancerAppArea()
    {
        return Ok(new
        {
            message = "Welcome FreelancerApp! Yeh area sirf FreelancerApps ke liye hai.",
            yourId = currentUser.UserId
        });
    }

    [HttpGet("me/withdraw-eligibility")]
    [Authorize(Policy = "CnicVerified")]
    public IActionResult WithdrawEligibility()
    {
        return Ok(new
        {
            eligible = true,
            message = "CNIC verified! Aap withdrawal kar sakte hain."
        });
    }
}