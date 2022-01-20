using CardKartShared.Util;
using Ceen;
using Ceen.Httpd;
using Ceen.Httpd.Handler;
using Ceen.Httpd.Logging;
using Ceen.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace CardKartServer.Vitaliks
{
    public class TimeOfDayHandler : IHttpModule
    {
        public async Task<bool> HandleAsync(IHttpContext context)
        {
            var request = context.Request;
            var response = context.Response;

            response.SetNonCacheable();
            await response.WriteAllJsonAsync(JsonConvert.SerializeObject(new { time = DateTime.Now.TimeOfDay }));
            return true;
        }
    }

    [Name("api")]
    public interface IAPI : IControllerPrefix { }

    [Name("v1")]
    public interface IApiV1 : IAPI { }

    [Name("entry")]
    public class ApiExampleController : Controller, IApiV1
    {
        [HttpGet]
        public IResult Index(IHttpContext context)
        {
            return OK;
        }

        public IResult Index(int id)
        {
            return Json(new { name = "asdo", tokenID = id, image_url = "https://i.kym-cdn.com/photos/images/newsfeed/000/782/935/ce2.png"});
        }
    }

    internal class TokenMetadataEndpoint
    {
        public static void XD()
        {
            var tcs = new CancellationTokenSource();
            var config = new ServerConfig()
                .AddLogger(new CLFStdOut())
                .AddRoute(
                    typeof(ApiExampleController)
                    .Assembly
                    .ToRoute(new ControllerRouterConfig(typeof(ApiExampleController))
                )
            );

            var certPem = File.ReadAllText("/etc/letsencrypt/live/78-138-17-232.cloud-xip.io/cert.pem");
            var eccPem = File.ReadAllText("/etc/letsencrypt/live/78-138-17-232.cloud-xip.io/privkey.pem");
            var cert = X509Certificate2.CreateFromPem(certPem, eccPem);
            config.SSLCertificate = cert;
            
            var task = HttpServer.ListenAsync(
                new IPEndPoint(IPAddress.Any, 443),
                true,
                config,
                tcs.Token
            );

            Console.WriteLine("Serving files, press enter to stop ...");
            Console.ReadLine();

            tcs.Cancel(); // Request stop
            task.Wait();  // Wait for shutdown
        }
    }
}
