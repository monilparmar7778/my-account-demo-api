using my_account_api.models;

namespace my_account_api.Interface
{
	public interface IEmployeeService
	{
		Task<EmployeeApiResponse<Employee>> CreateEmployeeAsync(Employee employee);
		Task<EmployeeApiResponse<Employee>> GetEmployeeByIdAsync(long emp_details_id);
		Task<EmployeeApiResponse<List<Employee>>> GetEmployeesAsync();
		Task<EmployeeApiResponse<Employee>> UpdateEmployeeAsync(Employee employee);
		Task<EmployeeApiResponse<Employee>> DeleteEmployeeAsync(long emp_details_id);
	}

	public class EmployeeApiResponse<T>
	{
		public bool success { get; set; }
		public string message { get; set; }
		public T data { get; set; }
		public long? emp_details_id { get; set; }
		public int? total { get; set; }
	}
}