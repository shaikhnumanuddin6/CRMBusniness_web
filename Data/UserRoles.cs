namespace BusinessCRM.Data
{
    public static class UserRoles
    {
        public const string SuperAdmin = "SuperAdmin";
        public const string Admin = "Admin";
        public const string Manager = "Manager";
        public const string Employee = "Employee";

        public static readonly IReadOnlyList<string> All = new[]
        {
            SuperAdmin,
            Admin,
            Manager,
            Employee
        };
    }
}
