using System;
using System.IO;
using System.Threading.Tasks;
using Grpc.Core;
using Shared;

public class MyService : Svc.SvcBase
{
    public override Task<CalculateReply> Calculate(CalculateRequest request, ServerCallContext context)
    {
        long result = -1;
        switch (request.Op)
        {
            case "+":
                result = request.X + request.Y;
                break;
            case "-":
                result = request.X - request.Y;
                break;
            case "*":
                result = request.X * request.Y;
                break;
            case "/":
                if (request.Y != 0)
                {
                    result = (long)request.X / request.Y;
                }
                break;
            default:
                break;
        }
        return Task.FromResult(new CalculateReply { Result = result });
    }

    static void Main(string[] args)
    {
        int port = int.Parse(args[0]);
        var pair = new KeyCertificatePair(
            File.ReadAllText("cert/service.pem"),
            File.ReadAllText("cert/service-key.pem")
            );
        var creds = new SslServerCredentials(new[] { pair });
        var server = new Server
        {
            Services = { Svc.BindService(new MyService()) },
            Ports = { new ServerPort("0.0.0.0", port, creds) }
        };
        server.Start();
        Console.WriteLine($"Server listening at port {port}. Press any key to terminate");
        Console.ReadKey();
    }
}
