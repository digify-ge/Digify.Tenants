using System;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Digify.Tenants;
using Digify.Tenants.Options;
using Digify.Tenants.Implementations;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection
{
    public class TenantBuilder<TTenantInfo> where TTenantInfo : class, ITenant, new()
    {
        public IServiceCollection Services { get; set; }

        public TenantBuilder(IServiceCollection services)
        {
            Services = services;
        }
        public TenantBuilder<TTenantInfo> WithPerTenantOptions<TOptions>(Action<TOptions, TTenantInfo> tenantConfigureOptions) where TOptions : class, new()
        {
            if (tenantConfigureOptions == null)
            {
                throw new ArgumentNullException(nameof(tenantConfigureOptions));
            }

            Services.TryAddSingleton<IOptionsMonitorCache<TOptions>, MultiTenantOptionsCache<TOptions, TTenantInfo>>();

            Services.AddSingleton<ITenantConfigureOptions<TOptions, TTenantInfo>, TenantConfigureOptions<TOptions, TTenantInfo>>(sp => new TenantConfigureOptions<TOptions, TTenantInfo>(tenantConfigureOptions));
            Services.TryAddTransient<IOptionsFactory<TOptions>, MultiTenantOptionsFactory<TOptions, TTenantInfo>>();
            Services.TryAddScoped<IOptionsSnapshot<TOptions>>(sp => BuildOptionsManager<TOptions>(sp));
            Services.TryAddSingleton<IOptions<TOptions>>(sp => BuildOptionsManager<TOptions>(sp));

            return this;
        }


        private static MultiTenantOptionsManager<TOptions> BuildOptionsManager<TOptions>(IServiceProvider sp) where TOptions : class, new()
        {
            var cache = ActivatorUtilities.CreateInstance(sp, typeof(MultiTenantOptionsCache<TOptions, TTenantInfo>));
            return (MultiTenantOptionsManager<TOptions>)
                ActivatorUtilities.CreateInstance(sp, typeof(MultiTenantOptionsManager<TOptions>), new[] { cache });
        }

        public TenantBuilder<TTenantInfo> WithStore<TStore>(ServiceLifetime lifetime, params object[] parameters)
            where TStore : IShellStore<TTenantInfo>
            => WithStore<TStore>(lifetime, sp => ActivatorUtilities.CreateInstance<TStore>(sp, parameters));

        public TenantBuilder<TTenantInfo> WithStore<TStore>(ServiceLifetime lifetime, Func<IServiceProvider, TStore> factory)
            where TStore : IShellStore<TTenantInfo>
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            Services.Add(ServiceDescriptor.Describe(typeof(IShellStore<TTenantInfo>), sp => factory(sp), lifetime));

            return this;
        }
    }
}