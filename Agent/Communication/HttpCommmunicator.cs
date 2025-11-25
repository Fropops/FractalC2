using Agent.Models;
using Agent.Service;
using BinarySerializer;
using Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Agent.Communication
{
    internal class HttpCommmunicator : EgressCommunicator
    {
        private HttpClient _client;
        public HttpCommmunicator(ConnexionUrl conn) : base(conn)
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = new
            RemoteCertificateValidationCallback
            (
               delegate
               {
                   return true;
               }
            );

            _client = new HttpClient();
            _client.Timeout = new TimeSpan(0, 0, 10);
            //_client.BaseAddress = new Uri($"https://{this.ConnectAddress}:{this.ConnectPort
            _client.BaseAddress = new Uri($"{conn}");
            //Console.WriteLine(_client.BaseAddress);
            _client.DefaultRequestHeaders.Clear();

        }


        public override void Init(Agent agent)
        {
            base.Init(agent);
            _client.DefaultRequestHeaders.Add("Authorization", this.Agent.MetaData.Id);
            this.Agent.SendMetaData().Wait();
        }

        bool lastCallError = true;
        protected override async Task<List<NetFrame>> CheckIn(List<NetFrame> results)
        {
            try
            {
                var data = await results.BinarySerializeAsync();
                string b64data = Convert.ToBase64String(data);
                var content = new StringContent(b64data);

                var response = await _client.PostAsync($"/", content);

                if (!response.IsSuccessStatusCode)
                {
#if DEBUG
                    Debug.WriteLine($"Response error : {response.StatusCode} (Encryption Key can be invalid).");
                    Debug.WriteLine($"Error {response.StatusCode}:{await response.Content.ReadAsStringAsync()}");
#endif
                    lastCallError = true;
                    return new List<NetFrame>();
                }

                if (lastCallError)
                {
                    await this.Agent.SendMetaData();
                    await this.Agent.SendRelays();
                }

                lastCallError = false;

                var responseContent = await response.Content.ReadAsStringAsync();

                var b64resp = Convert.FromBase64String(responseContent);
                return await b64resp.BinaryDeserializeAsync<List<NetFrame>>();
            }
            catch (Exception ex)
            {
                lastCallError = true;
                throw ex;
            }
        }


    }
}
