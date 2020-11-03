using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Digify.Tenants.Implementations
{
    public class ShellContentAccessor<T> : IShellContentAccessor<T> where T : class, ITenant, new()
    {
        private readonly ITenantResolutionStrategy _tenantResolutionStrategy;
        private readonly IShellStore<T> _tenantStore;
        public ShellContentAccessor(ITenantResolutionStrategy tenantResolutionStrategy, IShellStore<T> tenantStore)
        {
            _tenantResolutionStrategy = tenantResolutionStrategy;
            _tenantStore = tenantStore;
        }
        public IShellContext<T> ShellContext => new ShellContext<T>()
        {
            TenantInfo = GetTenant(),
        };
        public T GetTenant()
        {
            var (domainName, ipAddress, name) = _tenantResolutionStrategy.GetTenantIdentifierAsync().Result;
            return _tenantStore.TryGetByIdentifierAsync((domainName, ipAddress, name)).Result;
        }
        public async Task<T> GetTenantAsync()
        {
            var (domainName, ipAddress, name) = await _tenantResolutionStrategy.GetTenantIdentifierAsync();
            return await _tenantStore.TryGetByIdentifierAsync(name);
        }
    }
}
