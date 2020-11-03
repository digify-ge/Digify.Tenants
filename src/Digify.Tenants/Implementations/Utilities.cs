using System;
using Microsoft.Extensions.Logging;

namespace Digify.Tenants
{
    internal class Utilities
    {
        internal static void TryLoginfo(ILogger logger, string message)
        {
            if (logger != null)
            {
                logger.LogInformation(message);
            }
        }

        internal static void TryLogDebug(ILogger logger, string message)
        {
            if (logger != null)
            {
                logger.LogDebug(message);
            }
        }

        internal static void TryLogError(ILogger logger, string message, Exception e)
        {
            if (logger != null)
            {
                logger.LogError(e, message);
            }
        }
    }
}