namespace Digify.Tenants
{
    public class ShellContext<T> : IShellContext<T>
        where T : class, ITenant, new()
    {
        public T TenantInfo { get; set; }
    }
}