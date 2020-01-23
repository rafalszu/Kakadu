using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace SV.ActionApi.Extensions
{
    public static class HttpRequestMessageExtensions
    {
        public static string GetActionHeaderValue(this HttpRequestMessage msg)
        {
            if(msg == null)
                return string.Empty;

            if(msg.Method != HttpMethod.Post)
                return string.Empty;

            IEnumerable<string> actionValues = null;

            if(!(msg.Headers?.TryGetValues("SOAPAction", out actionValues) ?? false))
                return string.Empty;

            if(!(actionValues?.Any() ?? false))
                return string.Empty;

            var actionValue = actionValues.FirstOrDefault();
            if(string.IsNullOrWhiteSpace(actionValue))
                return string.Empty;

            string action = actionValue.Split("/", StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
            if(string.IsNullOrWhiteSpace(action))
                return string.Empty;

            return action.Sanitize();
        }
    }
}