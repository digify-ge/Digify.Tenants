using System;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Digify.Tenants.Options
{
    public class MultiTenantOptionsCache<TOptions, TTenantInfo> : IOptionsMonitorCache<TOptions>
        where TOptions : class
        where TTenantInfo : class, ITenant, new()
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly ConcurrentDictionary<string, IOptionsMonitorCache<TOptions>> map = new ConcurrentDictionary<string, IOptionsMonitorCache<TOptions>>();

        public MultiTenantOptionsCache(IHttpContextAccessor httpContextAccessor)
        {
            this._httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public void Clear()
        {
            var tenantId = _httpContextAccessor.HttpContext.GetShellContext<TTenantInfo>()?.TenantInfo?.Id ?? "";
            var cache = map.GetOrAdd(tenantId, new OptionsCache<TOptions>());

            cache.Clear();
        }

        public void Clear(string tenantId)
        {
            tenantId = tenantId ?? "";
            var cache = map.GetOrAdd(tenantId, new OptionsCache<TOptions>());

            cache.Clear();
        }

        public void ClearAll()
        {
            foreach(var cache in map.Values)
                cache.Clear();
        }

        public TOptions GetOrAdd(string name, Func<TOptions> createOptions)
        {
            if (createOptions == null)
            {
                throw new ArgumentNullException(nameof(createOptions));
            }

            name = name ?? Microsoft.Extensions.Options.Options.DefaultName;
            var tenantId = _httpContextAccessor.HttpContext.GetShellContext<TTenantInfo>()?.TenantInfo?.Id ?? "";

            var cache = map.GetOrAdd(tenantId, new OptionsCache<TOptions>());

            return cache.GetOrAdd(name, createOptions);
        }

        public bool TryAdd(string name, TOptions options)
        {
            name = name ?? Microsoft.Extensions.Options.Options.DefaultName;
            var tenantId = _httpContextAccessor.HttpContext.GetShellContext<TTenantInfo>()?.TenantInfo?.Id ?? "";

            var cache = map.GetOrAdd(tenantId, new OptionsCache<TOptions>());

            return cache.TryAdd(name, options);
        }

        public bool TryRemove(string name)
        {
            name = name ?? Microsoft.Extensions.Options.Options.DefaultName;
            var tenantId = _httpContextAccessor.HttpContext.GetShellContext<TTenantInfo>()?.TenantInfo?.Id ?? "";

            var cache = map.GetOrAdd(tenantId, new OptionsCache<TOptions>());

            return cache.TryRemove(name);
        }
    }
}