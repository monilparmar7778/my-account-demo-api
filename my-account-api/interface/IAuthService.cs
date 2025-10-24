using my_account_api.models;

  namespace my_account_api.Interface
 {
	public interface IAuthService
	{
		Task<AuthModel> AuthenticateUserAsync(AuthModel request);
		string GenerateJwtToken(AuthModel user);
	}
  }
