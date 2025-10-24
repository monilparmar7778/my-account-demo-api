namespace my_account_api.models
{
	public class AccountRecordsRequest
	{
		public string username { get; set; }  // Add this property
		public string from_date { get; set; }
		public string to_date { get; set; }
		public int skip { get; set; }
		public int take { get; set; }
		public List<SortDescriptor> sort { get; set; }  

	}
}
