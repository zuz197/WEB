using SV22T1020438.Models.HR;

namespace SV22T1020438.Admin.Models
{
    public class EmployeeChangePasswordViewModel
    {
        public Employee Employee { get; set; } = new Employee();
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}

