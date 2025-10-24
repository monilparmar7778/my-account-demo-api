using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using my_account_api.Interface;
using my_account_api.models;

namespace my_account_api.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AuthController : ControllerBase
	{
		private readonly IAuthService _authService;

		public AuthController(IAuthService authService)
		{
			_authService = authService;
		}

		[HttpPost("login")]
		public async Task<ActionResult<AuthModel>> Login([FromBody] AuthModel request)
		{
			try
			{
				var result = await _authService.AuthenticateUserAsync(request);

				if (result.success)
				{
					return Ok(new AuthModel
					{
						success = true,
						message = result.message,
						token = result.token,
						user_id = result.user_id,
						username = result.username,
						expires_at = result.expires_at
					});
				}
				else
				{
					return Unauthorized(new AuthModel
					{
						success = false,
						message = result.message,
						token = null,
						user_id = -1,
						username = null,
						expires_at = null
					});
				}
			}
			catch (Exception ex)
			{
				return StatusCode(500, new AuthModel
				{
					success = false,
					message = $"Internal server error: {ex.Message}",
					token = null,
					user_id = -1,
					username = null,
					expires_at = null
				});
			}
		}

		[HttpPost("validate-token")]
		public IActionResult ValidateToken()
		{
			var userIdentity = User.Identity;
			if (userIdentity.IsAuthenticated)
			{
				var userId = User.FindFirst("user_id")?.Value;
				var username = User.FindFirst("username")?.Value;

				return Ok(new AuthModel
				{
					success = true,
					message = "Token is valid",
					user_id = long.Parse(userId),
					username = username
				});
			}

			return Unauthorized(new AuthModel
			{
				success = false,
				message = "Token is invalid or expired"
			});
		}
	}
}
