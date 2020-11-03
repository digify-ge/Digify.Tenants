using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Digify.Tenants.Implementations
{
    public class ShellManager<TTenantInfo> : IShellManager<TTenantInfo> where TTenantInfo : class, ITenant, new() 
    {
        private readonly IShellStore<TTenantInfo> shellStore;

        public ShellManager(IShellStore<TTenantInfo> shellStore)
        {
            this.shellStore = shellStore;
        }
        public async Task<IEnumerable<TTenantInfo>> GetTenantsAsync()
        {
            return await shellStore.GetAllAsync();
        }
        public async Task DisableTenant(string id)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentException("id should be null or empty");

            var tenant = await shellStore.TryGetAsync(id);
            tenant.IsActive = false;
            await shellStore.TryUpdateAsync(tenant);
        }
        public async Task EnableTenant(string id)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentException("id should be null or empty");
            var tenant = await shellStore.TryGetAsync(id);
            tenant.IsActive = true;
            await shellStore.TryUpdateAsync(tenant);
        }
    }
}
