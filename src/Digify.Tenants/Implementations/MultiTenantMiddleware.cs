using System;
using System.Net;
using System.Threading.Tasks;
using Digify.Tenants.Implementations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Digify.Tenants.AspNetCore
{
    internal class MultiTenantMiddleware<TTenant> where TTenant : class, ITenant, new()
    {
        private readonly RequestDelegate next;

        public MultiTenantMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var tenantOptions = context.RequestServices.GetService<IOptions<MultiTenantOptions>>().Value;
            var shellContextAccessor = context.RequestServices.GetService<IShellContentAccessor<TTenant>>();

            if (!context.Items.ContainsKey(Constants.HttpContextTenantKey))
            {
                if (shellContextAccessor.ShellContext != null && shellContextAccessor.ShellContext.TenantInfo != null)
                {
                    if (!shellContextAccessor.ShellContext.TenantInfo.IsActive)
                    {
                        var message = new MultiTenantException(HttpStatusCode.Forbidden, "Tenant is Disabled");
                        if (tenantOptions != null && tenantOptions.OnExecutingTenant != null)
                            await tenantOptions.OnExecutingTenant(message, context);
                        else
                            throw message;
                    }
                    if (shellContextAccessor.ShellContext.TenantInfo.IsActive)
                    {
                        context.Items.Add(Constants.HttpContextTenantKey, shellContextAccessor.ShellContext);
                    }
                }
            }

            if (next != null)
            {
                await next(context);
            }
        }
    }
}