using ChargeBackAuthApi.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SharedClassLibrary.DTOs;
using System.Data;
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
        private readonly FileUploadDbContext _dbContext;

        public AccountRepository(IConfiguration config, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, FileUploadDbContext dbContext)
        {
            _config = config;
            _userManager = userManager;
            _roleManager = roleManager;
            _dbContext = dbContext;
        }
        public async Task<GeneralResponse> CreateAccount(UserDTO UserDTO, string merchantId)
        {
            if (UserDTO is null)
                return new GeneralResponse(false, "Modal is empty", "");

            if (string.IsNullOrWhiteSpace(merchantId))
                return new GeneralResponse(false, "Merchant ID is required", "");

            // Fetch the merchant entity using the provided merchantId
            var merchant = await _dbContext.Merchants.FirstOrDefaultAsync(m => m.Id.ToString() == merchantId);

            if (merchant == null)
                return new GeneralResponse(false, "Merchant not found", "");

            var newUser = new ApplicationUser()
            {
                Name = UserDTO.Name,
                Email = UserDTO.Email,
                UserName = UserDTO.Email,
                MerchantId = merchantId // Associate the user with the merchant using the provided merchantId
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

            // Assign the merchant role based on the fetched merchant name
            var roleName = merchant.Name;
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role is null)
            {
                await _roleManager.CreateAsync(new IdentityRole() { Name = roleName });
            }

            await _userManager.AddToRoleAsync(newUser, roleName);

            // Check if the user's merchant is "Polaris" and assign the "Admin" role
            if (roleName.ToLower() == "polaris bank")
            {
                var adminRoleExists = await _roleManager.RoleExistsAsync("Admin");
                if (!adminRoleExists)
                {
                    await _roleManager.CreateAsync(new IdentityRole("Admin"));
                }

                await _userManager.AddToRoleAsync(newUser, "Admin");
            }

            // Return success message with the merchant name
            return new GeneralResponse(true, $"User for {merchant.Name} successfully created", "");
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

            string merchantId = getUser.MerchantId; // Retrieve the merchant ID from the user
            IEnumerable<string> roles = getUserRole; // Retrieve the roles of the user

            var userSession = new UserSession(getUser.Id, getUser.Name, getUser.Email, roles);

            string token = GenerateToken(userSession, merchantId, roles);
            return new LoginResponse(true, token!, "Login completed");
        }

        private string GenerateToken(UserSession user, string merchantId, IEnumerable<string> roles)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var userClaims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id),
        new Claim(ClaimTypes.Name, user.Name),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim("MerchantId", merchantId) // Add the merchant ID as a custom claim
    };

            foreach (var role in roles)
            {
                userClaims.Add(new Claim(ClaimTypes.Role, role)); // Add each role as a claim
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
