using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using TeamServer.UI;
using TeamServer.UI.Services;
using Microsoft.Extensions.DependencyInjection;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddSingleton<AuthService>();
builder.Services.AddSingleton<AgentService>();
builder.Services.AddSingleton<CommandService>();

builder.Services.AddHttpClient<TeamServerClient>((sp, client) =>
{
    client.BaseAddress = new Uri("http://localhost:5000");
    var authService = sp.GetRequiredService<AuthService>();
    var token = authService.GenerateToken();
    client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Bearer {token}");
});

await builder.Build().RunAsync();
