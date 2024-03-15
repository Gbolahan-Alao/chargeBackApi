using ChargeBackAuthApi.Data;
using ChargeBackAuthApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedClassLibrary.DTOs;

namespace ChargeBackAuthApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController(IUserAccount userAccount) : ControllerBase
    {
        [HttpPost("register")]
      //  [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Register(UserDTO userDTO, string merchant)
        {
            var response = await userAccount.CreateAccount(userDTO,merchant );
            return Ok(response);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO LoginDTO)
        {
            var response = await userAccount.LoginAccount(LoginDTO);
            return Ok(response);
        }

        [HttpGet("GetRegisteredUsers")]
        
        public ActionResult<IEnumerable<ApplicationUser>> GetRegisteredUsers()
        {
            var users = userAccount.GetRegisteredUsers();
            return Ok(users);
        }

        [HttpGet("{email}")]
        
        public async Task<IActionResult> GetUserRoles(string email)
        {
            var roles = await userAccount.GetUserRolesAsync(email);
            if (!roles.Any())
            {
                return NotFound(new { message = "User not found or no roles assigned." });
            }

            return Ok(roles);
        }


    }
}

