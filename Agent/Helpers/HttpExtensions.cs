using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Agent.Helpers
{
    public static class HttpExtensions
    {

        public static async Task<HttpListenerContext> AcceptHttpClientAsync(this HttpListener listener, CancellationTokenSource cts)
        {
            using (cts.Token.Register(listener.Stop))
            {
                try
                {
                    var client = await listener.GetContextAsync().ConfigureAwait(false);
                    return client;
                }
                catch (ObjectDisposedException ex)
                {
                    // Token was canceled - swallow the exception and return null
                    if (cts.Token.IsCancellationRequested) return null;
                    throw ex;
                }
            }
        }


        public static async Task ReturnNotFound(this HttpListenerResponse response)
        {
            response.StatusCode = 404;
            response.ContentType = "text/plain";
            byte[] buffer404 = System.Text.Encoding.UTF8.GetBytes("Not Found");
            response.ContentLength64 = buffer404.Length;
            System.IO.Stream output404 = response.OutputStream;
            await output404.WriteAsync(buffer404, 0, buffer404.Length);
            output404.Close();
        }

        public static async Task ReturnFile(this HttpListenerResponse response, byte[] content)
        {
            response.StatusCode = 200;
            response.ContentType = "application/octet-stream";
            byte[] buffer = content;
            response.ContentLength64 = buffer.Length;
            System.IO.Stream output = response.OutputStream;
            await output.WriteAsync(buffer, 0, buffer.Length);
            output.Close();
        }
    }
}
