using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Digify.Tenants.Stores
{
    public class InMemoryStore<TTenantInfo> : IShellStore<TTenantInfo>
        where TTenantInfo : class, ITenant, new()
    {
        private readonly ConcurrentDictionary<string, TTenantInfo> tenantMap;
        private readonly InMemoryStoreOptions<TTenantInfo> options;

        public InMemoryStore(IOptions<InMemoryStoreOptions<TTenantInfo>> options)
        {
            this.options = options?.Value ?? new InMemoryStoreOptions<TTenantInfo>();

            var stringComparer = StringComparer.OrdinalIgnoreCase;
            if(this.options.IsCaseSensitive)
                stringComparer = StringComparer.Ordinal;
            
            tenantMap = new ConcurrentDictionary<string, TTenantInfo>(stringComparer);
            foreach(var tenant in this.options.Tenants)
            {
                if(String.IsNullOrWhiteSpace(tenant.Id))
                    throw new MultiTenantException("Missing tenant id in options.");
                if(String.IsNullOrWhiteSpace(tenant.Identifier))
                    throw new MultiTenantException("Missing tenant identifier in options.");
                if(tenantMap.ContainsKey(tenant.Identifier))
                    throw new MultiTenantException("Duplicate tenant identifier in options.");

                tenant.Domain = !string.IsNullOrEmpty(tenant.Domain) ? tenant.Domain.Replace("https://", "").Replace("http://", "") : string.Empty;
                tenantMap.TryAdd(tenant.Identifier, tenant);
            }
        }

        public virtual async Task<TTenantInfo> TryGetAsync(string id)
        {
            var result = tenantMap.Values.Where(ti => ti.Id == id).FirstOrDefault();
            
            return await Task.FromResult(result);
        }

        public virtual async Task<TTenantInfo> TryGetByIdentifierAsync(string identifier)
        {
            tenantMap.TryGetValue(identifier, out var result);
            
            return await Task.FromResult(result);
        }
        public async Task<TTenantInfo> TryGetByIdentifierAsync((string domainName, string ipAddresss, string name) identifiers)
        {
            if(tenantMap.TryGetValue(identifiers.name, out var result))
            {
                return await Task.FromResult(result);
            }
            if (tenantMap.Values.Any(e => e.Domain.ToLower() == identifiers.domainName.ToLower()))
            {
                return await Task.FromResult(tenantMap.Values.First(e => e.Domain == identifiers.domainName));
            }
            return null;
        }
        public async Task<IEnumerable<TTenantInfo>> GetAllAsync()
        {
            return await Task.FromResult(tenantMap.Select(x => x.Value).OrderBy(e => e.Id).ToList());
        }

        public async Task<bool> TryAddAsync(TTenantInfo tenantInfo)
        {
            tenantInfo.Domain = !string.IsNullOrEmpty(tenantInfo.Domain) ? tenantInfo.Domain.Replace("https://", "").Replace("http://", "") : string.Empty;

            var result = tenantMap.TryAdd(tenantInfo.Identifier, tenantInfo);

            return await Task.FromResult(result);
        }

        public async Task<bool> TryRemoveAsync(string identifier)
        {
            var result = tenantMap.TryRemove(identifier, out var dummy);

            return await Task.FromResult(result);
        }

        public async Task<bool> TryUpdateAsync(TTenantInfo tenantInfo)
        {
            var existingTenantInfo = await TryGetAsync(tenantInfo.Id);

            if(existingTenantInfo != null)
            {
                existingTenantInfo = tenantInfo;
            }

            return existingTenantInfo != null;
        }

     
    }
}