using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Linq;

namespace SV.ActionApi.Extensions
{
    public static class StringExtensions
    {
        /// Removes all characters that are not letters or digits
        public static string Sanitize(this string s)
        {
            if(string.IsNullOrWhiteSpace(s))
                return s;

            string result = new string((from c in s where char.IsLetterOrDigit(c) select c).ToArray());
            return result;
        }
    }
}