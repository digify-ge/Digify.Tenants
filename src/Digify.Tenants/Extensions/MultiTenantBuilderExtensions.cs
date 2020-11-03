using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Digify.Tenants;
using Digify.Tenants.AspNetCore;
using Digify.Tenants.Implementations;
using Digify.Tenants.Options;
using Digify.Tenants.Stores;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class TenantBuilderExtension
    {
        public static TenantBuilder<TTenantInfo> WithInMemoryStore<TTenantInfo>(this TenantBuilder<TTenantInfo> builder)
            where TTenantInfo : class, ITenant, new()
            => builder.WithInMemoryStore<TTenantInfo>(_ => { });

        public static TenantBuilder<TTenantInfo> WithInMemoryStore<TTenantInfo>(this TenantBuilder<TTenantInfo> builder,
                                                                                              Action<InMemoryStoreOptions<TTenantInfo>> config)
            where TTenantInfo : class, ITenant, new()
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            builder.Services.Configure<InMemoryStoreOptions<TTenantInfo>>(config);

            return builder.WithStore<InMemoryStore<TTenantInfo>>(ServiceLifetime.Singleton);
        }
        public static TenantBuilder<TTenantInfo> WithResolutionStrategy<TTenantInfo>(this TenantBuilder<TTenantInfo> builder) 
            where TTenantInfo : class, ITenant, new()
        {
            builder.Services.AddHttpContextAccessor();
            builder.Services.Add(ServiceDescriptor.Describe(typeof(ITenantResolutionStrategy), typeof(HostResolutionStrategy), ServiceLifetime.Transient));

            return builder;
        }
        public static TenantBuilder<TTenantInfo> WithAuthentication<TTenantInfo>(this TenantBuilder<TTenantInfo> builder)
           where TTenantInfo : class, ITenant, new()
        {
            builder.Services.ConfigureAll<CookieAuthenticationOptions>(options =>
            {
                var origOnValidatePrincipal = options.Events.OnValidatePrincipal;
                options.Events.OnValidatePrincipal = async context =>
                {
                    await origOnValidatePrincipal(context);

                    if (context.Principal == null)
                        return;

                    var currentTenant = context.HttpContext.GetShellContext<TTenantInfo>()?.TenantInfo?.Identifier;

                    if (currentTenant == null && !context.Principal.Claims.Any(c => c.Type == Constants.TenantToken))
                        return;

                    if (!context.Principal.Claims.Where(c => c.Type == Constants.TenantToken && String.Equals(c.Value, currentTenant, StringComparison.OrdinalIgnoreCase)).Any())
                        context.RejectPrincipal();
                };

                // Set the tenant claim when signing in.
                var origOnSigningIn = options.Events.OnSigningIn;
                options.Events.OnSigningIn = async context =>
                {
                    await origOnSigningIn(context);

                    if (context.Principal == null)
                        return;

                    var identity = (ClaimsIdentity)context.Principal.Identity;
                    var currentTenant = context.HttpContext.GetShellContext<TTenantInfo>()?.TenantInfo?.Identifier;

                    if (currentTenant != null)
                        identity.AddClaim(new Claim(Constants.TenantToken, currentTenant));
                };
            });

            builder.WithPerTenantOptions<CookieAuthenticationOptions>((options, tc) =>
            {
                var d = (dynamic)tc;
                try { options.LoginPath = ((string)d.CookieLoginPath).Replace(Constants.TenantToken, tc.Identifier); } catch { }
                try { options.LogoutPath = ((string)d.CookieLogoutPath).Replace(Constants.TenantToken, tc.Identifier); } catch { }
                try { options.AccessDeniedPath = ((string)d.CookieAccessDeniedPath).Replace(Constants.TenantToken, tc.Identifier); } catch { }
            });

            if (!builder.Services.Where(s => s.ServiceType == typeof(IAuthenticationService)).Any())
                throw new MultiTenantException("WithRemoteAuthenticationCallbackStrategy() must be called after AddAutheorization() in ConfigureServices.");
            builder.Services.DecorateService<IAuthenticationService, MultiTenantAuthenticationService<TTenantInfo>>();

            builder.WithPerTenantOptions<OpenIdConnectOptions>((options, tc) =>
            {
                if (tc is ITenant tenant)
                {
                    try { options.Authority = tenant.Authority; } catch { }
                    try { options.ClientId = tenant.ClientId; } catch { }
                    try { options.ClientSecret = tenant.ClientSecret; } catch { }
                }

            });

            builder.Services.Replace(ServiceDescriptor.Singleton<IAuthenticationSchemeProvider, MultiTenantAuthenticationSchemeProvider>());

            builder.WithPerTenantOptions<AuthenticationOptions>((options, tc) =>
            {
                if (tc is ITenant tenant)
                {
                    options.DefaultChallengeScheme = tenant.ChallengeScheme ?? options.DefaultChallengeScheme;
                }
            });

            return builder;
        }
    }
}