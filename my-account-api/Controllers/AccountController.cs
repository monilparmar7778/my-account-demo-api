using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using my_account_api.models;
using my_account_api.Interface;

namespace my_account_api.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AccountController : ControllerBase
	{
		private readonly IAccountService _accountService;

		public AccountController(IAccountService accountService)
		{
			_accountService = accountService;
		}

		// POST: api/Account/CreateGetMoney - Create only Get Money transaction
		[HttpPost("CreateGetMoney")]
		public async Task<ActionResult<ApiResponse<Account>>> CreateGetMoney([FromBody] Account account)
		{
			try
			{
				var result = await _accountService.CreateGetMoneyAsync(account);
				return result.success ? Ok(result) : BadRequest(result);
			}
			catch (Exception ex)
			{
				return StatusCode(500, new ApiResponse<Account>
				{
					success = false,
					message = $"Internal server error: {ex.Message}",
					data = null
				});
			}
		}

		// POST: api/Account/CreateGiveMoney - Create only Give Money transaction
		[HttpPost("CreateGiveMoney")]
		public async Task<ActionResult<ApiResponse<Account>>> CreateGiveMoney([FromBody] Account account)
		{
			try
			{
				var result = await _accountService.CreateGiveMoneyAsync(account);
				return result.success ? Ok(result) : BadRequest(result);
			}
			catch (Exception ex)
			{
				return StatusCode(500, new ApiResponse<Account>
				{
					success = false,
					message = $"Internal server error: {ex.Message}",
					data = null
				});
			}
		}

		// POST: api/Account/CompleteTransaction - Create complete transaction (both get and give)
		[HttpPost("CompleteTransaction")]
		public async Task<ActionResult<ApiResponse<Account>>> CompleteTransaction([FromBody] Account account)
		{
			try
			{
				var result = await _accountService.CreateCompleteTransactionAsync(account);
				return result.success ? Ok(result) : BadRequest(result);
			}
			catch (Exception ex)
			{
				return StatusCode(500, new ApiResponse<Account>
				{
					success = false,
					message = $"Internal server error: {ex.Message}",
					data = null
				});
			}
		}

		// Other existing methods (GET, PUT, DELETE) remain the same...
		[HttpGet]
		public async Task<ActionResult<ApiResponse<List<Account>>>> GetAccounts()
		{
			try
			{
				var result = await _accountService.GetAccountsAsync();
				return Ok(result);
			}
			catch (Exception ex)
			{
				return StatusCode(500, new ApiResponse<List<Account>>
				{
					success = false,
					message = $"Internal server error: {ex.Message}",
					data = new List<Account>()
				});
			}
		}

		[HttpGet("{acid}")]
		public async Task<ActionResult<ApiResponse<Account>>> GetAccount(long acid)
		{
			try
			{
				var result = await _accountService.GetAccountByIdAsync(acid);
				return result.success ? Ok(result) : NotFound(result);
			}
			catch (Exception ex)
			{
				return StatusCode(500, new ApiResponse<Account>
				{
					success = false,
					message = $"Internal server error: {ex.Message}",
					data = null
				});
			}
		}

		[HttpPut("{acid}")]
		public async Task<ActionResult<ApiResponse<Account>>> UpdateAccount(long acid, [FromBody] Account account)
		{
			try
			{
				account.acid = acid;
				var result = await _accountService.UpdateAccountAsync(account);
				return result.success ? Ok(result) : BadRequest(result);
			}
			catch (Exception ex)
			{
				return StatusCode(500, new ApiResponse<Account>
				{
					success = false,
					message = $"Internal server error: {ex.Message}",
					data = null
				});
			}
		}

		[HttpDelete("{acid}")]
		public async Task<ActionResult<ApiResponse<Account>>> DeleteAccount(long acid)
		{
			try
			{
				var result = await _accountService.DeleteAccountAsync(acid);
				return result.success ? Ok(result) : BadRequest(result);
			}
			catch (Exception ex)
			{
				return StatusCode(500, new ApiResponse<Account>
				{
					success = false,
					message = $"Internal server error: {ex.Message}",
					data = null
				});
			}
		}
	}
}