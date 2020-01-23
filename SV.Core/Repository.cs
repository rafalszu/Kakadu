using System;
using System.Collections.Generic;
using LiteDB;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SV.Core.Interfaces;
using SV.Core.Models;

namespace SV.Core
{
    public class Repository : IRepository
    {
        private readonly ILogger<Repository> logger;
        private readonly LiteRepository database;

        public Repository(IOptions<DatabaseSettings> databaseSettings, ILogger<Repository> logger)
        {
            database = new LiteRepository(databaseSettings.Value.ConnectionString);
            this.logger = logger;
        }

        public ServiceModel GetService(string serviceCode)
        {
            if(string.IsNullOrWhiteSpace(serviceCode))
                throw new ArgumentNullException(nameof(serviceCode));

            return database.Query<ServiceModel>()
                           .Include(x => x.KnownRoutes)
                           .Where(x => x.Code.Equals(serviceCode, StringComparison.InvariantCultureIgnoreCase))
                           .FirstOrDefault();
        }

        public ServiceModel CreateService(ServiceModel model)
        {
            if(model == null)
                throw new ArgumentNullException(nameof(model));

            database.Insert<ServiceModel>(model);
            return model;
        }

        public ServiceModel UpdateService(ServiceModel model)
        {
            if(database.Update<ServiceModel>(model))
                return model;
            
            return null;
        }

        // private ServiceModel GetDummyService()
        // {
        //     return new ServiceModel
        //     {
        //         Id = Guid.NewGuid(),
        //         Code = "dummy",
        //         Name = "dummy sample REST",
        //         Address = new Uri("https://jsonplaceholder.typicode.com", UriKind.Absolute),
        //         UnkownRoutesPassthrough = true,
        //         KnownRoutes = new List<KnownRouteModel> 
        //         {
        //             new KnownRouteModel {
        //                 Id = Guid.NewGuid(),
        //                 RelativeUri = new Uri("/posts/1"),
        //                 Method = MethodTypeEnum.GET,
        //                 Replies = new List<KnownRouteReplyModel> {
        //                     new KnownRouteReplyModel {
        //                         StatusCode = 200,
        //                         ContentType = "application/json; charset=utf-8",
        //                         ContentBase64 = "ewogICAgdXNlcklkOiAxLAogICAgaWQ6IDEsCiAgICB0aXRsZTogc3VudCBhdXQgZmFjZXJlIHJlcGVsbGF0IHByb3ZpZGVudCBvY2NhZWNhdGkgZXhjZXB0dXJpIG9wdGlvIHJlcHJlaGVuZGVyaXQsCiAgICBib2R5OiBxdWlhIGV0IHN1c2NpcGl0bnN1c2NpcGl0IHJlY3VzYW5kYWUgY29uc2VxdXVudHVyIGV4cGVkaXRhIGV0IGN1bW5yZXByZWhlbmRlcml0IG1vbGVzdGlhZSB1dCB1dCBxdWFzIHRvdGFtbm5vc3RydW0gcmVydW0gZXN0IGF1dGVtIHN1bnQgcmVtIGV2ZW5pZXQgYXJjaGl0ZWN0bwp9Cg=="
        //                     }
        //                 }
        //             }
        //         }
        //     };
        // }

        // private ServiceModel GetCalculatorService()
        // {
        //     return new ServiceModel
        //     {
        //         Id = Guid.NewGuid(),
        //         Code = "calculator",
        //         Name = "calculator SOAP",
        //         Address = new Uri("http://www.dneonline.com", UriKind.Absolute),
        //         UnkownRoutesPassthrough = true,
        //         KnownRoutes = new List<KnownRouteModel>
        //         {
        //             new KnownRouteModel 
        //             {
        //                 Id = Guid.NewGuid(),
        //                 RelativeUri = new Uri("/calculator.asmx"),
        //                 Method = MethodTypeEnum.POST,
        //                 Action = "Add",
        //                 Replies = new List<KnownRouteReplyModel> {
        //                     new KnownRouteReplyModel {
        //                         StatusCode = 200,
        //                         ContentType = "text/xml; charset=utf-8",
        //                         ContentBase64 = "PHNvYXA6RW52ZWxvcGUgeG1sbnM6c29hcD1odHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy9zb2FwL2VudmVsb3BlLyB4bWxuczp4c2k9aHR0cDovL3d3dy53My5vcmcvMjAwMS9YTUxTY2hlbWEtaW5zdGFuY2UgeG1sbnM6eHNkPWh0dHA6Ly93d3cudzMub3JnLzIwMDEvWE1MU2NoZW1hPgogICA8c29hcDpCb2R5PgogICAgICA8QWRkUmVzcG9uc2UgeG1sbnM9aHR0cDovL3RlbXB1cmkub3JnLz4KICAgICAgICAgPEFkZFJlc3VsdD41PC9BZGRSZXN1bHQ+CiAgICAgIDwvQWRkUmVzcG9uc2U+CiAgIDwvc29hcDpCb2R5Pgo8L3NvYXA6RW52ZWxvcGU+Cg=="
        //                     }
        //                 }
        //             }
        //         }
        //     };
        // }

        
    }
}
