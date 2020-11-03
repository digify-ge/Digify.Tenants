using System.Collections.Generic;
using System.Threading.Tasks;

namespace Digify.Tenants
{
    public interface IShellStore<TTenantInfo> where TTenantInfo : class, ITenant, new()
    {
        Task<bool> TryAddAsync(TTenantInfo tenantInfo);
        Task<bool> TryUpdateAsync(TTenantInfo tenantInfo);
        Task<bool> TryRemoveAsync(string id);
        Task<TTenantInfo> TryGetByIdentifierAsync(string identifier);
        Task<TTenantInfo> TryGetByIdentifierAsync((string domainName, string ipAddresss, string name) identifiers);
        Task<TTenantInfo> TryGetAsync(string id);
        Task<IEnumerable<TTenantInfo>> GetAllAsync();
    }
}