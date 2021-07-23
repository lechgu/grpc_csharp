using System;
using Grpc.Core;
using Shared;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            string host = args[0];
            int port = int.Parse(args[1]);
            long x = long.Parse(args[2]);
            string op = args[3];
            long y = long.Parse(args[4]);

            var channel = new Channel(
                host,
                port,
                ChannelCredentials.Insecure
                );
            var client = new Svc.SvcClient(channel);
            var reply = client.Calculate(new CalculateRequest
            {
                X = x,
                Y = y,
                Op = op
            });
            Console.WriteLine($"The calculated result is: {reply.Result}");
        }
    }
}
