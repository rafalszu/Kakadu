using System.Collections.Generic;
using System.Net.Http.Headers;

namespace SV.ActionApi.Extensions
{
    public static class HttpHeadersExtensions
    {
        public static Dictionary<string, string> AsDictionary(this HttpHeaders headers)
        {
            if(headers == null)
                return null;

            var results = new Dictionary<string, string>();
            foreach(var entry in headers)
            {
                if(entry.Value == null)
                    continue;
                    
                // skip transfer-encoding on purpose
                if(entry.Key.Equals("Transfer-Encoding", System.StringComparison.InvariantCultureIgnoreCase))
                    continue;

                results.Add(entry.Key, string.Join(", ", entry.Value));
            }

            return results;
        }
    }
}