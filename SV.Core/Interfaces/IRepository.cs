using SV.Core.Models;

namespace SV.Core.Interfaces
{
    public interface IDataProvider
    {
        ServiceModel GetService(string serviceCode);

        ServiceModel CreateService(ServiceModel model);

        ServiceModel UpdateService(ServiceModel model);
    }
}