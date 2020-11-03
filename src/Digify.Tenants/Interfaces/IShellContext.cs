namespace Digify.Tenants
{
    public interface IShellContext<T>  where T : class, ITenant, new()
    {
        T TenantInfo { get; set; }
    }
}