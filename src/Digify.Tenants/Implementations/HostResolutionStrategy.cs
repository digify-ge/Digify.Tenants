using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Digify.Tenants.Implementations
{
    public class HostResolutionStrategy : ITenantResolutionStrategy
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;


        public HostResolutionStrategy(
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            ILoggerFactory loggerFactory)
        {
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _logger = loggerFactory.CreateLogger<HostResolutionStrategy>();
        }

        public async Task<(string domainName, string ipAddresss, string name)> GetTenantIdentifierAsync()
        {
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext == null)
            {
                return await Task.FromResult((string.Empty, string.Empty, string.Empty));
            }
            else
            {
                var domainName = httpContext.Request.Host.Value;
                var ipAddress = GetIpAddress(httpContext);
                var tenantIdentifier = await GetFromHttpContextAsync(httpContext);
                if (_configuration.GetSection("UsePathToResolveTenant").Value != "true")
                {
                    if (string.IsNullOrEmpty(tenantIdentifier) && httpContext.Request.RouteValues.TryGetValue(Constants.TenantToken, out var tenantIdentifierValue))
                    {
                        tenantIdentifier = tenantIdentifierValue.ToString();
                    }

                    return await Task.FromResult((domainName, ipAddress, tenantIdentifier));
                }
                else
                {
                    return await Task.FromResult((domainName, ipAddress, GetSubDomain(httpContext)));
                }
            }
        }
        private async Task<string> GetFromHttpContextAsync(HttpContext httpContext)
        {
            var schemes = httpContext.RequestServices.GetRequiredService<IAuthenticationSchemeProvider>();

            foreach (var scheme in (await schemes.GetRequestHandlerSchemesAsync()).
                Where(s => typeof(IAuthenticationRequestHandler).IsAssignableFrom(s.HandlerType)))
            {
                var optionsType = scheme.HandlerType.GetProperty("Options").PropertyType;
                var optionsMonitorType = typeof(IOptionsMonitor<>).MakeGenericType(optionsType);
                var optionsMonitor = httpContext.RequestServices.GetRequiredService(optionsMonitorType);
                var options = optionsMonitorType.GetMethod("Get").Invoke(optionsMonitor, new[] { scheme.Name }) as RemoteAuthenticationOptions;

                var callbackPath = (PathString)(optionsType.GetProperty("CallbackPath")?.GetValue(options) ?? PathString.Empty);
                var signedOutCallbackPath = (PathString)(optionsType.GetProperty("SignedOutCallbackPath")?.GetValue(options) ?? PathString.Empty);

                if (callbackPath.HasValue && callbackPath == httpContext.Request.Path ||
                    signedOutCallbackPath.HasValue && signedOutCallbackPath == httpContext.Request.Path)
                {
                    try
                    {
                        string state = null;
                        if (string.Equals(httpContext.Request.Method, "GET", StringComparison.OrdinalIgnoreCase))
                        {
                            state = httpContext.Request.Query["state"];
                        }
                        else if (string.Equals(httpContext.Request.Method, "POST", StringComparison.OrdinalIgnoreCase)
                            && httpContext.Request.HasFormContentType
                            && httpContext.Request.Body.CanRead)
                        {
                            var formOptions = new FormOptions { BufferBody = true, MemoryBufferThreshold = 1048576 };

                            var form = await httpContext.Request.ReadFormAsync(formOptions);
                            state = form.Where(i => i.Key.ToLowerInvariant() == "state").Single().Value;
                        }

                        var properties = ((dynamic)options).StateDataFormat.Unprotect(state) as AuthenticationProperties;

                        if (properties == null)
                        {
                            if (_logger != null)
                                _logger.LogWarning("A tenant could not be determined because no state paraameter passed with the remote authentication callback.");
                            return null;
                        }

                        properties.Items.TryGetValue(Constants.TenantToken, out var identifier);

                        return identifier;
                    }
                    catch (Exception e)
                    {
                        throw new MultiTenantException("Error occurred resolving tenant for remote authentication.", e);
                    }
                }
            }

            return string.Empty;
        }
        private string GetIpAddress(HttpContext httpContext)
        {
            var remoteIpAddress = httpContext.Connection.RemoteIpAddress;
            var ipAddressString = remoteIpAddress.ToString();

            if (remoteIpAddress.IsIPv4MappedToIPv6)
            {
                return remoteIpAddress.MapToIPv6().ToString();
            }
            else
            {
                return remoteIpAddress.MapToIPv4().ToString();
            }
        }

        private string GetSubDomain(HttpContext httpContext)
        {
            var subDomain = string.Empty;

            var host = httpContext.Request.Host.Host;

            if (!string.IsNullOrWhiteSpace(host))
            {
                subDomain = host.Split('.')[0];
            }

            return subDomain.Trim().ToLower();
        }
    }
}
