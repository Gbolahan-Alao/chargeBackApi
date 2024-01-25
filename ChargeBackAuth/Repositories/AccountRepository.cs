using ChargeBackAuthApi.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SharedClassLibrary.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static SharedClassLibrary.DTOs.ServiceResponses;

namespace ChargeBackAuthApi.Repositories {

    public interface IUserAccount
    {
        Task<GeneralResponse> CreateAccount(UserDTO UserDTO);
        Task<LoginResponse> LoginAccount(LoginDTO LoginDTO);
        IEnumerable<ApplicationUser> GetRegisteredUsers();
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
        
        
        public async Task<GeneralResponse> CreateAccount(UserDTO UserDTO)
        {
            if (UserDTO is null) return new GeneralResponse(false, "Modal is empty");
            var newUser = new ApplicationUser()
            {
                Name = UserDTO.Name,
                Email = UserDTO.Email,
                PasswordHash = UserDTO.Password,
                UserName = UserDTO.Email

            };
            var user = await _userManager.FindByEmailAsync(newUser.Email);
            if (user is not null) return new GeneralResponse(false, "User Registered Already");

            var createUser = await _userManager.CreateAsync(newUser!, UserDTO.Password);

            if (!createUser.Succeeded)
            {
                foreach (var error in createUser.Errors)
                {
                    Console.WriteLine($"Error: {error.Code}, Description: {error.Description}");
                }

                return new GeneralResponse(false, "Error Occurred, Please try again");
            }
            //Assign Default Role: Admin to first registrar; rest is user
            var checkAdmin = await _roleManager.FindByNameAsync("Admin");
            if (checkAdmin is null)
            {
                await _roleManager.CreateAsync(new IdentityRole() { Name = "Admin" });
                await _userManager.AddToRoleAsync(newUser, "Admin");
                return new GeneralResponse(true, "Account Created");
            } else
            {
                var checkUser = await _roleManager.FindByNameAsync("User");
                if (checkUser is null) await _roleManager.CreateAsync(new IdentityRole() { Name = "User" });

                await _userManager.AddToRoleAsync(newUser, "User");
                return new GeneralResponse(true, "Account Created");
            }
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
            var userSession = new UserSession(getUser.Id, getUser.Name, getUser.Email, getUserRole.First());

            string token = GenerateToken(userSession);
            return new LoginResponse(true, token!, "Login completed");
        }

        private string GenerateToken(UserSession user) {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var userClaims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

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
            var users = _userManager.Users.ToList();
            return users;
        }

    } }

