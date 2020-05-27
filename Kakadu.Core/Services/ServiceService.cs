using System;
using System.Collections.Generic;
using Kakadu.Core.Interfaces;
using Kakadu.Core.Models;
using LiteDB;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kakadu.Core.Services
{
    public class ServiceService : IServiceService
    {
        private readonly ILogger<ServiceService> _logger;
        private readonly LiteRepository _instance;

        public ServiceService(ILogger<ServiceService> logger, LiteRepository instance)
        {
            _logger = logger;
            _instance = instance;
        }

        public ServiceModel Create(ServiceModel model)
        {
            if(model == null)
                throw new ArgumentNullException(nameof(model));

            _instance.Insert<ServiceModel>(model);
            return model;
        }

        public ServiceModel Get(string serviceCode)
        {
            if(string.IsNullOrWhiteSpace(serviceCode))
                throw new ArgumentNullException(nameof(serviceCode));

            return _instance.Query<ServiceModel>()
                                              .Include(x => x.KnownRoutes)
                                              .Where(x => x.Code.Equals(serviceCode, StringComparison.InvariantCultureIgnoreCase))
                                              .FirstOrDefault();
        }

        public List<ServiceModel> GetAll()
        {
            return _instance.Fetch<ServiceModel>();
        }

        public ServiceModel Update(ServiceModel model)
        {
            if(model == null)
                throw new ArgumentNullException(nameof(model));

            if(_instance.Update<ServiceModel>(model))
                return model;
            
            return null;
        }

        public ServiceModel AddKnownRoutes(string serviceCode, IEnumerable<KnownRouteModel> knownRoutes)
        {
            if (string.IsNullOrWhiteSpace(serviceCode))
                throw new ArgumentNullException(nameof(serviceCode));
            if (knownRoutes == null)
                throw new ArgumentNullException(nameof(knownRoutes));

            var service = Get(serviceCode);
            if (service == null)
                throw new Exception($"No service found by code '{serviceCode}'");

            service.KnownRoutes ??= new List<KnownRouteModel>();
            service.KnownRoutes.AddRange(knownRoutes);

            return Update(service);
        }

        public bool Delete(string serviceCode)
        {
            int count = _instance.Delete<ServiceModel>(x => x.Code.Equals(serviceCode, StringComparison.InvariantCultureIgnoreCase));
            return count > 0;
        }
    }
}