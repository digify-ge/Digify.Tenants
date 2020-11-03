using System;
using System.Net;

namespace Digify.Tenants
{
    public class MultiTenantException : Exception
    {
        public HttpStatusCode StatusCode { get; set; }
        public MultiTenantException(string message, Exception innerException = null)
            : base(message, innerException) { }
        public MultiTenantException(HttpStatusCode statusCode, string message, Exception innerException = null)
            : base(message, innerException) {
            StatusCode = statusCode;
        }
    }
}