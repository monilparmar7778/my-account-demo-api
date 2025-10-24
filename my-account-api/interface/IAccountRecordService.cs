using my_account_api.models;

namespace my_account_api.Interface
{
	public interface IAccountRecordService
	{
		Task<AccountRecordsResponse> GetAccountRecordsAsync(AccountRecordsRequest request);
	}
	public class AccountRecordsResponse
	{
		public bool success { get; set; }
		public string message { get; set; }
		public List<AccountRecord> data { get; set; }
		public int total { get; set; }
		public decimal total_get_money { get; set; }
		public decimal total_give_money { get; set; }
		public decimal total_interest_amount { get; set; }
		public decimal net_balance { get; set; }
	}
}
