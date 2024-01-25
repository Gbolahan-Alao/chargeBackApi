using ChargeBackAuthApi.Data;
using ChargeBackAuthApi.Repositories;
using Microsoft.AspNetCore.Mvc;
using SharedClassLibrary.DTOs;

namespace ChargeBackAuthApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController(IUserAccount userAccount) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<IActionResult> Register(UserDTO userDTO)
        {
            var response = await userAccount.CreateAccount(userDTO);
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
    }
}

