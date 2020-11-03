using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Digify.Tenants
{
    public interface IShellContentAccessor<T> where T : class, ITenant, new()
    {
        IShellContext<T> ShellContext { get; }
        T GetTenant();
        Task<T> GetTenantAsync();
    }
}
