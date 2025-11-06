using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using my_account_api.models;
using my_account_api.Interface;

namespace my_account_api.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class UserController : ControllerBase
	{
		private readonly IUserService _userService;

		public UserController(IUserService userService)
		{
			_userService = userService;
		}

		[HttpPost]
		public async Task<ActionResult<UserResponse>> CreateUser([FromBody] User user)
		{
			try
			{
				var result = await _userService.CreateUserAsync(user);
				return result.success ? Ok(result) : BadRequest(result);
			}
			catch (Exception ex)
			{
				return StatusCode(500, new UserResponse
				{
					success = false,
					message = $"Internal server error: {ex.Message}"
				});
			}
		}

		[HttpGet("basic")]
		public async Task<ActionResult<UsersResponse>> GetUsersBasicInfo()
		{
			try
			{
				var result = await _userService.GetUsersBasicInfoAsync();
				return Ok(result);
			}
			catch (Exception ex)
			{
				return StatusCode(500, new UsersResponse
				{
					success = false,
					message = $"Internal server error: {ex.Message}"
				});
			}
		}
	}
}