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

        bool Delete(string serviceCode);
    }
}