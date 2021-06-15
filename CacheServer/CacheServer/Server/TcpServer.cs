using CacheServer.Data;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace CacheServer.Server
{
    class TcpServer : ITcpServer
    {
        private readonly TcpListener server;
        private readonly Thread serverThread;
        private readonly CancellationTokenSource cancellationToken;
        private readonly Database database;
        public TcpServer(string ipAddress, int port, CancellationTokenSource cancellationTokenSource, Database database)
        {
            server = new TcpListener(IPAddress.Parse(ipAddress), port);
            this.database = database;
            cancellationToken = cancellationTokenSource;
            serverThread = new Thread(async () => await Listener());
        }
        public void Start()
        {
            server.Start();
            serverThread.Start();
        }

        public void Stop()
        {
            cancellationToken.Cancel();
            server.Stop();
            cancellationToken.Dispose();
        }

        private async Task Listener() //Main loop of TCP server. After new connection Client is move to another thread.
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var client = await server.AcceptTcpClientAsync();
                _ = ClientHandler(client);
            }
        }

        private async Task ClientHandler(TcpClient client)
        {
            NetworkStream stream = null;
            try
            {
                stream = client.GetStream();
                while (!cancellationToken.IsCancellationRequested)
                {
                    //Reading header -> when value is diffrent than "get" or "set" the server disconnects.
                    string header = await TcpHelper.ReadExactlySizeFromStream(stream, 4, cancellationToken);
                    if (header.Equals("get "))
                    {
                        //Reading key and send responde
                        var key = await TcpHelper.ReadUntil(stream, '\n', cancellationToken);
                        string answer = database.Get(key.Remove(key.Length - 2));

                        if (answer == null)
                        {
                            await TcpHelper.SendMessage(stream, "MISSING\r\n", cancellationToken);
                        }
                        else
                        {
                            await TcpHelper.SendMessage(stream, $"OK {answer.Length-2}\r\n", cancellationToken);
                            await TcpHelper.SendMessage(stream, answer, cancellationToken);
                        }
                    }
                    else if (header.Equals("set "))
                    {
                        //reading key and length of value
                        var key = await TcpHelper.ReadUntil(stream, ' ', cancellationToken);
                        var value_size = await TcpHelper.ReadUntil(stream, '\n', cancellationToken);

                        //reading value of key.
                        var value = await TcpHelper.ReadExactlySizeFromStream(stream, int.Parse(value_size.Remove(value_size.Length - 2, 2)) + 2, cancellationToken);
                        database.Set(key, value);
                        //sending response
                        await TcpHelper.SendMessage(stream, "OK\r\n", cancellationToken);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                stream?.Close();
                client?.Close();
            }
        }
    }
}