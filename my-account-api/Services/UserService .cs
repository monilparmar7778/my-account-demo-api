﻿using System.Text.Json;
using Npgsql;
using NpgsqlTypes;
using my_account_api.models;
using my_account_api.Interface;
using Microsoft.Extensions.Configuration;

namespace my_account_api.Services
{
	public class UserService : IUserService
	{
		private readonly string _connectionString;

		public UserService(IConfiguration configuration)
		{
			_connectionString = configuration.GetConnectionString("PostgreConnection")
				?? "Host=localhost;Port=5432;Database=my_account_demo;Username=postgres;Password=667866;";
		}

		public async Task<UserResponse> CreateUserAsync(User user)
		{
			try
			{
				// Validate required fields (REMOVE password validation)
				if (string.IsNullOrEmpty(user.username) || string.IsNullOrEmpty(user.email))
				{
					return new UserResponse
					{
						success = false,
						message = "Username and email are required"
					};
				}

				using var connection = new NpgsqlConnection(_connectionString);
				await connection.OpenAsync();

				using var command = new NpgsqlCommand(
					"SELECT create_user(@Username, @Email, @MobileNo, @FullName, @Password)",
					connection
				);

				command.Parameters.AddWithValue("@Username", NpgsqlDbType.Varchar, user.username);
				command.Parameters.AddWithValue("@Email", NpgsqlDbType.Varchar, user.email);
				command.Parameters.AddWithValue("@MobileNo", NpgsqlDbType.Varchar, (object)user.mobile_no ?? DBNull.Value);
				command.Parameters.AddWithValue("@FullName", NpgsqlDbType.Varchar, (object)user.full_name ?? DBNull.Value);

				// SET DEFAULT PASSWORD HERE
				string defaultPassword = "default123";
				command.Parameters.AddWithValue("@Password", NpgsqlDbType.Varchar, defaultPassword);

				var result = await command.ExecuteScalarAsync();

				if (result == null)
				{
					return new UserResponse
					{
						success = false,
						message = "Database returned no result"
					};
				}

				var jsonResponse = JsonSerializer.Deserialize<JsonElement>(result.ToString()!);

				if (jsonResponse.TryGetProperty("success", out var successElement) && successElement.GetBoolean())
				{
					var response = new UserResponse
					{
						success = true,
						message = jsonResponse.TryGetProperty("message", out var msgElement) ? msgElement.GetString() : "User created successfully"
					};

					if (jsonResponse.TryGetProperty("user_id", out var userIdElement))
					{
						response.user_id = userIdElement.GetInt64();
					}

					return response;
				}
				else
				{
					var message = jsonResponse.TryGetProperty("message", out var msgElement) ? msgElement.GetString() : "User creation failed";
					return new UserResponse
					{
						success = false,
						message = message
					};
				}
			}
			catch (Exception ex)
			{
				return new UserResponse
				{
					success = false,
					message = $"Error creating user: {ex.Message}"
				};
			}
		}
	}
}