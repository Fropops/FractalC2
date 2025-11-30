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
            this.LoadFromDB(app);
        }

        private void LoadFromDB(IApplicationBuilder app)
        {
            var assembly = Assembly.GetExecutingAssembly();

            var storableInterfaceTypes = assembly.GetTypes()
                .Where(t => t.IsInterface &&
                           typeof(IStorable).IsAssignableFrom(t) &&
                           t != typeof(IStorable));

            foreach(var storableInterface in storableInterfaceTypes)
            {
                var service = app.ApplicationServices.GetService(storableInterface) as IStorable;
                if (service != null)
                    service.LoadFromDB();
            }
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
    }
}
