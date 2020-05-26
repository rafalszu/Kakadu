using System;

namespace Kakadu.DTO.Constants
{
    public static class KakaduConstants
    {
        public const string ACCESS_TOKEN = "KakaduApiAccessToken";
        public const string ACTIONAPI_INSTANCES = "KakaduActionApiInstances";
        public const string SERVICES = "Services";
        private const string RECORD = "Record";
        private const string FOUNDROUTES = "KnownRoutes";
        private const string SERVICE = "Service";
        
        public static string GetRecordKey(string serviceCode)
        {
            if(string.IsNullOrWhiteSpace(serviceCode))
                throw new ArgumentNullException();

            return $"{serviceCode}|{KakaduConstants.RECORD}";
        }

        public static string GetFoundRoutesKey(string serviceCode)
        {
            if(string.IsNullOrWhiteSpace(serviceCode))
                throw new ArgumentNullException(nameof(serviceCode));

            return $"{serviceCode}|{KakaduConstants.FOUNDROUTES}";
        }

        public static string GetServiceKey(string serviceCode)
        {
            if (string.IsNullOrWhiteSpace(serviceCode))
                throw new ArgumentNullException(nameof(serviceCode));

            return $"{serviceCode}|{KakaduConstants.SERVICE}";
        }
    }
}