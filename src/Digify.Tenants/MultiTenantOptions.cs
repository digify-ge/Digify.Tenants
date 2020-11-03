using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Digify.Tenants
{
    public class MultiTenantOptions
    {
        public IList<string> IgnoredIdentifiers = new List<string>();
        public Func<object, HttpContext, Task> OnExecutingTenant;
    }
}