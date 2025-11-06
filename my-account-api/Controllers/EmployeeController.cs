using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using my_account_api.models;
using my_account_api.Interface;

namespace my_account_api.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class EmployeeController : ControllerBase
	{
		private readonly IEmployeeService _employeeService;

		public EmployeeController(IEmployeeService employeeService)
		{
			_employeeService = employeeService;
		}

		// POST: api/Employee
		[HttpPost]
		public async Task<ActionResult<EmployeeApiResponse<Employee>>> CreateEmployee([FromBody] Employee employee)
		{
			try
			{
				var result = await _employeeService.CreateEmployeeAsync(employee);
				return result.success ? Ok(result) : BadRequest(result);
			}
			catch (Exception ex)
			{
				return StatusCode(500, new EmployeeApiResponse<Employee>
				{
					success = false,
					message = $"Internal server error: {ex.Message}",
					data = null
				});
			}
		}

		// GET: api/Employee
		[HttpGet]
		public async Task<ActionResult<EmployeeApiResponse<List<Employee>>>> GetEmployees()
		{
			try
			{
				var result = await _employeeService.GetEmployeesAsync();
				return Ok(result);
			}
			catch (Exception ex)
			{
				return StatusCode(500, new EmployeeApiResponse<List<Employee>>
				{
					success = false,
					message = $"Internal server error: {ex.Message}",
					data = new List<Employee>()
				});
			}
		}

		// GET: api/Employee/{id}
		[HttpGet("{emp_details_id}")]
		public async Task<ActionResult<EmployeeApiResponse<Employee>>> GetEmployee(long emp_details_id)
		{
			try
			{
				var result = await _employeeService.GetEmployeeByIdAsync(emp_details_id);
				return result.success ? Ok(result) : NotFound(result);
			}
			catch (Exception ex)
			{
				return StatusCode(500, new EmployeeApiResponse<Employee>
				{
					success = false,
					message = $"Internal server error: {ex.Message}",
					data = null
				});
			}
		}

		// PUT: api/Employee/{id}
		[HttpPut("{emp_details_id}")]
		public async Task<ActionResult<EmployeeApiResponse<Employee>>> UpdateEmployee(long emp_details_id, [FromBody] Employee employee)
		{
			try
			{
				employee.emp_details_id = emp_details_id;
				var result = await _employeeService.UpdateEmployeeAsync(employee);
				return result.success ? Ok(result) : BadRequest(result);
			}
			catch (Exception ex)
			{
				return StatusCode(500, new EmployeeApiResponse<Employee>
				{
					success = false,
					message = $"Internal server error: {ex.Message}",
					data = null
				});
			}
		}

		// DELETE: api/Employee/{id}
		[HttpDelete("{emp_details_id}")]
		public async Task<ActionResult<EmployeeApiResponse<Employee>>> DeleteEmployee(long emp_details_id)
		{
			try
			{
				var result = await _employeeService.DeleteEmployeeAsync(emp_details_id);
				return result.success ? Ok(result) : BadRequest(result);
			}
			catch (Exception ex)
			{
				return StatusCode(500, new EmployeeApiResponse<Employee>
				{
					success = false,
					message = $"Internal server error: {ex.Message}",
					data = null
				});
			}
		}
	}
}