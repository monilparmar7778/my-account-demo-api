	using System.Text.Json;
using Npgsql;
using NpgsqlTypes;
using my_account_api.models;
using my_account_api.Interface;
using Microsoft.Extensions.Configuration;

namespace my_account_api.Services
{
	public class AccountService : IAccountService
	{
		private readonly string _connectionString;

		public AccountService(IConfiguration configuration)
		{
			_connectionString = configuration.GetConnectionString("PostgreConnection");

			if (string.IsNullOrEmpty(_connectionString))
			{
				_connectionString = "Host=localhost;Port=5432;Database=my_account_demo;Username=postgres;Password=667866;";
				Console.WriteLine("Using fallback connection string");
			}

			Console.WriteLine($"AccountService initialized: {MaskPassword(_connectionString)}");
		}

		// ============ SEPARATE OPERATIONS ============

		public async Task<ApiResponse<Account>> CreateGetMoneyAsync(Account account)
		{
			return await ExecuteAccountOperation("CREATE_GET_MONEY", account);
		}

		public async Task<ApiResponse<Account>> CreateGiveMoneyAsync(Account account)
		{
			return await ExecuteAccountOperation("CREATE_GIVE_MONEY", account);
		}

		public async Task<ApiResponse<Account>> CreateCompleteTransactionAsync(Account account)
		{
			return await ExecuteAccountOperation("CREATE_COMPLETE", account);
		}

		// ============ EXISTING OPERATIONS ============

		public async Task<ApiResponse<Account>> CreateAccountAsync(Account account)
		{
			return await ExecuteAccountOperation("CREATE", account);
		}

		public async Task<ApiResponse<Account>> GetAccountByIdAsync(long acid)
		{
			var tempAccount = new Account { acid = acid };
			return await ExecuteAccountOperation("READ", tempAccount);
		}

		public async Task<ApiResponse<List<Account>>> GetAccountsAsync()
		{
			try
			{
				Console.WriteLine("Getting all accounts...");

				using var connection = new NpgsqlConnection(_connectionString);
				await connection.OpenAsync();
				Console.WriteLine("Database connection opened successfully");

				using var command = new NpgsqlCommand(
					"SELECT account_operation('LIST', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)",
					connection
				);

				Console.WriteLine("Executing database command for LIST operation...");
				var result = await command.ExecuteScalarAsync();

				if (result == null)
				{
					Console.WriteLine("Database returned NULL");
					return new ApiResponse<List<Account>>
					{
						success = false,
						message = "Database returned no result",
						data = new List<Account>(),
						total = 0
					};
				}

				Console.WriteLine($"Raw database response: {result}");
				var jsonResponse = JsonSerializer.Deserialize<JsonElement>(result.ToString()!);

				if (jsonResponse.TryGetProperty("success", out var successElement) && successElement.GetBoolean())
				{
					if (jsonResponse.TryGetProperty("data", out var dataElement) && dataElement.ValueKind == JsonValueKind.Array)
					{
						var accounts = JsonSerializer.Deserialize<List<Account>>(dataElement.GetRawText());
						var total = jsonResponse.TryGetProperty("total", out var totalElement) ? totalElement.GetInt32() : 0;

						Console.WriteLine($"Successfully retrieved {accounts?.Count ?? 0} accounts");

						return new ApiResponse<List<Account>>
						{
							success = true,
							message = "Accounts retrieved successfully",
							data = accounts ?? new List<Account>(),
							total = total
						};
					}
					else
					{
						Console.WriteLine("No data array found in response");
						return new ApiResponse<List<Account>>
						{
							success = true,
							message = "No accounts found",
							data = new List<Account>(),
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
					return new ApiResponse<List<Account>>
					{
						success = false,
						message = message,
						data = new List<Account>(),
						total = 0
					};
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error in GetAccountsAsync: {ex.Message}");
				return new ApiResponse<List<Account>>
				{
					success = false,
					message = $"Error: {ex.Message}",
					data = new List<Account>(),
					total = 0
				};
			}
		}

		public async Task<ApiResponse<List<Account>>> GetAccountsByDateRangeAsync(DateTime? startDate, DateTime? endDate)
		{
			try
			{
				Console.WriteLine($"Getting accounts by date range: {startDate} to {endDate}");

				var tempAccount = new Account
				{
					start_date = startDate,
					end_date = endDate
				};

				var result = await ExecuteAccountOperation("LIST", tempAccount);

				if (result.success)
				{
					using var connection = new NpgsqlConnection(_connectionString);
					await connection.OpenAsync();

					using var command = new NpgsqlCommand(
						"SELECT account_operation('LIST', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, @StartDate, @EndDate, NULL, NULL)",
						connection
					);

					command.Parameters.AddWithValue("@StartDate", NpgsqlDbType.Date, (object)startDate ?? DBNull.Value);
					command.Parameters.AddWithValue("@EndDate", NpgsqlDbType.Date, (object)endDate ?? DBNull.Value);

					var rawResult = await command.ExecuteScalarAsync();

					if (rawResult != null)
					{
						var jsonResponse = JsonSerializer.Deserialize<JsonElement>(rawResult.ToString()!);

						if (jsonResponse.TryGetProperty("success", out var successElement) && successElement.GetBoolean() &&
							jsonResponse.TryGetProperty("data", out var dataElement) && dataElement.ValueKind == JsonValueKind.Array)
						{
							var accounts = JsonSerializer.Deserialize<List<Account>>(dataElement.GetRawText());
							var total = jsonResponse.TryGetProperty("total", out var totalElement) ? totalElement.GetInt32() : 0;

							Console.WriteLine($"Successfully retrieved {accounts?.Count ?? 0} accounts for date range");

							return new ApiResponse<List<Account>>
							{
								success = true,
								message = result.message ?? "Accounts retrieved successfully",
								data = accounts ?? new List<Account>(),
								total = total
							};
						}
					}

					Console.WriteLine("No data returned from database for date range");
					return new ApiResponse<List<Account>>
					{
						success = true,
						message = result.message ?? "No accounts found for the specified date range",
						data = new List<Account>(),
						total = 0
					};
				}
				else
				{
					Console.WriteLine($"Database operation failed for date range: {result.message}");
					return new ApiResponse<List<Account>>
					{
						success = false,
						message = result.message ?? "Failed to retrieve accounts for date range",
						data = new List<Account>(),
						total = 0
					};
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error in GetAccountsByDateRangeAsync: {ex.Message}");
				return new ApiResponse<List<Account>>
				{
					success = false,
					message = $"Error: {ex.Message}",
					data = new List<Account>(),
					total = 0
				};
			}
		}

		public async Task<ApiResponse<Account>> UpdateAccountAsync(Account account)
		{
			return await ExecuteAccountOperation("UPDATE", account);
		}

		public async Task<ApiResponse<Account>> DeleteAccountAsync(long acid)
		{
			try
			{
				Console.WriteLine($"Attempting to delete account with ID: {acid}");

				using var connection = new NpgsqlConnection(_connectionString);
				await connection.OpenAsync();

				using var command = new NpgsqlCommand("DELETE FROM public.tbl_account WHERE acid = @Acid", connection);
				command.Parameters.AddWithValue("@Acid", acid);

				var rowsAffected = await command.ExecuteNonQueryAsync();

				if (rowsAffected > 0)
				{
					Console.WriteLine($"Successfully deleted account with ID: {acid}");
					return new ApiResponse<Account>
					{
						success = true,
						message = "Account deleted successfully",
						data = null
					};
				}
				else
				{
					Console.WriteLine($"No account found with ID: {acid}");
					return new ApiResponse<Account>
					{
						success = false,
						message = "Account not found",
						data = null
					};
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error deleting account {acid}: {ex.Message}");
				return new ApiResponse<Account>
				{
					success = false,
					message = $"Error deleting account: {ex.Message}",
					data = null
				};
			}
		}

		// ============ PRIVATE METHODS ============

		private async Task<ApiResponse<Account>> ExecuteAccountOperation(string operation, Account account)
		{
			try
			{
				Console.WriteLine($"\n=== Executing operation: {operation} ===");
				Console.WriteLine($"Account ID: {account.acid}");
				Console.WriteLine($"Name (User ID): {account.name}");
				Console.WriteLine($"Get Money: {account.getmoney}");
				Console.WriteLine($"Interest: {account.intrest}");
				Console.WriteLine($"Give Money: {account.givemoney}");
				Console.WriteLine($"Charter Description: {account.charterDescription}");
				Console.WriteLine($"Give Charter Description: {account.giveCharterDescription}");
				Console.WriteLine($"Start Date: {account.start_date}");
				Console.WriteLine($"End Date: {account.end_date}");

				using var connection = new NpgsqlConnection(_connectionString);
				await connection.OpenAsync();
				Console.WriteLine("Database connection opened successfully");

				// UPDATED SQL COMMAND: Added two new parameters for charter descriptions
				using var command = new NpgsqlCommand(
					"SELECT account_operation(@Operation, @Acid, @Name, @GetMoney, @Intrest, @GiveMoney, @Date, @Agent, @Remark, @GiveName, @GiveRemark, @UtiNo, @GiveUtiNo, @GiveDate, @GiveAgent, @StartDate, @EndDate, @CharterDescription, @GiveCharterDescription)",
					connection
				);

				// Add parameters with proper null handling
				command.Parameters.AddWithValue("@Operation", NpgsqlDbType.Varchar, operation);
				command.Parameters.AddWithValue("@Acid", NpgsqlDbType.Bigint, account.acid);
				command.Parameters.AddWithValue("@Name", NpgsqlDbType.Varchar, (object)account.name ?? DBNull.Value);
				command.Parameters.AddWithValue("@GetMoney", NpgsqlDbType.Numeric, (object)account.getmoney ?? DBNull.Value);
				command.Parameters.AddWithValue("@Intrest", NpgsqlDbType.Numeric, (object)account.intrest ?? DBNull.Value);
				command.Parameters.AddWithValue("@GiveMoney", NpgsqlDbType.Numeric, (object)account.givemoney ?? DBNull.Value);
				command.Parameters.AddWithValue("@Date", NpgsqlDbType.Date, (object)account.date ?? DBNull.Value);
				command.Parameters.AddWithValue("@Agent", NpgsqlDbType.Varchar, (object)account.agent ?? DBNull.Value);
				command.Parameters.AddWithValue("@Remark", NpgsqlDbType.Varchar, (object)account.remark ?? DBNull.Value);
				command.Parameters.AddWithValue("@GiveName", NpgsqlDbType.Varchar, (object)account.givename ?? DBNull.Value);
				command.Parameters.AddWithValue("@GiveRemark", NpgsqlDbType.Varchar, (object)account.giveremark ?? DBNull.Value);
				command.Parameters.AddWithValue("@UtiNo", NpgsqlDbType.Numeric, (object)account.utino ?? DBNull.Value);
				command.Parameters.AddWithValue("@GiveUtiNo", NpgsqlDbType.Numeric, (object)account.giveutino ?? DBNull.Value);
				command.Parameters.AddWithValue("@GiveDate", NpgsqlDbType.Date, (object)account.givedate ?? DBNull.Value);
				command.Parameters.AddWithValue("@GiveAgent", NpgsqlDbType.Varchar, (object)account.giveagent ?? DBNull.Value);
				command.Parameters.AddWithValue("@StartDate", NpgsqlDbType.Date, (object)account.start_date ?? DBNull.Value);
				command.Parameters.AddWithValue("@EndDate", NpgsqlDbType.Date, (object)account.end_date ?? DBNull.Value);

				// ADD THESE TWO NEW PARAMETERS for charter descriptions
				command.Parameters.AddWithValue("@CharterDescription", NpgsqlDbType.Varchar, (object)account.charterDescription ?? DBNull.Value);
				command.Parameters.AddWithValue("@GiveCharterDescription", NpgsqlDbType.Varchar, (object)account.giveCharterDescription ?? DBNull.Value);

				Console.WriteLine("Executing database command...");
				var result = await command.ExecuteScalarAsync();

				if (result == null)
				{
					Console.WriteLine("Database returned NULL");
					return new ApiResponse<Account>
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
				return new ApiResponse<Account>
				{
					success = false,
					message = $"Operation failed: {ex.Message}",
					data = null
				};
			}
		}

		private ApiResponse<Account> ParseResponse(string operation, JsonElement jsonResponse)
		{
			try
			{
				Console.WriteLine($"\n=== Parsing response for operation: {operation} ===");
				Console.WriteLine($"Full JSON Response: {jsonResponse}");

				if (!jsonResponse.TryGetProperty("success", out var successElement))
				{
					Console.WriteLine("ERROR: Missing 'success' property in response");
					return new ApiResponse<Account>
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
					return new ApiResponse<Account>
					{
						success = false,
						message = message,
						data = null
					};
				}

				// Handle different operations based on their specific response formats
				switch (operation)
				{
					case "CREATE_GET_MONEY":
					case "CREATE_GIVE_MONEY":
					case "CREATE_COMPLETE":
					case "CREATE":
						var createResponse = new ApiResponse<Account>
						{
							success = true,
							message = message,
							data = null
						};

						if (jsonResponse.TryGetProperty("acid", out var acidElement))
						{
							createResponse.acid = acidElement.GetInt64();
							Console.WriteLine($"Created transaction with ID: {createResponse.acid}");
						}

						return createResponse;

					case "READ":
						if (jsonResponse.TryGetProperty("data", out var readDataElement))
						{
							var account = ParseAccountData(readDataElement);
							return new ApiResponse<Account>
							{
								success = true,
								message = message,
								data = account
							};
						}
						else
						{
							Console.WriteLine("ERROR: READ operation missing 'data' property");
							return new ApiResponse<Account>
							{
								success = false,
								message = "READ operation: missing data property",
								data = null
							};
						}

					case "UPDATE":
						return new ApiResponse<Account>
						{
							success = true,
							message = message,
							data = null
						};

					case "LIST":
						var listResponse = new ApiResponse<Account>
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
						return new ApiResponse<Account>
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
				return new ApiResponse<Account>
				{
					success = false,
					message = $"Error parsing response: {ex.Message}",
					data = null
				};
			}
		}

		private Account ParseAccountData(JsonElement dataElement)
		{
			var account = new Account();

			try
			{
				Console.WriteLine("Parsing account data...");

				if (dataElement.TryGetProperty("acid", out var acidElement) && acidElement.ValueKind != JsonValueKind.Null)
					account.acid = acidElement.GetInt64();

				if (dataElement.TryGetProperty("name", out var nameElement) && nameElement.ValueKind != JsonValueKind.Null)
					account.name = nameElement.GetString();

				if (dataElement.TryGetProperty("getmoney", out var getMoneyElement) && getMoneyElement.ValueKind != JsonValueKind.Null)
					account.getmoney = getMoneyElement.GetDecimal();

				if (dataElement.TryGetProperty("intrest", out var intrestElement) && intrestElement.ValueKind != JsonValueKind.Null)
					account.intrest = intrestElement.GetDecimal();

				if (dataElement.TryGetProperty("givemoney", out var giveMoneyElement) && giveMoneyElement.ValueKind != JsonValueKind.Null)
					account.givemoney = giveMoneyElement.GetDecimal();

				// ADD THESE: Parse charter description fields
				if (dataElement.TryGetProperty("charterdescription", out var charterDescElement) && charterDescElement.ValueKind != JsonValueKind.Null)
					account.charterDescription = charterDescElement.GetString();

				if (dataElement.TryGetProperty("givecharterdescription", out var giveCharterDescElement) && giveCharterDescElement.ValueKind != JsonValueKind.Null)
					account.giveCharterDescription = giveCharterDescElement.GetString();

				// Other existing fields...
				if (dataElement.TryGetProperty("date", out var dateElement) && dateElement.ValueKind != JsonValueKind.Null)
					account.date = dateElement.GetDateTime();

				if (dataElement.TryGetProperty("agent", out var agentElement) && agentElement.ValueKind != JsonValueKind.Null)
					account.agent = agentElement.GetString();

				if (dataElement.TryGetProperty("remark", out var remarkElement) && remarkElement.ValueKind != JsonValueKind.Null)
					account.remark = remarkElement.GetString();

				if (dataElement.TryGetProperty("givename", out var giveNameElement) && giveNameElement.ValueKind != JsonValueKind.Null)
					account.givename = giveNameElement.GetString();

				if (dataElement.TryGetProperty("giveremark", out var giveRemarkElement) && giveRemarkElement.ValueKind != JsonValueKind.Null)
					account.giveremark = giveRemarkElement.GetString();

				if (dataElement.TryGetProperty("utino", out var utiNoElement) && utiNoElement.ValueKind != JsonValueKind.Null)
					account.utino = utiNoElement.GetDecimal();

				if (dataElement.TryGetProperty("giveutino", out var giveUtiNoElement) && giveUtiNoElement.ValueKind != JsonValueKind.Null)
					account.giveutino = giveUtiNoElement.GetDecimal();

				if (dataElement.TryGetProperty("givedate", out var giveDateElement) && giveDateElement.ValueKind != JsonValueKind.Null)
					account.givedate = giveDateElement.GetDateTime();

				if (dataElement.TryGetProperty("giveagent", out var giveAgentElement) && giveAgentElement.ValueKind != JsonValueKind.Null)
					account.giveagent = giveAgentElement.GetString();

				if (dataElement.TryGetProperty("ismoney", out var ismoneyElement) && ismoneyElement.ValueKind != JsonValueKind.Null)
					account.ismoney = ismoneyElement.GetBoolean();

				Console.WriteLine($"Successfully parsed account: {account.acid} - User ID: {account.name} - Charter: {account.charterDescription} - Give Charter: {account.giveCharterDescription}");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error parsing account data: {ex.Message}");
			}

			return account;
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