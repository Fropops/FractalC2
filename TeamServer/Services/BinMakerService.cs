using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TeamServer.Models;

namespace TeamServer.Services;

public interface IBinMakerService
{
    //string GenerateStagersFor(Listener listener);

    string DonutFolder { get; }
    //string ReaNimatorFolder { get; }

    void GenerateB64s();
    //string GenerateBins(Listener listener);


    //string GeneratedAgentBinFileName { get; }
    //string GeneratedAgentExeFileName { get; }

    //string GeneratedAgentDllFileName { get; }

    string SourcePath { get; }
}

public class BinMakerService : IBinMakerService
{
    private readonly IFileService _fileService;
    private readonly IConfiguration _configuration;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger _logger;

    public const string BinExtension = ".bin";
    public const string Base64Extension = ".b64";
    public const string X86Suffix = "-x86";
    public const string AgentFileName = "Agent.exe";
    public const string Stage1FileName = "Stage1.dll";

    public BinMakerService(IFileService fileService, IConfiguration configuration, ILoggerFactory loggerFactory)
    {
        _fileService = fileService;
        _configuration = configuration;
        _loggerFactory = loggerFactory;

        _logger = _loggerFactory.CreateLogger("BinMaker");
    }

    public string DonutFolder => _configuration.GetValue<string>("DonutFolder");
    //public string ReaNimatorFolder => _configuration.GetValue<string>("ReaNimatorFolder");

    public string SourcePath => "Source/";

    private string GetX86FileName(string fileName)
    {
        return Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileNameWithoutExtension(fileName) + X86Suffix + Path.GetExtension(fileName));
    }

    //public string GenerateStagersFor(Listener listener)
    //{
    //    string gen = this.GenerateExeStager(listener);
    //    gen += this.GenerateDllStager(listener);
    //    return gen;
    //}

    /*public string GenerateBins(Listener listener)
    {
        _logger.LogInformation($"Generate Bin files !");
        var res = GenerateBin(listener, true);
        res += GenerateBin(listener);
        return res;
    }

    public string GenerateBin(bool x86 = false)
    {
        var cmd = Path.Combine(DonutFolder, "donut");
        var inputFile = Path.Combine(this.SourcePath, AgentFileName);
        if (x86)
            inputFile = GetX86FileName(inputFile);
        var outFile = this._fileService.GetWebHostPath(Path.GetFileNameWithoutExtension(inputFile) + BinExtension);

        if (!Directory.Exists(Path.GetDirectoryName(outFile)))
            Directory.CreateDirectory(Path.GetDirectoryName(outFile));

        List<string> args = new List<string>();
        args.Add(inputFile);
        args.Add("-f 1");
        if(x86)
            args.Add("-a 1");
        else
            args.Add("-a 2");
        args.Add($"-o");
        args.Add(outFile);
        args.Add($"-p");
        args.Add($"{listener.Protocol}:{listener.Ip}:{listener.BindPort}");

        //string args = $"'{inputFile}' -f 1 -a 2 -o '{outFile}' -p '{listener.Ip}:{listener.BindPort}'";
        _logger.LogInformation($"Executing {cmd} {string.Join(' ', args)}");
        var ret = ExecuteCommand(cmd, args, this.DonutFolder);
        return ret;
    }*/


    public void GenerateB64s()
    {
        _logger.LogInformation($"Generate base64 files !");
        GenerateB64(AgentFileName, true);
        GenerateB64(AgentFileName);
        GenerateB64(Stage1FileName, true);
        GenerateB64(Stage1FileName);
    }

    public void GenerateB64(string sourceFile, bool x86 = false)
    {
        var inputFile = Path.Combine(this.SourcePath, sourceFile);
        if (x86)
            inputFile = GetX86FileName(inputFile);

        byte[] bytes = File.ReadAllBytes(inputFile);
        string base64 = Convert.ToBase64String(bytes);
        var outFile = this._fileService.GetWebHostPath(Path.GetFileNameWithoutExtension(inputFile) + Base64Extension);

        if (!Directory.Exists(Path.GetDirectoryName(outFile)))
            Directory.CreateDirectory(Path.GetDirectoryName(outFile));
        _logger.LogInformation($"Encoding base64 :  {inputFile} => {outFile}");

        File.WriteAllText(outFile, base64);

    }

    //public string GenerateExeStager(Listener listener)
    //{
    //    var res = GenerateBin(listener);
    //    res += Environment.NewLine;
    //    res += GenerateExeStagerFromBin(listener);
    //    return res;
    //}

    //public string GenerateDllStager(Listener listener)
    //{
    //    var res = GenerateBin(listener);
    //    res += Environment.NewLine;
    //    res += GenerateDllStagerFromBin(listener);
    //    return res;
    //}
    //public string GenerateExeStagerFromBin(Listener listener)
    //{
    //    var cmd = Path.Combine(ReaNimatorFolder, "reaNimator");
    //    var inputFile = Path.Combine(this._fileService.GetListenerPath(listener.Name), this.GeneratedAgentBinFileName);
    //    var outFile = Path.Combine(this._fileService.GetListenerPath(listener.Name), this.GeneratedAgentExeFileName);

    //    if (!Directory.Exists(this._fileService.GetListenerPath(listener.Name)))
    //        Directory.CreateDirectory(this._fileService.GetListenerPath(listener.Name));

    //    List<string> args = new List<string>();
    //    args.Add("-f");
    //    args.Add(inputFile);
    //    args.Add("-t");
    //    args.Add("raw");
    //    args.Add($"-o");
    //    args.Add(outFile);
    //    args.Add($"-e");
    //    args.Add($"-u");
    //    args.Add($"-p");
    //    args.Add($"explorer.exe");

    //    //reaNimator -f /Share/tmp/Stager/agent.bin -t raw -o /Share/tmp/Stager/TestSelf.exe -e -u -p "explorer.exe"
    //    _logger.LogInformation($"Executing {cmd} {string.Join(' ', args)}");
    //    var ret = ExecuteCommand(cmd, args, this.ReaNimatorFolder);
    //    return ret;
    //}

    //public string GenerateDllStagerFromBin(Listener listener)
    //{
    //    var cmd = Path.Combine(ReaNimatorFolder, "reaNimator");
    //    var inputFile = Path.Combine(this._fileService.GetListenerPath(listener.Name), this.GeneratedAgentBinFileName);
    //    var outFile = Path.Combine(this._fileService.GetListenerPath(listener.Name), this.GeneratedAgentDllFileName);

    //    if (!Directory.Exists(this._fileService.GetListenerPath(listener.Name)))
    //        Directory.CreateDirectory(this._fileService.GetListenerPath(listener.Name));

    //    List<string> args = new List<string>();
    //    args.Add("-f");
    //    args.Add(inputFile);
    //    args.Add("-t");
    //    args.Add("raw");
    //    args.Add($"-o");
    //    args.Add(outFile);
    //    args.Add($"-e");
    //    args.Add($"-u");
    //    args.Add($"-b");
    //    args.Add($"-p");
    //    args.Add($"explorer.exe");

    //    //reaNimator -f /Share/tmp/Stager/agent.bin -t raw -o /Share/tmp/Stager/TestSelf.exe -e -u -p "explorer.exe"
    //    _logger.LogInformation($"Executing {cmd} {string.Join(' ', args)}");
    //    var ret = ExecuteCommand(cmd, args, this.ReaNimatorFolder);
    //    return ret;
    //}

    public string ExecuteCommand(string fileName, List<string> args, string startIn)
    {
        //var startInfo = new ProcessStartInfo()
        //{
        //    FileName = fileName,
        //    Arguments = args,
        //    WorkingDirectory = startIn,
        //    RedirectStandardError = true,
        //    RedirectStandardOutput = true,
        //    UseShellExecute = false,
        //    CreateNoWindow = true,

        //};

        //var process = new Process
        //{
        //    StartInfo = startInfo,
        //};


        Collection<string> collection = new Collection<string>(args);

        var psi =
        new ProcessStartInfo
        {
            FileName = fileName,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            WorkingDirectory = startIn,
        };

        foreach (var arg in args)
            psi.ArgumentList.Add(arg);

        var process = Process.Start(psi);




        string output = string.Empty;
        process.OutputDataReceived += (s, e) => { output += e.Data + Environment.NewLine; };
        process.ErrorDataReceived += (s, e) => { output += e.Data + Environment.NewLine; };

        process.Start();

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        process.WaitForExit();

        return output;
    }
}
