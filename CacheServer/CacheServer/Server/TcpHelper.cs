using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CacheServer.Server
{
    public static class TcpHelper
    {
        internal static async Task<string> ReadUntil(NetworkStream stream, char bound, CancellationTokenSource cancellationTokenSource)
        {
            var message = new StringBuilder();
            var data = new byte[1];
            var currentRead = 0;
            char prev = ' ';
            bool loopNotEnded = true;
            while((currentRead = await stream.ReadAsync(data,0,1,cancellationTokenSource.Token)) != 0 && loopNotEnded)
            {
                var c = Convert.ToChar(data[0]);
                if (bound == ' ' && c == ' ') break;
                message.Append(c);
                if (bound == '\n' && c == '\n' && prev == '\r') break;
                prev = c;
            }
            if (currentRead == 0)
                throw new Exception("Communication closed.");
            return message.ToString();
        }

        internal static async Task<string> ReadExactlySizeFromStream(NetworkStream stream, int value_size, CancellationTokenSource cancellationTokenSource)
        {
            var data = new byte[value_size];
            var loadedFromBegin = 0;
            while(loadedFromBegin != value_size)
            {
                if ((loadedFromBegin += await stream.ReadAsync(data, loadedFromBegin, value_size, cancellationTokenSource.Token)) == 0)
                    throw new Exception("Communication closed.");
            }

            return Encoding.UTF8.GetString(data, 0, loadedFromBegin);
        }

        internal static async Task SendMessage(NetworkStream stream, string message, CancellationTokenSource cancellationTokenSource)
        {
            var data = Encoding.UTF8.GetBytes(message);
            await stream.WriteAsync(data, 0, data.Length, cancellationTokenSource.Token);
        }
    }
}
