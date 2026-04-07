using SV22T1020438.Models.HR;

namespace SV22T1020438.Admin.Models
{
    public class EmployeeRoleViewModel
    {
        public Employee Employee { get; set; } = new Employee();
        public List<string> SelectedRoles { get; set; } = new List<string>();
    }
}

