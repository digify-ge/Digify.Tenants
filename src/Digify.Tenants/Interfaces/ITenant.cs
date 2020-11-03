using System.Collections.Generic;

namespace Digify.Tenants
{
    public interface ITenant
    {
        string Id { get; set; }
        public string Name { get; set; }

        string Identifier { get; set; }
        string Authority { get; set; }

        string ClientId { get; set; }
        string ClientSecret { get; set; }
        string ConnectionString { get; set; }
        string Domain { get; set; }
        bool IsActive { get; set; }
        string ChallengeScheme { get; set; }

        Dictionary<string, object> Items { get; set; }
    }
}