namespace BusinessCRM.ViewModels
{
    public class EmployeeDashboardViewModel
    {
        public string FullName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsClockedIn { get; set; }
        public DateTime? ClockInTime { get; set; }
        public int AssignedTasksCount { get; set; }
    }
}
