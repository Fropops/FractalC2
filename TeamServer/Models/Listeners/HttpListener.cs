using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using TeamServer.Services;

namespace TeamServer.Models
{
    public class HttpListener : Listener
    {
        public static Dictionary<int, List<HttpListener>> ListenersByPorts = new Dictionary<int, List<HttpListener>>();

        public override string Protocol => this.Secured ? "https" : "http";

        public HttpListener(string name, int bindPort, string ip, bool secured = true) : base(name, bindPort, ip)
        {
            Secured = secured;
        }

        public HttpListener(string id, string name, int bindPort, string ip, bool secured = true) : base(name, bindPort, ip)
        {
            Secured = secured;
        }

        private CancellationTokenSource _tokenSource;

        public override async Task Start()
        {
            var port = this.BindPort;
            bool shouldStart = false;
            if (ListenersByPorts.ContainsKey(port))
            {
                var list = ListenersByPorts[port];
                if (list.Count == 0)
                    shouldStart = true;
                list.Add(this);

            }
            else
            {
                ListenersByPorts.Add(port, new List<HttpListener>() { this });
                shouldStart = true;
            }

            if (!shouldStart)
                return;

            var hostBuilder = new HostBuilder()
                .ConfigureWebHostDefaults(host =>
                {
                    host.UseUrls($"http://*:{BindPort}");
                    host.Configure(ConfigureApp);
                    host.ConfigureServices(ConfigureServices);

                    host.UseKestrel(options =>
                    {
                        options.Listen(IPAddress.Any, BindPort, listenOptions =>
                        {
                            if (this.Secured)
                            {
                                //listenOptions.UseHttps("sslcert.pfx");
                                listenOptions.UseHttps("certs/ts.pfx", "teamserver");
                            }
                        });
                        options.Limits.MinRequestBodyDataRate =
                         new MinDataRate(bytesPerSecond: 100, gracePeriod: TimeSpan.FromSeconds(20));
                    });
                });
            var host = hostBuilder.Build();


            _tokenSource = new CancellationTokenSource();
            host.RunAsync(_tokenSource.Token);
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers(); //adds all controllers
            //service.AddControllers(mvcOptions =>
            //{
            //    mvcOptions.Filters.Add<HttpListenerActionFilter>();
            //    mvcOptions.
            //});
            services.AddSingleton(this._listenerService);
            services.AddSingleton(this._agentService);
            services.AddSingleton(this._fileService);
            services.AddSingleton(this._binMakerService);
            services.AddSingleton(this._changeTrackingService);
            services.AddSingleton(this._webHostService);
            services.AddSingleton(this._cryptoService);
            services.AddSingleton(this._auditService);
            services.AddSingleton(this._resultService);
            services.AddSingleton(this._frameService);
            services.AddSingleton(this._serverService);
            services.AddSingleton(this._rportfwdService);
            services.AddSingleton(this._downloadFileService);
        }

        private void ConfigureApp(IApplicationBuilder app)
        {
            app.UseRouting();

            app.UseEndpoints(e =>
            {
                //e.MapControllerRoute("ci", "/{*url}", new { Controller = "HttpListener", Action = "HandleImplant" });
                //e.MapControllerRoute("wh", "/wh/{id}", new { Controller = "HttpListener", Action = "WebHost" });
                e.MapControllerRoute("request", "/{*relativeUrl}", new { Controller = "HttpListener", Action = "HandleRequest" });
            });
        }

        public override void Stop()
        {
            var port = this.BindPort;

            var list = ListenersByPorts[port];
            list.Remove(this);


            if (list.Count == 0)
                _tokenSource.Cancel();
        }
    }
}
