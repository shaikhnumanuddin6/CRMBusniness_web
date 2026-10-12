using BusinessCRM.Models;

namespace BusinessCRM.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalEmployees { get; set; }
        public int TotalCustomers { get; set; }
        public int TodayClockedIn { get; set; }
        public int PendingTasks { get; set; }
        public List<Customer> RecentCustomers { get; set; } = new();
    }
}
