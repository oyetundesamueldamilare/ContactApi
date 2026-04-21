using ContactApi.Dto;
using ContactApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ContactApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly RoleManager<IdentityRole> _roleManager;
      

        public AuthController(UserManager<ApplicationUser> userManager, IConfiguration configuration, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _configuration = configuration;
            _roleManager = roleManager;
        }
        [HttpPost ("Craete-Role")]

        public async Task <IActionResult> CreateRole([FromBody] string roleName)
        {  
            if (string.IsNullOrWhiteSpace(roleName))
                return BadRequest("Role name cannot be empty.");
            if (await _roleManager.RoleExistsAsync(roleName))
                return BadRequest($"Role '{roleName}' already exists.");
            var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return BadRequest($"Failed to create role: {errors}");
            }
            return Ok($"Role '{roleName}' created successfully.");
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            { if (!await _roleManager.RoleExistsAsync(model.Role))
                return BadRequest($"Role {model.Role} does not exist"); }
            if (model == null) return BadRequest("Invalid user data.");
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                //var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return BadRequest($"User registration failed");
            }
            var roleResult = await _userManager.AddToRoleAsync(user, model.Role);
            return Ok("User registered successfully.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            if (model == null) return BadRequest("Invalid login data.");
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
            {
                return Unauthorized("Invalid email or password.");
            }
            // Generate JWT token here and return it
            var token = GeneratedJWTToken(user); // Placeholder for actual token generation logic
            return Ok(new { Token = token });
        }

        private async Task<string> GeneratedJWTToken(ApplicationUser user)
        {
            var jwtKey = _configuration["Jwt:Key"]!;
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id ?? throw new InvalidOperationException("User Id is null")),
        new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty)
    };

            if (!string.IsNullOrWhiteSpace(user.FullName))
            {
                claims.Add(new Claim(JwtRegisteredClaimNames.Name, user.FullName));
            }

            // 🔑 Add role claims
            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
       



    }
}
}
