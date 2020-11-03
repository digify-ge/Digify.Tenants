using Digify.Tenants;
using Digify.Tenants.AspNetCore;

namespace Microsoft.AspNetCore.Builder
{
    public static class MultiTenantApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseMultyTenancy<Tenant>(this IApplicationBuilder builder)
            where Tenant : class, ITenant, new()
            => builder.UseMiddleware<MultiTenantMiddleware<Tenant>>();
        
    }
}