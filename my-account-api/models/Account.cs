namespace my_account_api.models
{
	public class Account
	{
		public long acid { get; set; }

		public string? name { get; set; }
		public decimal? getmoney { get; set; }
		public decimal? intrest { get; set; }
		public decimal? givemoney { get; set; }
		public DateTime? date { get; set; }
		public string? agent { get; set; }
		public string? remark { get; set; }
		public decimal? utino { get; set; }

		public string? givename { get; set; }
		public string? giveremark { get; set; }
		public decimal? giveutino { get; set; }
		public DateTime? givedate { get; set; }
		public string? giveagent { get; set; }

		public DateTime? created_at { get; set; }
		public DateTime? modified_at { get; set; }

		public DateTime? start_date { get; set; }
		public DateTime? end_date { get; set; }
		public bool? ismoney { get; set; }

	}
}
