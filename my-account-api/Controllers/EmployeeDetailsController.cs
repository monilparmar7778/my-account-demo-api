using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using my_account_api.Interface;
using my_account_api.models;
using my_account_api.Services;

namespace my_account_api.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class EmployeeDetailsController : ControllerBase
	{
		private readonly IEmployeeDetailsService _employeeDetailsService;

		public EmployeeDetailsController(IEmployeeDetailsService employeeDetailsService)
		{
			_employeeDetailsService = employeeDetailsService;
		}

		[HttpPost]
		public async Task<ActionResult<EmployeeResponse>> CreateEmployee([FromBody] EmployeeDetails employee)
		{
			try
			{
				var result = await _employeeDetailsService.CreateEmployeeAsync(employee);
				return result.success ? Ok(result) : BadRequest(result);
			}
			catch (Exception ex)
			{
				return StatusCode(500, new EmployeeResponse
				{
					success = false,
					message = $"Internal server error: {ex.Message}"
				});
			}
		}

		//[HttpGet("basic")]
		//public async Task<ActionResult<EmployeesResponse>> GetEmployeesBasicInfo()
		//{
		//	try
		//	{
		//		var result = await _employeeDetailsService.GetEmployeesBasicInfoAsync();
		//		return Ok(result);
		//	}
		//	catch (Exception ex)
		//	{
		//		return StatusCode(500, new EmployeesResponse
		//		{
		//			success = false,
		//			message = $"Internal server error: {ex.Message}"
		//		});
		//	}
		//}

		

		[HttpGet("all-details")]
		public async Task<ActionResult<EmployeesResponse>> GetAllEmployeeDetails()
		{
			try
			{
				var employeeDetailsService = _employeeDetailsService as EmployeeDetailsService;
				var result = await employeeDetailsService.GetAllEmployeeDetailsAsync();
				return Ok(result);
			}
			catch (Exception ex)
			{
				return StatusCode(500, new EmployeesResponse
				{
					success = false,
					message = $"Internal server error: {ex.Message}"
				});
			}
		}
	}
}
