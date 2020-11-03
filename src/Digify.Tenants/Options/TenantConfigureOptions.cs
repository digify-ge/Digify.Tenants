using System;

namespace Digify.Tenants.Options
{
    public class TenantConfigureOptions<TOptions, TTenantInfo> : ITenantConfigureOptions<TOptions, TTenantInfo>
        where TOptions : class, new()
        where TTenantInfo : class, ITenant, new()
    {
        private readonly Action<TOptions, TTenantInfo> configureOptions;

        public TenantConfigureOptions(Action<TOptions, TTenantInfo> configureOptions)
        {
            this.configureOptions = configureOptions;
        }

        public void Configure(TOptions options, TTenantInfo tenantInfo)
        {
            configureOptions(options, tenantInfo);
        }
    }
}