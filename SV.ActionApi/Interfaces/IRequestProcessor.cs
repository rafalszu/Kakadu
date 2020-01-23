using System.Threading.Tasks;

namespace SV.ActionApi.Interfaces
{
    public interface IRequestProcessor
    {
        Task ProcessRequest(string serviceCode);
    }
}