using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TNTPlus.Interfaces
{
    public interface IRequestHandler
    {
        string Endpoint { get; } // Например, "/addmoney"
        Task<object> Handle(HttpListenerRequest request); // Метод обработки запроса
    }
}
