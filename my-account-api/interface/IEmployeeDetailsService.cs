using my_account_api.models;

namespace my_account_api.Interface
{
	public interface IEmployeeDetailsService
	{
		Task<EmployeeResponse> CreateEmployeeAsync(EmployeeDetails employee);
		Task<EmployeesResponse> GetEmployeesBasicInfoAsync();
	}
	public class EmployeeResponse
	{
		public bool success { get; set; }
		public string message { get; set; }
		public EmployeeDetails data { get; set; }
		public long? employee_id { get; set; }
	}

	public class EmployeesResponse
	{
		public bool success { get; set; }
		public string message { get; set; }
		public List<EmployeeDetails> data { get; set; } = new List<EmployeeDetails>();
	}
}
