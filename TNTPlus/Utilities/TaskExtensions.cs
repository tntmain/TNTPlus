using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TNTPlus.Utilities
{
    public static class TaskExtensions
    {
        public static Task<HttpListenerContext> WithCancellation(this Task<HttpListenerContext> task, CancellationToken cancellationToken)
        {
            return Task.Run(() => task, cancellationToken);
        }
    }
}
