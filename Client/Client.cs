using System;
using System.IO;
using System.Threading.Tasks;
using Grpc.Core;
using Shared;

namespace Client
{
    class Program
    {
        static void DoCalculator(Svc.SvcClient client, String[] args)
        {
            long x = long.Parse(args[2]);
            string op = args[3];
            long y = long.Parse(args[4]);
            var reply = client.Calculate(new CalculateRequest
            {
                X = x,
                Y = y,
                Op = op
            });
            Console.WriteLine($"The calculated result is: {reply.Result}");
        }
        static async Task DoTimeSeries(Svc.SvcClient client, String[] args)
        {
            Console.WriteLine("doing time series");
            using var duplex = client.Median();
            var responseTask = Task.Run(async () =>
            {
                while (await duplex.ResponseStream.MoveNext())
                {
                    var resp = duplex.ResponseStream.Current;
                    Console.WriteLine($"{resp.Timestamp}: {resp.Value}");
                }
            });
            int ts = 1;
            double temp = 10.0;
            var rnd = new Random();
            while (true)
            {
                await duplex.RequestStream.WriteAsync(new Temperature { Timestamp = ts, Value = temp });
                ts += 1;
                temp += rnd.NextDouble() - 0.5;
            }
        }

        static async Task Main(string[] args)
        {
            string host = args[0];
            int port = int.Parse(args[1]);

            var creds = new SslCredentials(
                File.ReadAllText("cert/ca.pem")
            );
            var channel = new Channel(
                host,
                port,
                creds
                );
            var client = new Svc.SvcClient(channel);
            if (args.Length == 2)
            {
                await DoTimeSeries(client, args);
            }
            else
            {
                DoCalculator(client, args);
            }
        }
    }
}
