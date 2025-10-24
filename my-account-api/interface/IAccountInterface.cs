using my_account_api.models;

namespace my_account_api.Interface
{
	public interface IAccountService
	{
		// Separate operations for Get Money and Give Money
		Task<ApiResponse<Account>> CreateGetMoneyAsync(Account account);
		Task<ApiResponse<Account>> CreateGiveMoneyAsync(Account account);
		Task<ApiResponse<Account>> CreateCompleteTransactionAsync(Account account);

		// Existing operations
		Task<ApiResponse<Account>> CreateAccountAsync(Account account);
		Task<ApiResponse<Account>> GetAccountByIdAsync(long acid);
		Task<ApiResponse<List<Account>>> GetAccountsAsync();
		Task<ApiResponse<List<Account>>> GetAccountsByDateRangeAsync(DateTime? startDate, DateTime? endDate);
		Task<ApiResponse<Account>> UpdateAccountAsync(Account account);
		Task<ApiResponse<Account>> DeleteAccountAsync(long acid);
	}

	public class ApiResponse<T>
	{
		public bool success { get; set; }
		public string message { get; set; }
		public T data { get; set; }
		public long? acid { get; set; }
		public int? total { get; set; }
	}
}