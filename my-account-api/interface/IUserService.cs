using my_account_api.models;

namespace my_account_api.Interface
{
	public interface IUserService
	{
		Task<UserResponse> CreateUserAsync(User user);
		Task<UsersResponse> GetUsersBasicInfoAsync(); // Only this get method
	}

	public class UserResponse
	{
		public bool success { get; set; }
		public string message { get; set; }
		public User data { get; set; }
		public long? user_id { get; set; }
	}

	public class UsersResponse
	{
		public bool success { get; set; }
		public string message { get; set; }
		public List<User> data { get; set; } = new List<User>();
	}
}