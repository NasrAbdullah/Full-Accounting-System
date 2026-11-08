using AccountingSystem.Models;

namespace AccountingSystem.Helpers
{
    public static class Session
    {
        public static User CurrentUser { get; set; }
        public static bool IsAdmin { get { return CurrentUser != null && CurrentUser.RoleName == "Admin"; } }
        public static bool IsManager { get { return CurrentUser != null && CurrentUser.RoleName == "Manager"; } }
        public static bool IsEmployee { get { return CurrentUser != null && CurrentUser.RoleName == "Employee"; } }
    }
}