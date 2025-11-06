using System.Text.Json;
using Npgsql;
using NpgsqlTypes;
using my_account_api.models;
using my_account_api.Interface;
using Microsoft.Extensions.Configuration;

namespace my_account_api.Services
{
	public class EmployeeService : IEmployeeService
	{
		private readonly string _connectionString;

		public EmployeeService(IConfiguration configuration)
		{
			_connectionString = configuration.GetConnectionString("PostgreConnection");

			if (string.IsNullOrEmpty(_connectionString))
			{
				_connectionString = "Host=localhost;Port=5432;Database=my_account_demo;Username=postgres;Password=667866;";
				Console.WriteLine("Using fallback connection string for EmployeeService");
			}

			Console.WriteLine($"EmployeeService initialized: {MaskPassword(_connectionString)}");
		}

		// ============ CRUD OPERATIONS ============

		public async Task<EmployeeApiResponse<Employee>> CreateEmployeeAsync(Employee employee)
		{
			return await ExecuteEmployeeOperation("INSERT", employee);
		}

		public async Task<EmployeeApiResponse<Employee>> GetEmployeeByIdAsync(long emp_details_id)
		{
			var tempEmployee = new Employee { emp_details_id = emp_details_id };
			return await ExecuteEmployeeOperation("SELECT", tempEmployee);
		}

		public async Task<EmployeeApiResponse<List<Employee>>> GetEmployeesAsync()
		{
			try
			{
				Console.WriteLine("Getting all employees...");

				using var connection = new NpgsqlConnection(_connectionString);
				await connection.OpenAsync();
				Console.WriteLine("Database connection opened successfully");

				using var command = new NpgsqlCommand(
					"SELECT manage_employee('SELECT', NULL, NULL, NULL, NULL, NULL)",
					connection
				);

				Console.WriteLine("Executing database command for SELECT operation...");
				var result = await command.ExecuteScalarAsync();

				if (result == null)
				{
					Console.WriteLine("Database returned NULL");
					return new EmployeeApiResponse<List<Employee>>
					{
						success = false,
						message = "Database returned no result",
						data = new List<Employee>(),
						total = 0
					};
				}

				Console.WriteLine($"Raw database response: {result}");
				var jsonResponse = JsonSerializer.Deserialize<JsonElement>(result.ToString()!);

				if (jsonResponse.TryGetProperty("success", out var successElement) && successElement.GetBoolean())
				{
					if (jsonResponse.TryGetProperty("data", out var dataElement) && dataElement.ValueKind == JsonValueKind.Array)
					{
						var employees = JsonSerializer.Deserialize<List<Employee>>(dataElement.GetRawText());
						var total = jsonResponse.TryGetProperty("total", out var totalElement) ? totalElement.GetInt32() : 0;

						Console.WriteLine($"Successfully retrieved {employees?.Count ?? 0} employees");

						return new EmployeeApiResponse<List<Employee>>
						{
							success = true,
							message = "Employees retrieved successfully",
							data = employees ?? new List<Employee>(),
							total = total
						};
					}
					else
					{
						Console.WriteLine("No data array found in response");
						return new EmployeeApiResponse<List<Employee>>
						{
							success = true,
							message = "No employees found",
							data = new List<Employee>(),
							total = 0
						};
					}
				}
				else
				{
					var message = jsonResponse.TryGetProperty("message", out var messageElement)
						? messageElement.GetString()
						: "Operation failed";

					Console.WriteLine($"Database operation failed: {message}");
					return new EmployeeApiResponse<List<Employee>>
					{
						success = false,
						message = message,
						data = new List<Employee>(),
						total = 0
					};
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error in GetEmployeesAsync: {ex.Message}");
				return new EmployeeApiResponse<List<Employee>>
				{
					success = false,
					message = $"Error: {ex.Message}",
					data = new List<Employee>(),
					total = 0
				};
			}
		}

		public async Task<EmployeeApiResponse<Employee>> UpdateEmployeeAsync(Employee employee)
		{
			return await ExecuteEmployeeOperation("UPDATE", employee);
		}

		public async Task<EmployeeApiResponse<Employee>> DeleteEmployeeAsync(long emp_details_id)
		{
			try
			{
				Console.WriteLine($"Attempting to delete employee with ID: {emp_details_id}");

				using var connection = new NpgsqlConnection(_connectionString);
				await connection.OpenAsync();

				using var command = new NpgsqlCommand("DELETE FROM public.tbl_employee WHERE emp_details_id = @EmpDetailsId", connection);
				command.Parameters.AddWithValue("@EmpDetailsId", emp_details_id);

				var rowsAffected = await command.ExecuteNonQueryAsync();

				if (rowsAffected > 0)
				{
					Console.WriteLine($"Successfully deleted employee with ID: {emp_details_id}");
					return new EmployeeApiResponse<Employee>
					{
						success = true,
						message = "Employee deleted successfully",
						data = null
					};
				}
				else
				{
					Console.WriteLine($"No employee found with ID: {emp_details_id}");
					return new EmployeeApiResponse<Employee>
					{
						success = false,
						message = "Employee not found",
						data = null
					};
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error deleting employee {emp_details_id}: {ex.Message}");
				return new EmployeeApiResponse<Employee>
				{
					success = false,
					message = $"Error deleting employee: {ex.Message}",
					data = null
				};
			}
		}

		// ============ PRIVATE METHODS ============

		private async Task<EmployeeApiResponse<Employee>> ExecuteEmployeeOperation(string operation, Employee employee)
		{
			try
			{
				Console.WriteLine($"\n=== Executing operation: {operation} ===");
				Console.WriteLine($"Employee ID: {employee.emp_details_id}");
				Console.WriteLine($"Name: {employee.employee_name}");
				Console.WriteLine($"Amount: {employee.employee_amount}");
				Console.WriteLine($"Description: {employee.employee_descripation}");
				Console.WriteLine($"Insert Date: {employee.insert_date}");

				using var connection = new NpgsqlConnection(_connectionString);
				await connection.OpenAsync();
				Console.WriteLine("Database connection opened successfully");

				using var command = new NpgsqlCommand(
					"SELECT manage_employee(@Operation, @EmpDetailsId, @EmployeeName, @EmployeeAmount, @EmployeeDescription, @InsertDate)",
					connection
				);

				// Add parameters with proper null handling
				command.Parameters.AddWithValue("@Operation", NpgsqlDbType.Varchar, operation);
				command.Parameters.AddWithValue("@EmpDetailsId", NpgsqlDbType.Bigint, employee.emp_details_id);
				command.Parameters.AddWithValue("@EmployeeName", NpgsqlDbType.Varchar, (object)employee.employee_name ?? DBNull.Value);
				command.Parameters.AddWithValue("@EmployeeAmount", NpgsqlDbType.Numeric, (object)employee.employee_amount ?? DBNull.Value);
				command.Parameters.AddWithValue("@EmployeeDescription", NpgsqlDbType.Varchar, (object)employee.employee_descripation ?? DBNull.Value);

				// FIX: Handle date parameter properly
				if (employee.insert_date.HasValue)
				{
					command.Parameters.AddWithValue("@InsertDate", NpgsqlDbType.Date, employee.insert_date.Value.Date); // Use .Date to get only date part
				}
				else
				{
					command.Parameters.AddWithValue("@InsertDate", NpgsqlDbType.Date, DBNull.Value);
				}

				Console.WriteLine("Executing database command...");
				var result = await command.ExecuteScalarAsync();

				if (result == null)
				{
					Console.WriteLine("Database returned NULL");
					return new EmployeeApiResponse<Employee>
					{
						success = false,
						message = "Database returned no result",
						data = null
					};
				}

				Console.WriteLine($"Raw database response: {result}");
				var jsonResponse = JsonSerializer.Deserialize<JsonElement>(result.ToString()!);

				return ParseResponse(operation, jsonResponse);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Database operation failed: {ex.Message}");
				Console.WriteLine($"Stack trace: {ex.StackTrace}");
				return new EmployeeApiResponse<Employee>
				{
					success = false,
					message = $"Operation failed: {ex.Message}",
					data = null
				};
			}
		}

		private EmployeeApiResponse<Employee> ParseResponse(string operation, JsonElement jsonResponse)
		{
			try
			{
				Console.WriteLine($"\n=== Parsing response for operation: {operation} ===");
				Console.WriteLine($"Full JSON Response: {jsonResponse}");

				if (!jsonResponse.TryGetProperty("success", out var successElement))
				{
					Console.WriteLine("ERROR: Missing 'success' property in response");
					return new EmployeeApiResponse<Employee>
					{
						success = false,
						message = "Invalid response format: missing 'success' property",
						data = null
					};
				}

				var success = successElement.GetBoolean();

				var message = jsonResponse.TryGetProperty("message", out var messageElement)
					? messageElement.GetString() ?? "Operation completed"
					: "Operation completed";

				Console.WriteLine($"Success: {success}, Message: {message}");

				if (!success)
				{
					return new EmployeeApiResponse<Employee>
					{
						success = false,
						message = message,
						data = null
					};
				}

				// Handle different operations based on their specific response formats
				switch (operation)
				{
					case "INSERT":
						var createResponse = new EmployeeApiResponse<Employee>
						{
							success = true,
							message = message,
							data = null
						};

						if (jsonResponse.TryGetProperty("emp_details_id", out var empIdElement))
						{
							createResponse.emp_details_id = empIdElement.GetInt64();
							Console.WriteLine($"Created employee with ID: {createResponse.emp_details_id}");
						}

						return createResponse;

					case "SELECT":
						if (jsonResponse.TryGetProperty("data", out var readDataElement))
						{
							var employee = ParseEmployeeData(readDataElement);
							return new EmployeeApiResponse<Employee>
							{
								success = true,
								message = message,
								data = employee
							};
						}
						else
						{
							Console.WriteLine("ERROR: SELECT operation missing 'data' property");
							return new EmployeeApiResponse<Employee>
							{
								success = false,
								message = "SELECT operation: missing data property",
								data = null
							};
						}

					case "UPDATE":
						return new EmployeeApiResponse<Employee>
						{
							success = true,
							message = message,
							data = null
						};

					case "LIST":
						var listResponse = new EmployeeApiResponse<Employee>
						{
							success = true,
							message = message,
							data = null,
							total = jsonResponse.TryGetProperty("total", out var totalElement)
								? totalElement.GetInt32()
								: 0
						};

						Console.WriteLine("LIST operation completed - data will be handled separately");
						return listResponse;

					default:
						Console.WriteLine($"ERROR: Unknown operation: {operation}");
						return new EmployeeApiResponse<Employee>
						{
							success = false,
							message = $"Unknown operation: {operation}",
							data = null
						};
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error parsing response: {ex.Message}");
				Console.WriteLine($"Stack trace: {ex.StackTrace}");
				return new EmployeeApiResponse<Employee>
				{
					success = false,
					message = $"Error parsing response: {ex.Message}",
					data = null
				};
			}
		}

		private Employee ParseEmployeeData(JsonElement dataElement)
		{
			var employee = new Employee();

			try
			{
				Console.WriteLine("Parsing employee data...");

				if (dataElement.TryGetProperty("emp_details_id", out var empIdElement) && empIdElement.ValueKind != JsonValueKind.Null)
					employee.emp_details_id = empIdElement.GetInt64();

				if (dataElement.TryGetProperty("employee_name", out var nameElement) && nameElement.ValueKind != JsonValueKind.Null)
					employee.employee_name = nameElement.GetString();

				if (dataElement.TryGetProperty("employee_amount", out var amountElement) && amountElement.ValueKind != JsonValueKind.Null)
					employee.employee_amount = amountElement.GetDecimal();

				if (dataElement.TryGetProperty("employee_descripation", out var descElement) && descElement.ValueKind != JsonValueKind.Null)
					employee.employee_descripation = descElement.GetString();

				// ADDED: Parse insert_date field
				if (dataElement.TryGetProperty("insert_date", out var insertDateElement) && insertDateElement.ValueKind != JsonValueKind.Null)
					employee.insert_date = insertDateElement.GetDateTime();

				Console.WriteLine($"Successfully parsed employee: {employee.emp_details_id} - {employee.employee_name} - {employee.employee_amount} - {employee.insert_date}");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error parsing employee data: {ex.Message}");
			}

			return employee;
		}

		private static string MaskPassword(string connectionString)
		{
			if (string.IsNullOrEmpty(connectionString)) return "NULL";

			try
			{
				var startIndex = connectionString.IndexOf("Password=", StringComparison.OrdinalIgnoreCase);
				if (startIndex == -1) return connectionString;

				startIndex += 9;
				var endIndex = connectionString.IndexOf(';', startIndex);
				if (endIndex == -1) endIndex = connectionString.Length;

				var password = connectionString.Substring(startIndex, endIndex - startIndex);
				return connectionString.Replace(password, "******");
			}
			catch
			{
				return connectionString;
			}
		}
	}
}