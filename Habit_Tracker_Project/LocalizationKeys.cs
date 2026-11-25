namespace Habit_Tracker_Project;

public static class LocalizationKeys
{
    public static class Navigation
    {
        public const string Login = "Navigation.Login";
        public const string Register = "Navigation.Register";
        public const string Logout = "Navigation.Logout";
        public const string Home = "Navigation.Home";
        public const string Dashboard = "Navigation.Dashboard";
        public const string Profile = "Navigation.Profile";
        public const string Notifications = "Navigation.Notifications";
    }

    public static class Auth
    {
        public const string LoginTitle = "Auth.LoginTitle";
        public const string RegisterTitle = "Auth.RegisterTitle";
        public const string Email = "Auth.Email";
        public const string Password = "Auth.Password";
        public const string ConfirmPassword = "Auth.ConfirmPassword";
        public const string RememberMe = "Auth.RememberMe";
        public const string FullName = "Auth.FullName";
    }

    public static class Common
    {
        public const string Save = "Common.Save";
        public const string Cancel = "Common.Cancel";
        public const string Search = "Common.Search";
        public const string Reset = "Common.Reset";
        public const string Delete = "Common.Delete";
        public const string Edit = "Common.Edit";
    }

    public static class Validation
    {
        public const string Required = "Validation.Required";
        public const string InvalidEmail = "Validation.InvalidEmail";
        public const string PasswordsDoNotMatch = "Validation.PasswordsDoNotMatch";
    }

    public static class Messages
    {
        public const string InvalidLogin = "Messages.InvalidLogin";
        public const string AccountNotFound = "Messages.AccountNotFound";
        public const string EmailAlreadyRegistered = "Messages.EmailAlreadyRegistered";
        public const string AccountBlocked = "Messages.AccountBlocked";
    }
}
