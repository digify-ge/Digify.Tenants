using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Digify.Tenants
{
    public interface ITenantResolutionStrategy
    {
        Task<(string domainName, string ipAddresss, string name)> GetTenantIdentifierAsync();
    }
}
