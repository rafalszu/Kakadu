using Kakadu.Core.Models;

namespace Kakadu.Core.Interfaces
{
    public interface IDataProvider
    {
        ServiceModel GetService(string serviceCode);

        ServiceModel CreateService(ServiceModel model);

        ServiceModel UpdateService(ServiceModel model);
    }
}