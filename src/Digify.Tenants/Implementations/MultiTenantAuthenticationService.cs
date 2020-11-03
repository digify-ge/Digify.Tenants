using System.Security.Claims;
using System.Threading.Tasks;
using Digify.Tenants.Implementations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace Digify.Tenants.AspNetCore
{
    internal class MultiTenantAuthenticationService<TTenantInfo> : IAuthenticationService
        where TTenantInfo : class, ITenant, new()
    {
        private readonly IAuthenticationService inner;
        private readonly IShellContentAccessor<TTenantInfo> shellContentAccessor;

        public MultiTenantAuthenticationService(IAuthenticationService inner, IShellContentAccessor<TTenantInfo> shellContentAccessor)
        {
            this.inner = inner ?? throw new System.ArgumentNullException(nameof(inner));
            this.shellContentAccessor = shellContentAccessor;
        }

        private void AddTenantIdentiferToProperties(HttpContext context, ref AuthenticationProperties properties)
        {
            var shellContext = shellContentAccessor.ShellContext;
            if (shellContext?.TenantInfo != null)
            {
                properties = properties ?? new AuthenticationProperties();
                if(!properties.Items.Keys.Contains(Constants.TenantToken))
                    properties.Items.Add(Constants.TenantToken, shellContext.TenantInfo.Identifier);
            }
        }

        public Task<AuthenticateResult> AuthenticateAsync(HttpContext context, string scheme)
            => inner.AuthenticateAsync(context, scheme);

        public async Task ChallengeAsync(HttpContext context, string scheme, AuthenticationProperties properties)
        {
            AddTenantIdentiferToProperties(context, ref properties);
            await inner.ChallengeAsync(context, scheme, properties);
        }

        public async Task ForbidAsync(HttpContext context, string scheme, AuthenticationProperties properties)
        {
            AddTenantIdentiferToProperties(context, ref properties);
            await inner.ForbidAsync(context, scheme, properties);
        }

        public async Task SignInAsync(HttpContext context, string scheme, ClaimsPrincipal principal, AuthenticationProperties properties)
        {
            AddTenantIdentiferToProperties(context, ref properties);
            await inner.SignInAsync(context, scheme, principal, properties);
        }

        public async Task SignOutAsync(HttpContext context, string scheme, AuthenticationProperties properties)
        {
            AddTenantIdentiferToProperties(context, ref properties);
            await inner.SignOutAsync(context, scheme, properties);
        }
    }
}
