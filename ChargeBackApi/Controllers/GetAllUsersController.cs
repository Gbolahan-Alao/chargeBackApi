
using ChargeBackAuthApi.Data;
using ChargeBackAuthApi.Repositories;
using Microsoft.AspNetCore.Mvc;


namespace ChargeBackApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GetAllUsersController : ControllerBase
    {
        private readonly IUserAccount _userAccount;
        public GetAllUsersController(IUserAccount userAccount)
        {
            _userAccount = userAccount;
        }
        [HttpGet("GetUsers")]
        public ActionResult<IEnumerable<ApplicationUser>> GetRegisteredUsers()
        {
            var users = _userAccount.GetRegisteredUsers();
            return Ok(users);
        }
    }
}
