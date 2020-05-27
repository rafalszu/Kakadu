using System.Collections.Generic;
using Kakadu.Core.Models;

namespace Kakadu.Core.Interfaces
{
    public interface IServiceService
    {
        ServiceModel Get(string serviceCode);

        List<ServiceModel> GetAll();

        ServiceModel Create(ServiceModel model);

        ServiceModel Update(ServiceModel model);

        ServiceModel AddKnownRoutes(string serviceCode, IEnumerable<KnownRouteModel> knownRoutes);

        bool Delete(string serviceCode);
    }
}