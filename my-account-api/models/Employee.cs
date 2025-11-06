namespace my_account_api.models
{
	public class Employee
	{
		public long emp_details_id { get; set; }
		public string employee_name { get; set; }
		public decimal employee_amount { get; set; }
		public string employee_descripation { get; set; }
		public DateTime? insert_date { get; set; }
	}
}