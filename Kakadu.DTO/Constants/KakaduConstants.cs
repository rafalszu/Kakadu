using System;

namespace Kakadu.DTO.Constants
{
    public static class KakaduConstants
    {
        public const string ACCESS_TOKEN = "KakaduApiAccessToken";
        public const string ACTIONAPI_INSTANCES = "KakaduActionApiInstances";
        private const string RECORD = "{0}|Record";

        public static string GetRecordKey(string serviceCode)
        {
            if(string.IsNullOrWhiteSpace(serviceCode))
                throw new ArgumentNullException();

            return $"{serviceCode}|{KakaduConstants.RECORD}";
        }
    }
}