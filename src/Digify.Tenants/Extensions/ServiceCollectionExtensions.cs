using System;
using System.Linq;
using Digify.Tenants;
using Digify.Tenants.Implementations;
using Microsoft.AspNetCore.Http;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <returns>An new instance of MultiTenantBuilder.</returns>
        public static TenantBuilder<T> AddMultiTenant<T>(this IServiceCollection services, Action<MultiTenantOptions> config)
            where T : class, ITenant, new()
        {
            services.AddScoped<IShellContentAccessor<T>, ShellContentAccessor<T>>();
            services.AddScoped<IShellManager<T>, ShellManager<T>>();

            services.AddScoped<IShellContext<T>, ShellContext<T>>();
            
            services.AddScoped<T>(sp => sp.GetRequiredService<IHttpContextAccessor>().HttpContext?.GetShellContext<T>()?.TenantInfo);
            services.AddScoped<ITenant>(sp => sp.GetService<T>());
            
            services.Configure<MultiTenantOptions>(config);
            
            return new TenantBuilder<T>(services);
        }

        public static TenantBuilder<T> AddMultiTenant<T>(this IServiceCollection services)
            where T : class, ITenant, new()
        {
            return services.AddMultiTenant<T>(_ => { });
        }

        public static bool DecorateService<TService, TImpl>(this IServiceCollection services, params object[] parameters)
        {
            var existingService = services.SingleOrDefault(s => s.ServiceType == typeof(TService));
            if (existingService == null)
                return false;

            var newService = new ServiceDescriptor(existingService.ServiceType,
                                           sp =>
                                           {
                                               TService inner = (TService)ActivatorUtilities.CreateInstance(sp, existingService.ImplementationType);

                                               var parameters2 = new object[parameters.Length + 1];
                                               Array.Copy(parameters, 0, parameters2, 1, parameters.Length);
                                               parameters2[0] = inner;

                                               return ActivatorUtilities.CreateInstance<TImpl>(sp, parameters2);
                                           },
                                           existingService.Lifetime);

            if (existingService.ImplementationInstance != null)
            {
                newService = new ServiceDescriptor(existingService.ServiceType,
                                           sp =>
                                           {
                                               TService inner = (TService)existingService.ImplementationInstance;
                                               return ActivatorUtilities.CreateInstance<TImpl>(sp, inner, parameters);
                                           },
                                           existingService.Lifetime);
            }
            else if (existingService.ImplementationFactory != null)
            {
                newService = new ServiceDescriptor(existingService.ServiceType,
                                           sp =>
                                           {
                                               TService inner = (TService)existingService.ImplementationFactory(sp);
                                               return ActivatorUtilities.CreateInstance<TImpl>(sp, inner, parameters);
                                           },
                                           existingService.Lifetime);
            }

            services.Remove(existingService);
            services.Add(newService);

            return true;
        }
    }
}