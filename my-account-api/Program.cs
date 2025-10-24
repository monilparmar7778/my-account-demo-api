using my_account_api.Interface;
using my_account_api.Services;
using my_account_api.models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace my_account_api;

public class Program
{
	public static void Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);

		Console.WriteLine("=== STARTING APPLICATION ===");

		// Add services to the container
		builder.Services.AddControllers();
		builder.Services.AddLogging();

		// Register Services
		builder.Services.AddScoped<IAccountService, AccountService>();
		builder.Services.AddScoped<IUserService, UserService>();
		builder.Services.AddScoped<IAccountRecordService, AccountRecordService>();
		builder.Services.AddScoped<IAuthService, AuthService>(); // Add Auth Service

		// Configure JWT Settings
		builder.Services.Configure<AuthModel>(builder.Configuration.GetSection("AuthModel"));					

		// JWT Authentication Configuration
		var authSettings = builder.Configuration.GetSection("AuthModel").Get<AuthModel>();
		var key = Encoding.ASCII.GetBytes(authSettings?.Secret ?? "YourSuperSecretKeyForJWTTokenGeneration2024!");

		builder.Services.AddAuthentication(options =>
		{
			options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
			options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
		})
		.AddJwtBearer(options =>
		{
			options.RequireHttpsMetadata = false;
			options.SaveToken = true;
			options.TokenValidationParameters = new TokenValidationParameters
			{
				ValidateIssuerSigningKey = true,
				IssuerSigningKey = new SymmetricSecurityKey(key),
				ValidateIssuer = true,
				ValidateAudience = true,
				ValidIssuer = authSettings?.Issuer ?? "MyAccountAPI",
				ValidAudience = authSettings?.Audience ?? "MyAccountApp",
				ClockSkew = TimeSpan.Zero
			};
		});

		builder.Services.AddAuthorization();

		// Add CORS - IMPORTANT: Configure properly for Angular
		builder.Services.AddCors(options =>
		{
			options.AddPolicy("AllowAngularApp", policy =>
			{
				policy.WithOrigins(
					"http://localhost:4200",
					"https://my-account-demo.onrender.com"  // Add your deployed Angular URL
				)
				.AllowAnyHeader()
				.AllowAnyMethod()
				.AllowCredentials();
			});
		});

		builder.Services.AddEndpointsApiExplorer();
		builder.Services.AddSwaggerGen();

		var app = builder.Build();

		// Configure the HTTP request pipeline
		if (app.Environment.IsDevelopment())
		{
			app.UseSwagger();
			app.UseSwaggerUI(c =>
			{
				c.SwaggerEndpoint("/swagger/v1/swagger.json", "My Account API v1");
				c.ConfigObject.AdditionalItems["persistAuthorization"] = "false"; // Disable auth persistence
				c.ConfigObject.AdditionalItems["displayRequestDuration"] = true; // Optional: show request duration
			});
			app.UseDeveloperExceptionPage();
		}

		// Use CORS - MUST be before UseAuthorization and MapControllers
		app.UseCors("AllowAngularApp");

		// Authentication & Authorization Middleware
		app.UseAuthentication(); // Add this line - MUST come before UseAuthorization
		app.UseHttpsRedirection();
		app.UseAuthorization();

		app.MapControllers();

		Console.WriteLine("=== APPLICATION STARTED SUCCESSFULLY ===");
		Console.WriteLine("API URL: https://localhost:7230");
		Console.WriteLine("Angular URL: http://localhost:4200");
		Console.WriteLine("CORS configured for Angular app");
		Console.WriteLine("JWT Authentication configured");

		app.Run();
	}
}