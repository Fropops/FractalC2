using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using WebCommander;
using WebCommander.Services;
using Microsoft.Extensions.DependencyInjection;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddSingleton<AuthService>();
builder.Services.AddSingleton<AgentService>();
builder.Services.AddSingleton<CommandService>();
builder.Services.AddSingleton<TerminalHistoryService>();
builder.Services.AddSingleton<ToastService>();

builder.Services.AddHttpClient<TeamServerClient>();

await builder.Build().RunAsync();
