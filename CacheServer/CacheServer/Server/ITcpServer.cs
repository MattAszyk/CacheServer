using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CacheServer.Server
{
    interface ITcpServer
    {
        void Start();
        void Stop();
    }
}
