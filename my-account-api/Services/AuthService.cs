using my_account_api.models;
using Npgsql;
using NpgsqlTypes;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using my_account_api.Interface;

namespace my_account_api.Services
{
	public class AuthService: IAuthService
	{
		private readonly string _connectionString;
		private readonly AuthModel _jwtSettings;

		public AuthService(IConfiguration configuration)
		{
			_connectionString = configuration.GetConnectionString("PostgreConnection")
				?? "Host=localhost;Port=5432;Database=my_account_demo;Username=postgres;Password=667866;";

			_jwtSettings = new AuthModel
			{
				Secret = configuration["AuthModel:Secret"] ?? "YourSuperSecretKeyForJWTTokenGeneration2024!",
				Issuer = configuration["AuthModel:Issuer"] ?? "MyAccountAPI",
				Audience = configuration["AuthModel:Audience"] ?? "MyAccountApp",
				ExpiryMinutes = int.Parse(configuration["AuthModel:ExpiryMinutes"] ?? "60")
			};
		}

		public async Task<AuthModel> AuthenticateUserAsync(AuthModel request)
		{
			try
			{
				if (string.IsNullOrEmpty(request.username) || string.IsNullOrEmpty(request.password))
				{
					return new AuthModel
					{
						success = false,
						message = "Username and password are required",
						token = null,
						user_id = -1,
						username = null,
						expires_at = null
					};
				}

				using var connection = new NpgsqlConnection(_connectionString);
				await connection.OpenAsync();

				using var command = new NpgsqlCommand(
					"SELECT * FROM authenticate_user_with_message(@Username, @Password)",
					connection
				);

				command.Parameters.AddWithValue("@Username", NpgsqlDbType.Varchar, request.username);
				command.Parameters.AddWithValue("@Password", NpgsqlDbType.Varchar, request.password);

				using var reader = await command.ExecuteReaderAsync();

				if (await reader.ReadAsync())
				{
					bool success = reader.GetBoolean(reader.GetOrdinal("success"));
					string message = reader.GetString(reader.GetOrdinal("message"));
					long userId = reader.GetInt64(reader.GetOrdinal("user_id"));

					if (success && userId != -1)
					{
						var userData = new AuthModel
						{
							username = request.username,
							user_id = userId
						};

						var token = GenerateJwtToken(userData);
						var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes);

						return new AuthModel
						{
							success = true,
							message = message,
							token = token,
							user_id = userId,
							username = request.username,
							expires_at = expiresAt
						};
					}
					else
					{
						return new AuthModel
						{
							success = false,
							message = message,
							token = null,
							user_id = -1,
							username = null,
							expires_at = null
						};
					}
				}

				return new AuthModel
				{
					success = false,
					message = "Authentication failed - user not found",
					token = null,
					user_id = -1,
					username = null,
					expires_at = null
				};
			}
			catch (Exception ex)
			{
				return new AuthModel
				{
					success = false,
					message = $"Error during authentication: {ex.Message}",
					token = null,
					user_id = -1,
					username = null,
					expires_at = null
				};
			}
		}

		public string GenerateJwtToken(AuthModel user)
		{
			var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
			var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

			var claims = new[]
			{
				new Claim(JwtRegisteredClaimNames.Sub, user.username),
				new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
				new Claim("user_id", user.user_id.ToString()),
				new Claim("username", user.username)
			};

			var token = new JwtSecurityToken(
				issuer: _jwtSettings.Issuer,
				audience: _jwtSettings.Audience,
				claims: claims,
				expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
				signingCredentials: credentials
			);

			return new JwtSecurityTokenHandler().WriteToken(token);
		}
	}
}
