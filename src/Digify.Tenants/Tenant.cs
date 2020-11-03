using Digify.Tenants.Implementations;
using System.Collections.Generic;

namespace Digify.Tenants
{
    public class Tenant : ITenant
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public string Identifier { get; set; }
        public string Authority { get; set; }

        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string ConnectionString { get; set; }
        public string Domain { get; set; }
        public bool IsActive { get; set; }
        public string ChallengeScheme { get; set; }

        public Dictionary<string, object> Items { get; set; } = new Dictionary<string, object>();
    }
}