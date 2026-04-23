using ContactApi.Dto;
using ContactApi.Models;
using ContactApi.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;


namespace ContactApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly RoleManager<IdentityRole> _roleManager;
      

        public AuthController(UserManager<ApplicationUser> userManager, IJwtTokenService jwtTokenService, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _jwtTokenService = jwtTokenService;
            _roleManager = roleManager;
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
            var token = _jwtTokenService.GenerateTokenAsync(user); // Placeholder for actual token generation logic
            return Ok(new { Token = token });
        }
                                                            
}
}
