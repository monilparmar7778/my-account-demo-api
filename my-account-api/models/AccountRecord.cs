namespace my_account_api.models
{
	public class AccountRecord
	{
		public long transaction_id { get; set; }
		public string transaction_type { get; set; }
		public decimal amount { get; set; }
		public decimal interest_percentage { get; set; }  // Changed from interest
		public decimal interest_amount { get; set; }      // New field
		public decimal getmoney { get; set; }              // Add this
		public decimal givemoney { get; set; }             // Add this
		public decimal net_amount { get; set; }
		public DateTime? get_date { get; set; }           // Changed from transaction_date
		public DateTime? give_date { get; set; }          // New field
		public string agent { get; set; }
		public string remark { get; set; }
		public decimal utino { get; set; }
		public string party_name { get; set; }
		public string full_name { get; set; }
		public string mobile_no { get; set; }
		public string email { get; set; }

		// Summary fields (will be same for all records in response)
		public decimal total_get_money { get; set; }
		public decimal total_give_money { get; set; }
		public decimal total_interest_amount { get; set; } // Changed from total_interest
		public decimal net_balance { get; set; }
		public int total_count { get; set; }
	}
}
