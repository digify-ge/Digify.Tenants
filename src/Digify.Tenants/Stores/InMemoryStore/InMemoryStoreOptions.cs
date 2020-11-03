using System.Collections.Generic;

namespace Digify.Tenants.Stores
{
    public class InMemoryStoreOptions<TTenantInfo>
        where TTenantInfo : class, ITenant, new()
    {
        public bool IsCaseSensitive { get; set; } = false;
        public IList<TTenantInfo> Tenants { get; set; } = new List<TTenantInfo>();
    }
}