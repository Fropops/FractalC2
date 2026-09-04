# Configuration & Storage Mechanics — Technical Documentation

## Overview

Because WebCommander is a client-side WebAssembly application, traditional ASP.NET Core server configuration mechanisms (`appsettings.json`, environment variables, IIS web.config) do not apply to the running client instance.

Instead, WebCommander utilizes a hybrid configuration and storage strategy:
1. **Client-Side Web Storage (`localStorage`)**: Persists connection profiles and agent-specific terminal logs across browser sessions.
2. **Dynamic In-Memory Reconfiguration**: Manages dynamic `HttpClient` base URIs and JWT Bearer token generation on the fly.
3. **Stateless Symmetric Token Signing**: Constructs and signs HMAC-SHA256 JWT tokens directly inside the browser using the operator's pre-shared API key.

---

## 1. Authentication Configuration Contract

Located in `Models/AuthConfig.cs`:
```csharp
namespace WebCommander.Models
{
    public class AuthConfig
    {
        public string ServerUrl { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
    }
}
```

### Storage Location & JSON Schema
- **Storage Target**: Browser `window.localStorage`
- **Key**: `fractalc2_auth`
- **Example Value**:
  ```json
  {
    "ServerUrl": "http://192.168.1.100:5000",
    "Username": "operator_alice",
    "ApiKey": "FractalC2SecretApiKey123456789012"
  }
  ```

### Storage Lifecycle
- **Reading (`AuthService.GetAuthConfigAsync`)**: On application startup (`MainLayout.OnInitializedAsync`), the service invokes `localStorage.getItem("fractalc2_auth")`. If found, it deserializes the JSON string and caches the instance in `_cachedAuth`.
- **Writing (`AuthService.SaveAuthConfigAsync`)**: When submitting `LoginModal.razor`, the service serializes `AuthConfig` into JSON, writes it to `localStorage.setItem("fractalc2_auth", json)`, and updates the memory cache.
- **Clearing (`AuthService.ClearAuthConfigAsync`)**: When the operator clicks **Disconnect**, the key is removed via `localStorage.removeItem("fractalc2_auth")` and `_cachedAuth` is set to `null`.

---

## 2. Client-Side JWT Token Generation

WebCommander uses the `System.IdentityModel.Tokens.Jwt` package to construct cryptographically signed tokens directly inside WebAssembly:

```csharp
public async Task<string> GenerateTokenAsync()
{
    var auth = await GetAuthConfigAsync();
    if (auth == null)
        throw new InvalidOperationException("No authentication configuration found");

    var tokenHandler = new JwtSecurityTokenHandler();
    var key = Encoding.ASCII.GetBytes(auth.ApiKey);
    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(new[] { 
            new Claim("id", auth.Username), 
            new Claim("session", _sessionId) 
        }),
        Expires = DateTime.UtcNow.AddDays(7),
        SigningCredentials = new SigningCredentials(
            new SymmetricSecurityKey(key), 
            SecurityAlgorithms.HmacSha256Signature)
    };
    var token = tokenHandler.CreateToken(tokenDescriptor);
    return tokenHandler.WriteToken(token);
}
```

### Decoded Token Structure:
- **Header**:
  ```json
  {
    "alg": "HS256",
    "typ": "JWT"
  }
  ```
- **Payload Claims**:
  - `id`: Operator username (audited in TeamServer action logs).
  - `session`: Ephemeral GUID generated when `AuthService` initializes in the current browser tab.
  - `exp`: UNIX epoch expiration timestamp (set to 7 days from generation).
- **Signature**: HMAC-SHA256 signature calculated over the UTF-8 header and payload bytes using `AuthConfig.ApiKey` as the secret key.

---

## 3. Terminal History Storage

To ensure operators retain terminal outputs and command history across browser refreshes, `TerminalHistoryService` maintains per-agent history files in `localStorage`.

### Model (`Models/TerminalHistory.cs`):
```csharp
public enum TerminalLineType
{
    Normal,
    Error,
    Warning,
    Info,
    Command,
    Success
}

public class TerminalLine
{
    public string Text { get; set; } = string.Empty;
    public TerminalLineType Type { get; set; } = TerminalLineType.Normal;
    public Dictionary<string, string>? Metadata { get; set; }
}

public class TerminalHistory
{
    public List<TerminalLine> OutputLines { get; set; } = new();
    public List<string> CommandHistory { get; set; } = new();
    public HashSet<string> SentTaskIds { get; set; } = new();
    public Dictionary<string, string> TaskCommands { get; set; } = new();
}
```

### Storage Location & Schema
- **Key Pattern**: `terminal_history_{agentId}` (e.g., `terminal_history_a1b2c3d4`)
- **Metadata Dictionary**: Stores contextual attributes used by `TerminalOutput.razor` to re-render interactive dropdown menus upon reload:
  - `IsLsRow`: `"true"`
  - `IsFile`: `"true"` or `"false"`
  - `Name`: Full remote path
  - `IsPsRow`: `"true"`
  - `ProcessId`: Host process identifier

---

## 4. Security Considerations

1. **Browser Sandbox Isolation**: `localStorage` entries are partitioned strictly by the WebCommander origin domain (`protocol + host + port`). Other web pages cannot read stored API keys or terminal logs.
2. **Absence of Server-Side Session State**: Because WebCommander signs tokens using the pre-shared API key, the TeamServer does not need to maintain session databases or synchronize state across multiple server nodes.
3. **Key Size Constraint**: Under .NET 8, `SymmetricSecurityKey` requires secret keys for HMAC-SHA256 to have a minimum bit-length (typically 256 bits / 32 bytes). If an operator enters an excessively short API key, `AuthService` handles the `ArgumentOutOfRangeException` and displays a descriptive validation warning.

For high-level operational concepts and user instructions, return to the [Functional Documentation Index](../../Functional/WebCommander/index.md).
