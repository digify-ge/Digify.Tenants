using Microsoft.Extensions.Options;

namespace Digify.Tenants.Options
{
    internal class MultiTenantOptionsManager<TOptions> : IOptions<TOptions>, IOptionsSnapshot<TOptions> where TOptions : class, new()
    {
        private readonly IOptionsFactory<TOptions> _factory;
        private readonly IOptionsMonitorCache<TOptions> _cache; 

        public MultiTenantOptionsManager(IOptionsFactory<TOptions> factory, IOptionsMonitorCache<TOptions> cache)
        {
            _factory = factory;
            _cache = cache;
        }

        public TOptions Value
        {
            get
            {
                return Get(Microsoft.Extensions.Options.Options.DefaultName);
            }
        }

        public virtual TOptions Get(string name)
        {
            name = name ?? Microsoft.Extensions.Options.Options.DefaultName;

            return _cache.GetOrAdd(name, () => _factory.Create(name));
        }

        public void Reset()
        {
            _cache.Clear();
        }
    }
}