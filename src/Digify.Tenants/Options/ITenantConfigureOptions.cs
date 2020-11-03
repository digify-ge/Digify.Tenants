namespace Digify.Tenants.Options
{
    interface ITenantConfigureOptions<TOptions, TTenantInfo>
        where TOptions : class, new()
        where TTenantInfo : class, ITenant, new()
    {
        void Configure(TOptions options, TTenantInfo tenantInfo);
    }
}