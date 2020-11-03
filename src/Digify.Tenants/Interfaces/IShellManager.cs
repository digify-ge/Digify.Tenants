using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Digify.Tenants
{
    public interface IShellManager<TTenantInfo> where TTenantInfo : class, ITenant, new()
    {
        Task<IEnumerable<TTenantInfo>> GetTenantsAsync();
        Task DisableTenant(string id);
        Task EnableTenant(string id);
    }
}
