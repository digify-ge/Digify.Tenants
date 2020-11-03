using Digify.Tenants.Implementations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Digify.Tenants
{
    public static class HttpContextExtensions
    {
        public static IShellContext<T> GetShellContext<T>(this HttpContext context)
        where T : class, ITenant, new()
        {
            if (context == null || !context.Items.ContainsKey(Constants.HttpContextTenantKey))
                return null;
            return context.Items[Constants.HttpContextTenantKey] as IShellContext<T>;
        }
    }
}