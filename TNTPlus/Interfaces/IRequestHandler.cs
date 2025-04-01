using System.Net;
using System.Threading.Tasks;

namespace TNTPlus.Interfaces
{
    public interface IRequestHandler
    {
        string Endpoint { get; }
        Task<object> Handle(HttpListenerRequest request);
    }
}
