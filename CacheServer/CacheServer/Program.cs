using System;
using System.Threading;
using CacheServer.Data;
using CacheServer.Server;

namespace CacheServer
{
    class Program
    {

        static void Main(string[] args)
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            Database database = new Database();
            TcpServer server = new TcpServer("127.0.0.1", 10011, cancellationTokenSource, database);
            server.Start();
            Console.ReadLine();
            server.Stop();
            Console.ReadLine();

        }
    }
}