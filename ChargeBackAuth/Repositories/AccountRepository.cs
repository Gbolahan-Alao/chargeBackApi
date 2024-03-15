using ChargeBackAuthApi.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SharedClassLibrary.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static SharedClassLibrary.DTOs.ServiceResponses;

namespace ChargeBackAuthApi.Repositories
{
    public interface IUserAccount
    {
        Task<GeneralResponse> CreateAccount(UserDTO UserDTO, string merchant);
        Task<LoginResponse> LoginAccount(LoginDTO LoginDTO);
        IEnumerable<ApplicationUser> GetRegisteredUsers();
        Task<IEnumerable<string>> GetUserRolesAsync(string email);
    }

    public class AccountRepository : IUserAccount
    {
        private readonly IConfiguration _config;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountRepository(IConfiguration config, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _config = config;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<GeneralResponse> CreateAccount(UserDTO UserDTO, string merchant)
        {
            if (UserDTO is null)
                return new GeneralResponse(false, "Modal is empty", "");

            if (string.IsNullOrWhiteSpace(merchant))
                return new GeneralResponse(false, "Merchant is required", "");

            string merchantId;
            switch (merchant)
            {
                case "polFairmoney":
                    merchantId = "1234";
                    break;
                case "polTeamapt":
                    merchantId = "5678";
                    break;
                case "polPalmpay":
                    merchantId = "91011";
                    break;
                case "polaris": 
                    merchantId = "0000";
                    break;
                default:
                    throw new Exception("Invalid merchant name. Please enter a valid merchant name.");
            }

            var newUser = new ApplicationUser()
            {
                Name = UserDTO.Name,
                Email = UserDTO.Email,
                UserName = UserDTO.Email,
                MerchantId = merchantId 
            };

            var user = await _userManager.FindByEmailAsync(newUser.Email);
            if (user is not null)
                return new GeneralResponse(false, "User Registered Already", "");

            var createUser = await _userManager.CreateAsync(newUser, UserDTO.Password);
            if (!createUser.Succeeded)
            {
                var errorMessage = string.Join(", ", createUser.Errors.Select(error => $"{error.Code}: {error.Description}"));
                return new GeneralResponse(false, errorMessage, "Error Occurred, Please try again");
            }

            var role = await _roleManager.FindByNameAsync(merchant);
            if (role is null)
            {
                await _roleManager.CreateAsync(new IdentityRole() { Name = merchant });
            }

            await _userManager.AddToRoleAsync(newUser, merchant);

            if (merchant.ToLower() == "polaris")
            {
                var adminRoleExists = await _roleManager.RoleExistsAsync("Admin");
                if (!adminRoleExists)
                {
                    await _roleManager.CreateAsync(new IdentityRole("Admin"));
                }

                await _userManager.AddToRoleAsync(newUser, "Admin");
            }
            else
            {
                var userRoleExists = await _roleManager.RoleExistsAsync("User");
                if (!userRoleExists)
                {
                    await _roleManager.CreateAsync(new IdentityRole("User"));
                }

                await _userManager.AddToRoleAsync(newUser, "User");
            }

            return new GeneralResponse(true, "Account Created", "");
        }

        public async Task<LoginResponse> LoginAccount(LoginDTO LoginDTO)
        {
            if (LoginDTO == null)
                return new LoginResponse(false, null!, "Login container is empty");

            var getUser = await _userManager.FindByEmailAsync(LoginDTO.Email);
            if (getUser is null)
                return new LoginResponse(false, null!, "User not found");

            bool checkUserPasswords = await _userManager.CheckPasswordAsync(getUser, LoginDTO.Password);
            if (!checkUserPasswords)
                return new LoginResponse(false, null!, "Invalid Email/Password");

            var getUserRole = await _userManager.GetRolesAsync(getUser);

            string merchant = getUserRole.FirstOrDefault();
            if (string.IsNullOrEmpty(merchant))
                return new LoginResponse(false, null!, "User has no assigned merchant role");
            var userRoles = await _userManager.GetRolesAsync(getUser);

            var userSession = new UserSession(getUser.Id, getUser.Name, getUser.Email,userRoles);

            string token = GenerateToken(userSession);
            return new LoginResponse(true, token!, "Login completed");
        }

        private string GenerateToken(UserSession user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var userClaims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id),
        new Claim(ClaimTypes.Name, user.Name),
        new Claim(ClaimTypes.Email, user.Email)
    };

            foreach (var role in user.Roles)
            {
                userClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: userClaims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: credentials
                );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public IEnumerable<ApplicationUser> GetRegisteredUsers()
        {
            try
            {
                var users = _userManager.Users.ToList();
                return users;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting registered users: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<string>> GetUserRolesAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                throw new Exception("User not found");
            }

            var roles = await _userManager.GetRolesAsync(user);

            return roles;
        }

        public async Task<GeneralResponse> DeleteUserByEmail(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return new GeneralResponse(false, "User not found", "");

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                var errorMessage = string.Join(", ", result.Errors.Select(error => $"{error.Code}: {error.Description}"));
                return new GeneralResponse(false, errorMessage, "Error occurred while deleting user");
            }

            return new GeneralResponse(true, "User deleted successfully", "");
        }

        public async Task<GeneralResponse> ClearAllUsers()
        {
            var allUsers = await _userManager.Users.ToListAsync();
            foreach (var user in allUsers)
            {
                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    var errorMessage = string.Join(", ", result.Errors.Select(error => $"{error.Code}: {error.Description}"));
                    return new GeneralResponse(false, errorMessage, "Error occurred while clearing users");
                }
            }

            return new GeneralResponse(true, "All users cleared successfully", "");
        }
    }
}
