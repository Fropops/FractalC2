using Microsoft.AspNetCore.StaticFiles;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel pour écouter sur toutes les interfaces
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(5001); // HTTP
    // serverOptions.ListenAnyIP(5002, o => o.UseHttps()); // HTTPS si besoin
});

// Configure le chemin vers les fichiers WebAssembly
builder.WebHost.UseWebRoot("wwwroot");

var app = builder.Build();

// Servir TOUS les fichiers statiques (y compris _framework)
var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".dat"] = "application/octet-stream";
provider.Mappings[".dll"] = "application/octet-stream";
provider.Mappings[".wasm"] = "application/wasm";
provider.Mappings[".blat"] = "application/octet-stream";
provider.Mappings[".br"] = "application/octet-stream";

app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = provider,
    ServeUnknownFileTypes = true,
    DefaultContentType = "application/octet-stream"
});

// Fallback pour le routing Blazor
app.MapFallbackToFile("index.html");

app.Run();