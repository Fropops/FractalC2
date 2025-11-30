using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeamServer.Controllers;
using TeamServer.MiddleWare;
using TeamServer.Models;
using TeamServer.Services;
using TeamServer.Ext;
using TeamServer.Helper;
using TeamServer.Service;
using System.Reflection;

namespace TeamServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers();
            //services.AddSingleton<AgentsController>();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "TeamServer", Version = "v1" });
            });

            services.AddCors(options =>
            {
                //options.AddPolicy("AllowBlazor", builder =>
                //{
                //    builder.WithOrigins("http://localhost:5032") // Vérifiez bien que c'est le port de votre Blazor
                //           .AllowAnyMethod()
                //           .AllowAnyHeader();
                //});
                options.AddPolicy("AllowBlazor", builder =>
                {
                    builder
                        .AllowAnyOrigin()   // Autorise toutes les IP / domaines
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            });

            services.AddLogging();


            var discoveredServices = ServiceDiscovery.DiscoverInjectableServices(Assembly.GetExecutingAssembly());

            foreach (var (serviceInterface, implementation) in discoveredServices)
                services.AddSingleton(serviceInterface, implementation);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
           

            app.UseDeveloperExceptionPage();

            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TeamServer v1"));
            }

            app.UseMiddleware<JwtMiddleware>();

            app.UseRouting();

            app.UseCors("AllowBlazor");

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

           


            var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            var factory = app.ApplicationServices.GetService<ILoggerFactory>();
            var logger = factory.CreateLogger("FractalC2 - Team Server");
            logger.LogInformation($"Version {version}");


            //this.StartHttpHost(app);
            this.PopulateUsers(app);
            //this.StartDefaultListener(app);

            this.LoadFromDB(app);
        }

        private void LoadFromDB(IApplicationBuilder app)
        {
            app.ApplicationServices.GetService<IListenerService>().LoadFromDB();
            app.ApplicationServices.GetService<IAgentService>().LoadFromDB();
            app.ApplicationServices.GetService<ITaskService>().LoadFromDB();
            app.ApplicationServices.GetService<ITaskResultService>().LoadFromDB();
            app.ApplicationServices.GetService<IWebHostService>().LoadFromDB();
            app.ApplicationServices.GetService<IDownloadFileService>().LoadFromDB();
            app.ApplicationServices.GetService<IImplantService>().LoadFromDB();
        }


        private IListenerService ls;
        private IFileService fs;

       /* private void StartHttpHost(IApplicationBuilder app)
        {
            ls = app.ApplicationServices.GetService<IListenerService>();
            fs = app.ApplicationServices.GetService<IFileService>();

            var hostBuilder = new HostBuilder()
                .ConfigureWebHostDefaults(host =>
                {
                    host.UseUrls($"http://*:80");
                    host.Configure(ConfigureHttpHostApp);
                    host.ConfigureServices(ConfigureHttpHostServices);
                });
            var host = hostBuilder.Build();
            host.RunAsync();
        }*/


        private void ConfigureHttpHostServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSingleton<IListenerService>(ls);
            services.AddSingleton(fs);
        }

        private void ConfigureHttpHostApp(IApplicationBuilder app)
        {
            app.UseRouting();
            app.UseEndpoints(e =>
            {

                e.MapControllerRoute("/", "/{id}", new { Controller = "HttpHost", Action = "Dload" });
                e.MapControllerRoute("/", "/", new { Controller = "HttpHost", Action = "Index" });
            });
        }

        private void PopulateUsers(IApplicationBuilder app)
        {
            var config = app.ApplicationServices.GetService<IConfiguration>();
            var users = app.ApplicationServices.GetService<IUserService>();

            foreach (var cfgUser in config.GetSection("Users").GetChildren())
            {
                var user = new User();
                user.Id = cfgUser.GetValue<string>("Id");
                user.Key = cfgUser.GetValue<string>("Key");

                users.AddUser(user);
            }

        }

        /*private void StartDefaultListener(IApplicationBuilder app)
        {
            var listenerService = app.ApplicationServices.GetService<IListenerService>();
            var agentService = app.ApplicationServices.GetService<IAgentService>();
            var resultService = app.ApplicationServices.GetService<ITaskResultService>();
            var fileService = app.ApplicationServices.GetService<IFileService>();
            var binMakerService = app.ApplicationServices.GetService<IBinMakerService>();
            var config = app.ApplicationServices.GetService<IConfiguration>();
            var change = app.ApplicationServices.GetService<IChangeTrackingService>();
            var webHost = app.ApplicationServices.GetService<IWebHostService>();
            var crypto = app.ApplicationServices.GetService<ICryptoService>();
            var audit = app.ApplicationServices.GetService<IAuditService>();
            var frame = app.ApplicationServices.GetService<IFrameService>();
            var server = app.ApplicationServices.GetService<IServerService>();
            var rportfwd = app.ApplicationServices.GetService<IReversePortForwardService>();
            var db = app.ApplicationServices.GetService<IDatabaseService>()

            var factory = app.ApplicationServices.GetService<ILoggerFactory>();
            var logger = factory.CreateLogger("Default Listener Start");

            var defaultListenersConfig = config.GetValue<string>("ListenersConfig");
            if(defaultListenersConfig == null)
                return;
            IEnumerable<ListenerConfig> listeners = JsonConvert.DeserializeObject<IEnumerable<ListenerConfig>>(defaultListenersConfig);
            foreach (var listenerConf in listeners)
            {
                var listener = new HttpListener(listenerConf.Name, listenerConf.BindPort, listenerConf.Address, listenerConf.Secured);
                listener.Init(agentService, resultService, fileService, binMakerService, listenerService, logger, change, webHost, crypto, audit, frame, server, rportfwd, db);
                listener.Start();
                listenerService.AddListener(listener);
            }
        }*/
    }
}
