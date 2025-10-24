using my_account_api.Interface;
using my_account_api.models;
using Npgsql;
using NpgsqlTypes;

namespace my_account_api.Services
{
	public class AccountRecordService : IAccountRecordService
	{
		private readonly string _connectionString;

		public AccountRecordService(IConfiguration configuration)
		{
			_connectionString = configuration.GetConnectionString("PostgreConnection")
				?? "Host=localhost;Port=5432;Database=my_account_demo;Username=postgres;Password=667866;";
		}

		public async Task<AccountRecordsResponse> GetAccountRecordsAsync(AccountRecordsRequest request)
		{
			try
			{
				// Validate required fields
				if (string.IsNullOrEmpty(request.username))
				{
					return new AccountRecordsResponse
					{
						success = false,
						message = "Username is required",
						data = new List<AccountRecord>(),
						total = 0
					};
				}

				// Handle sorting - update default sort field
				string sortBy = "get_date"; // Changed from transaction_date
				string sortDir = "DESC";

				if (request.sort != null && request.sort.Count > 0)
				{
					var firstSort = request.sort[0];
					sortBy = firstSort.field ?? "get_date"; // Changed default
					sortDir = (firstSort.dir ?? "asc").ToUpper();
				}

				using var connection = new NpgsqlConnection(_connectionString);
				await connection.OpenAsync();

				using var command = new NpgsqlCommand(
					"SELECT * FROM get_account_records_for_grid_with_totals(@Username, @FromDate, @ToDate, @Skip, @Take, @SortBy, @SortDir)",
					connection
				);

				command.Parameters.AddWithValue("@Username", NpgsqlDbType.Varchar, request.username);

				// Convert string dates to DateTime
				if (!string.IsNullOrEmpty(request.from_date) && DateTime.TryParse(request.from_date, out DateTime fromDate))
				{
					command.Parameters.AddWithValue("@FromDate", NpgsqlDbType.Date, fromDate);
				}
				else
				{
					command.Parameters.AddWithValue("@FromDate", NpgsqlDbType.Date, DBNull.Value);
				}

				if (!string.IsNullOrEmpty(request.to_date) && DateTime.TryParse(request.to_date, out DateTime toDate))
				{
					command.Parameters.AddWithValue("@ToDate", NpgsqlDbType.Date, toDate);
				}
				else
				{
					command.Parameters.AddWithValue("@ToDate", NpgsqlDbType.Date, DBNull.Value);
				}

				command.Parameters.AddWithValue("@Skip", NpgsqlDbType.Integer, request.skip);
				command.Parameters.AddWithValue("@Take", NpgsqlDbType.Integer, request.take);
				command.Parameters.AddWithValue("@SortBy", NpgsqlDbType.Varchar, sortBy);
				command.Parameters.AddWithValue("@SortDir", NpgsqlDbType.Varchar, sortDir);

				using var reader = await command.ExecuteReaderAsync();

				var records = new List<AccountRecord>();
				decimal totalGetMoney = 0;
				decimal totalGiveMoney = 0;
				decimal totalInterestAmount = 0; // Changed from totalInterest
				decimal netBalance = 0;
				int totalCount = 0;

				while (await reader.ReadAsync())
				{
					// Extract summary from first record (same for all records)
					if (records.Count == 0)
					{
						totalGetMoney = reader.GetDecimal(reader.GetOrdinal("total_get_money"));
						totalGiveMoney = reader.GetDecimal(reader.GetOrdinal("total_give_money"));
						totalInterestAmount = reader.GetDecimal(reader.GetOrdinal("total_interest_amount")); // Changed
						netBalance = reader.GetDecimal(reader.GetOrdinal("net_balance"));
						totalCount = reader.GetInt32(reader.GetOrdinal("total_count"));
					}

					// Create account record with NULL handling for new date fields
					var record = new AccountRecord
					{
						transaction_id = reader.GetInt64(reader.GetOrdinal("transaction_id")),
						transaction_type = reader.GetString(reader.GetOrdinal("transaction_type")),
						amount = reader.GetDecimal(reader.GetOrdinal("amount")),
						getmoney = reader.GetDecimal(reader.GetOrdinal("getmoney")),         
						givemoney = reader.GetDecimal(reader.GetOrdinal("givemoney")),         
						interest_percentage = reader.GetDecimal(reader.GetOrdinal("interest_percentage")), 
						interest_amount = reader.GetDecimal(reader.GetOrdinal("interest_amount")), 
						net_amount = reader.GetDecimal(reader.GetOrdinal("net_amount")),

						// Handle nullable date fields
						get_date = reader.IsDBNull(reader.GetOrdinal("get_date")) ? null : reader.GetDateTime(reader.GetOrdinal("get_date")),
						give_date = reader.IsDBNull(reader.GetOrdinal("give_date")) ? null : reader.GetDateTime(reader.GetOrdinal("give_date")),

						// Handle nullable columns
						agent = reader.IsDBNull(reader.GetOrdinal("agent")) ? null : reader.GetString(reader.GetOrdinal("agent")),
						// ✅ Correct
						remark = reader.IsDBNull(reader.GetOrdinal("remark")) ? null : reader.GetString(reader.GetOrdinal("remark")),

						// Handle nullable utino column
						utino = reader.IsDBNull(reader.GetOrdinal("utino")) ? 0 : reader.GetDecimal(reader.GetOrdinal("utino")),

						party_name = reader.GetString(reader.GetOrdinal("party_name")),
						full_name = reader.GetString(reader.GetOrdinal("full_name")),
						mobile_no = reader.GetString(reader.GetOrdinal("mobile_no")),
						email = reader.GetString(reader.GetOrdinal("email")),

						// Summary fields (same for all records)
						total_get_money = totalGetMoney,
						total_give_money = totalGiveMoney,
						total_interest_amount = totalInterestAmount, // Changed
						net_balance = netBalance,
						total_count = totalCount
					};

					records.Add(record);
				}

				return new AccountRecordsResponse
				{
					success = true,
					message = "Account records retrieved successfully",
					data = records,
					total = totalCount,
					total_get_money = totalGetMoney,
					total_give_money = totalGiveMoney,
					total_interest_amount = totalInterestAmount, 
					net_balance = netBalance
				};
			}
			catch (Exception ex)
			{
				return new AccountRecordsResponse
				{
					success = false,
					message = $"Error retrieving account records: {ex.Message}",
					data = new List<AccountRecord>(),
					total = 0
				};
			}
		}
	}
}