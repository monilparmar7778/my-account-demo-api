namespace my_account_api.models
{
	public class User
	{
		public long user_id { get; set; }
		public string username { get; set; }
		public string email { get; set; }
		public string mobile_no { get; set; }
		public string full_name { get; set; }
		public string? password { get; set; } // Required for CREATE only
		public bool is_active { get; set; } = true;
		public DateTime? created_at { get; set; }
		public DateTime? updated_at { get; set; }
	}
}
