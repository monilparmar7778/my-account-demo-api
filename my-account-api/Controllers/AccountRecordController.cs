using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using my_account_api.Interface;
using my_account_api.models;

namespace my_account_api.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AccountRecordController : ControllerBase
	{
		private readonly IAccountRecordService _accountRecordService;

		public AccountRecordController(IAccountRecordService accountRecordService)
		{
			_accountRecordService = accountRecordService;
		}

		[HttpPost("records")]
		public async Task<ActionResult<AccountRecordsResponse>> GetAccountRecords([FromBody] AccountRecordsRequest request)
		{
			try
			{
				var result = await _accountRecordService.GetAccountRecordsAsync(request);
				return result.success ? Ok(result) : BadRequest(result);
			}
			catch (Exception ex)
			{
				return StatusCode(500, new AccountRecordsResponse
				{
					success = false,
					message = $"Internal server error: {ex.Message}",
					data = new List<AccountRecord>(),
					total = 0
				});
			}
		}

	
	}
}