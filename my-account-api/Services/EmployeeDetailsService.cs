using NpgsqlTypes;
using my_account_api.models;
using my_account_api.Interface;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using Npgsql;
using NpgsqlTypes;

namespace my_account_api.Services
{
	public class EmployeeDetailsService : IEmployeeDetailsService
	{
		private readonly string _connectionString;

		public EmployeeDetailsService(IConfiguration configuration)
		{
			_connectionString = configuration.GetConnectionString("PostgreConnection")
				?? "Host=localhost;Port=5432;Database=my_account_demo;Username=postgres;Password=667866;";
		}

		public async Task<EmployeeResponse> CreateEmployeeAsync(EmployeeDetails employee)
		{
			try
			{
				// Validate required fields
				if (string.IsNullOrEmpty(employee.employee_name))
				{
					return new EmployeeResponse
					{
						success = false,
						message = "Employee name is required"
					};
				}

				using var connection = new NpgsqlConnection(_connectionString);
				await connection.OpenAsync();

				// Using PostgreSQL function to create employee
				using var command = new NpgsqlCommand(
					"SELECT create_employee(@EmployeeName, @Email, @PhoneNo)",
					connection
				);

				command.Parameters.AddWithValue("@EmployeeName", NpgsqlDbType.Varchar, employee.employee_name);
				command.Parameters.AddWithValue("@Email", NpgsqlDbType.Varchar, (object)employee.email ?? DBNull.Value);
				command.Parameters.AddWithValue("@PhoneNo", NpgsqlDbType.Varchar, (object)employee.phoneno ?? DBNull.Value);

				var result = await command.ExecuteScalarAsync();

				if (result == null)
				{
					return new EmployeeResponse
					{
						success = false,
						message = "Database returned no result"
					};
				}

				var jsonResponse = JsonSerializer.Deserialize<JsonElement>(result.ToString()!);

				if (jsonResponse.TryGetProperty("success", out var successElement) && successElement.GetBoolean())
				{
					var response = new EmployeeResponse
					{
						success = true,
						message = jsonResponse.TryGetProperty("message", out var msgElement) ? msgElement.GetString() : "Employee created successfully"
					};

					if (jsonResponse.TryGetProperty("employee_id", out var employeeIdElement))
					{
						response.employee_id = employeeIdElement.GetInt64();
					}

					return response;
				}
				else
				{
					var message = jsonResponse.TryGetProperty("message", out var msgElement) ? msgElement.GetString() : "Employee creation failed";
					return new EmployeeResponse
					{
						success = false,
						message = message
					};
				}
			}
			catch (Exception ex)
			{
				return new EmployeeResponse
				{
					success = false,
					message = $"Error creating employee: {ex.Message}"
				};
			}
		}

		public async Task<EmployeesResponse> GetEmployeesBasicInfoAsync()
		{
			try
			{
				using var connection = new NpgsqlConnection(_connectionString);
				await connection.OpenAsync();

				// Using PostgreSQL function to get employees
				using var command = new NpgsqlCommand(
					"SELECT * FROM get_employees_basic_info()",
					connection
				);

				using var reader = await command.ExecuteReaderAsync();

				var employees = new List<EmployeeDetails>();

				while (await reader.ReadAsync())
				{
					var employee = new EmployeeDetails
					{
						employee_id = reader.GetInt64(0),
						employee_name = reader.GetString(1)
						// Only getting employee_id and employee_name as requested
					};
					employees.Add(employee);
				}

				return new EmployeesResponse
				{
					success = true,
					message = employees.Count > 0 ? "Employees basic info retrieved successfully" : "No employees found",
					data = employees
				};
			}
			catch (Exception ex)
			{
				return new EmployeesResponse
				{
					success = false,
					message = $"Error retrieving employees basic info: {ex.Message}"
				};
			}
		}

		// Alternative method using direct table query
		public async Task<EmployeesResponse> GetEmployeesBasicInfoDirectAsync()
		{
			try
			{
				using var connection = new NpgsqlConnection(_connectionString);
				await connection.OpenAsync();

				// Direct table query to get only employee_id and employee_name
				using var command = new NpgsqlCommand(
					"SELECT employee_id, employee_name FROM public.tbl_employee_info ORDER BY employee_id",
					connection
				);

				using var reader = await command.ExecuteReaderAsync();

				var employees = new List<EmployeeDetails>();

				while (await reader.ReadAsync())
				{
					var employee = new EmployeeDetails
					{
						employee_id = reader.GetInt64(0),
						employee_name = reader.GetString(1)
					};
					employees.Add(employee);
				}

				return new EmployeesResponse
				{
					success = true,
					message = employees.Count > 0 ? "Employees basic info retrieved successfully" : "No employees found",
					data = employees
				};
			}
			catch (Exception ex)
			{
				return new EmployeesResponse
				{
					success = false,
					message = $"Error retrieving employees basic info: {ex.Message}"
				};
			}
		}

		// Additional method to get all employee details
		public async Task<EmployeesResponse> GetAllEmployeeDetailsAsync()
		{
			try
			{
				using var connection = new NpgsqlConnection(_connectionString);
				await connection.OpenAsync();

				using var command = new NpgsqlCommand(
					"SELECT employee_id, employee_name, email, phoneno FROM public.tbl_employee_info ORDER BY employee_id",
					connection
				);

				using var reader = await command.ExecuteReaderAsync();

				var employees = new List<EmployeeDetails>();

				while (await reader.ReadAsync())
				{
					var employee = new EmployeeDetails
					{
						employee_id = reader.GetInt64(0),
						employee_name = reader.GetString(1),
						email = reader.IsDBNull(2) ? null : reader.GetString(2),
						phoneno = reader.IsDBNull(3) ? null : reader.GetString(3)
					};
					employees.Add(employee);
				}

				return new EmployeesResponse
				{
					success = true,
					message = employees.Count > 0 ? "All employee details retrieved successfully" : "No employees found",
					data = employees
				};
			}
			catch (Exception ex)
			{
				return new EmployeesResponse
				{
					success = false,
					message = $"Error retrieving employee details: {ex.Message}"
				};
			}
		}
	}
}
