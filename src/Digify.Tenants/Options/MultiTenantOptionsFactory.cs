using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Digify.Tenants.Options
{
    internal class MultiTenantOptionsFactory<TOptions, TTenantInfo> : IOptionsFactory<TOptions>
        where TOptions : class, new()
        where TTenantInfo : class, ITenant, new()
    {
        private readonly IEnumerable<IConfigureOptions<TOptions>> configureOptions;
        private readonly IEnumerable<ITenantConfigureOptions<TOptions, TTenantInfo>> tenantConfigureOptions;
        private readonly IHttpContextAccessor httpContextxAccessor;
        private readonly IEnumerable<IPostConfigureOptions<TOptions>> postConfigureOptions;

        public MultiTenantOptionsFactory(IEnumerable<IConfigureOptions<TOptions>> configureOptions,
            IEnumerable<IPostConfigureOptions<TOptions>> postConfigureOptions, 
            IEnumerable<ITenantConfigureOptions<TOptions, TTenantInfo>> tenantConfigureOptions,
            IHttpContextAccessor httpContextxAccessor)
        {
            this.configureOptions = configureOptions;
            this.tenantConfigureOptions = tenantConfigureOptions;
            this.httpContextxAccessor = httpContextxAccessor;
            this.postConfigureOptions = postConfigureOptions;
        }

        public TOptions Create(string name)
        {
            var options = new TOptions();
            foreach (var setup in configureOptions)
            {
                if (setup is IConfigureNamedOptions<TOptions> namedSetup)
                {
                    namedSetup.Configure(name, options);
                }
                else if (name == Microsoft.Extensions.Options.Options.DefaultName)
                {
                    setup.Configure(options);
                }
            }

            // Configure tenant options.
            if(httpContextxAccessor?.HttpContext?.GetShellContext<TTenantInfo>()?.TenantInfo != null)
            {
                foreach(var tenantConfigureOption in tenantConfigureOptions)
                    tenantConfigureOption.Configure(options, httpContextxAccessor?.HttpContext.GetShellContext<TTenantInfo>().TenantInfo);
            }

            foreach (var post in postConfigureOptions)
            {
                post.PostConfigure(name, options);
            }
            return options;
        }
    }
}
