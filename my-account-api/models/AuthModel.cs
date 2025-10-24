namespace my_account_api.models
{
	public class AuthModel
	{
		// Login Request Properties
		public string username { get; set; }
		public string password { get; set; }

		// Login Response Properties
		public bool success { get; set; }
		public string message { get; set; }
		public string token { get; set; }
		public long user_id { get; set; }
		public DateTime? expires_at { get; set; }

		// JWT Settings Properties
		public string Secret { get; set; }
		public string Issuer { get; set; }
		public string Audience { get; set; }
		public int ExpiryMinutes { get; set; }
	}
}
