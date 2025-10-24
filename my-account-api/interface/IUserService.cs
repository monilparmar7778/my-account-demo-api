using my_account_api.models;

namespace my_account_api.Interface
{
	public interface IUserService
	{
		Task<UserResponse> CreateUserAsync(User user);
	}

	public class UserResponse
	{
		public bool success { get; set; }
		public string message { get; set; }
		public User data { get; set; }
		public long? user_id { get; set; }
	}
}