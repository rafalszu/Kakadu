using System.Threading.Tasks;

namespace Kakadu.ActionApi.Interfaces
{
    public interface IRequestProcessor
    {
        Task ProcessRequest(string serviceCode);
    }
}