using System.Collections.Generic;

namespace Healthcheck.Middleware.AspNetCore
{
    public static class DictionaryExtensions
    {
        public static void AddIfMissing(this IDictionary<string, object> dictionary, string key, object value)
        {
            if (!dictionary.ContainsKey(key))
            {
                dictionary[key] = value;
            }
        }
    }
}