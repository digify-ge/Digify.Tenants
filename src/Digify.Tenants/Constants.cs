namespace Digify.Tenants
{
    internal static class Constants
    {
        public const int TenantIdMaxLength = 64;
        public const string HttpContextTenantKey = "Tenant";
        public const string SignInScheme = "Cookies";
        public const string OidcAuthenticationScheme = "oidc";
        public const string AccountLoginPage = "Account/Login";
        public const string TenantToken = "__tenant__";
    }
}