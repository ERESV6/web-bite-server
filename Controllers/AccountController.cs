using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using web_bite_server.Dtos.Account;
using web_bite_server.Models;

namespace web_bite_server.Controllers
{
    [Route("api/account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpPost("register")]
        [Produces("text/plain")]
        public async Task<ActionResult<string>> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var appUser = new AppUser
                {
                    UserName = registerDto.Username,
                    Email = registerDto.Email
                };

                var createdUser = await _userManager.CreateAsync(appUser, registerDto.Password);

                if (createdUser.Succeeded)
                {
                    var roleResult = await _userManager.AddToRoleAsync(appUser, "User");
                    if (roleResult.Succeeded)
                    {
                        return Ok("User created");
                    }
                    else
                    {
                        return StatusCode(500, roleResult.Errors);
                    }
                }
                else
                {
                    return StatusCode(500, createdUser.Errors);
                }
            }
            catch (Exception e)
            {
                return StatusCode(500, e);
            }
        }

        [HttpPost("login")]
        [Produces("application/json")]
        public async Task<ActionResult<UserDto>> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (_signInManager.IsSignedIn(HttpContext.User))
            {
                return Ok("Already logged in");
            }

            var user = await _userManager.FindByEmailAsync(loginDto.Email);

            if (user == null)
            {
                return Unauthorized("Invalid email");
            }

            var result = await _signInManager.PasswordSignInAsync(user.UserName ?? "", loginDto.Password, false, false);
            if (!result.Succeeded)
            {
                return Unauthorized("Email or password incorrect");
            }

            return Ok(new UserDto
            {
                Username = user.UserName ?? "",
                Email = user.Email ?? ""
            });
        }

        [HttpPost("logout")]
        [Produces("text/plain")]
        public async Task<ActionResult<string>> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok("User logged out");
        }

        [HttpGet("user-info")]
        [Authorize]
        [Produces("application/json")]
        public async Task<ActionResult<UserDto>> UserInfo()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(new UserDto
            {
                Username = user.UserName ?? "",
                Email = user.Email ?? ""
            });
        }
    }
}