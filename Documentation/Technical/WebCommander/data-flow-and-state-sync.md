# Data Flow & State Synchronization — Technical Documentation

## Overview

Because WebCommander executes as a client-side WebAssembly application in the operator's browser, data synchronization with the central TeamServer must handle asynchronous network latency, browser thread execution constraints, and non-blocking real-time event updates.

This document details the control flow, network interactions, and state propagation pipelines for the major operational scenarios.

---

## 1. Authentication & Initial State Synchronization

When an operator launches WebCommander or submits the login form:

```mermaid
sequenceDiagram
    autonumber
    actor Op as Operator
    participant UI as LoginModal.razor
    participant Auth as AuthService
    participant Client as TeamServerClient
    participant State as AgentService
    participant TS as TeamServer API

    Op->>UI: Enter ServerUrl, Username, ApiKey -> Click "Connect"
    UI->>Auth: SaveAuthConfigAsync(config)
    Auth->>Auth: Write to localStorage ("fractalc2_auth")
    
    UI->>Client: ReconfigureAsync()
    Client->>Auth: GenerateTokenAsync()
    Auth-->>Client: Signed JWT Bearer Token
    Client->>Client: Set BaseAddress & Authorization Header
    
    UI->>Auth: ValidateConnectionAsync(client)
    Auth->>Client: ValidateAuthAsync()
    Client->>TS: GET /Session/Auth
    TS-->>Client: 200 OK
    
    UI->>State: InitializeDataAsync()
    State->>State: ClearCache() & Set IsInitialLoading = true
    State->>Client: GetChangesAsync(history: true)
    Client->>TS: GET /session/Changes?history=true
    TS-->>Client: Full Historical Changes List (Agents, Listeners, Tasks, Implants)
    
    loop For each change in initial snapshot
        State->>State: Process change element & increment ProcessedChanges
        State->>UI: Trigger OnProgressUpdated (updates progress bar)
    end

    State->>State: Set IsInitialLoading = false
    State->>UI: Trigger OnLoadingStateChanged
    UI->>State: StartPolling() (Instantiate 2-second Timer)
    UI-->>Op: Close modal and render Dashboard
```

---

## 2. Background Delta Polling Loop

Once initialized, `AgentService` maintains a recurring `System.Threading.Timer` that fires every 2 seconds:

```mermaid
sequenceDiagram
    autonumber
    participant Timer as System.Threading.Timer (2s)
    participant State as AgentService
    participant Client as TeamServerClient
    participant TS as TeamServer API
    participant UI as Active Razor Page / Notification

    Timer->>State: Tick (PollForChanges)
    State->>Client: GetChangesAsync(history: false)
    Client->>TS: GET /session/Changes?history=false
    
    alt Changes Available
        TS-->>Client: [Change(Agent, ID1), Change(Result, ID2)]
        
        State->>Client: GetAgentAsync(ID1)
        Client->>TS: GET /api/Agents/{ID1}
        TS-->>Client: Agent Data
        State->>State: Update _agents[ID1]
        State->>UI: Emit OnAgentsUpdated / OnNewAgent
        
        State->>Client: GetTaskResultAsync(ID2)
        Client->>TS: GET /api/Tasks/{ID2}/result
        TS-->>Client: Task Result
        State->>State: Update _taskResults[ID2]
        State->>UI: Emit OnAgentResult(result, task)
    else No Changes
        TS-->>Client: [] (Empty List)
    else Network Error / 5xx
        TS-->>Client: Connection Failed
        State->>State: Set HasConnectionError = true
        State->>UI: Emit OnConnectionStatusChanged (Show retry banner)
    else Authorization Expired / 401
        TS-->>Client: 401 Unauthorized
        State->>State: StopPolling() & Set HasAuthorizationError = true
        State->>UI: Emit OnAuthorizationErrorChanged (Show Re-auth Modal)
    end
```

---

## 3. Command Tasking & Result Deserialization Flow

When an operator issues a command in the terminal (e.g., `ls C:\Windows`):

```mermaid
sequenceDiagram
    autonumber
    actor Op as Operator
    participant Term as Terminal.razor
    participant CmdSvc as CommandService
    participant Adapter as WebAgentCommandAdapter
    participant TSClient as TeamServerClient
    participant TS as TeamServer API
    participant Agent as Target Implant

    Op->>Term: Enters "ls C:\Windows"
    Term->>CmdSvc: ParseAndSendAsync("ls C:\Windows", agent)
    CmdSvc->>Adapter: Execute command handler
    Adapter->>TSClient: TaskAgent("ls C:\Windows", agent.Id, CommandId.Ls, parms)
    
    TSClient->>TSClient: BinarySerializeAsync(agentTask)
    TSClient->>TS: POST /api/Tasks/{agentId} (CreateTaskRequest)
    TS-->>TSClient: 200 OK (Task Created)
    TSClient-->>Term: Output: "Command ls tasked to agent..."
    
    Note over TS,Agent: Agent checks in, executes "ls", and serializes ListDirectoryResult into Objects
    Agent->>TS: Check-In Response (AgentTaskResult: Objects = byte[])
    
    Note over TSClient,TS: Delta polling detects ChangingElement.Result
    TSClient->>TS: GET /api/Tasks/{taskId}/result
    TS-->>TSClient: AgentTaskResult (Status = Completed, Objects = byte[])
    TSClient->>Term: HandleAgentResult(result, task)
    
    Term->>Term: ResultObjectHelper.DeserializeListDirectoryResults(result.Objects)
    Term->>Term: AddLsTable(listResult)
    Term-->>Op: Renders interactive directory table with Download/Delete buttons
```

---

## 4. Payload Generation Pipeline

When an operator creates a new implant via `ImplantCreator.razor`:

```mermaid
sequenceDiagram
    autonumber
    actor Op as Operator
    participant UI as ImplantCreator.razor
    participant Client as TeamServerClient
    participant TS as TeamServer Compiler Engine

    Op->>UI: Select Listener, Format (e.g. Executable), Injection (explorer.exe)
    Op->>UI: Click "Create Implant"
    UI->>Client: CreateImplantAsync(config)
    Client->>TS: POST /api/Implants/generate (ImplantConfig JSON)
    
    TS->>TS: Parse template, inject endpoints & encryption keys
    TS->>TS: Compile executable via Roslyn / MSBuild
    TS->>TS: Stage binary at /imp/{ImplantName}.exe
    TS-->>Client: APIImplantCreationResult (Implant metadata + compilation logs)
    
    Client-->>UI: (success, result)
    UI->>UI: Emit OnCreationResult
    UI-->>Op: Display success toast with build logs
```

---

## 5. Failover and Reconnection Mechanics

WebCommander implements a dual-tier error recovery mechanism:

```mermaid
flowchart TD
    PollTick["Polling Loop Check"] --> Req["GET /session/Changes"]
    Req -- "HTTP 200 OK" --> Healthy["Clear error states & update UI"]
    
    Req -- "HTTP 401 / 403" --> AuthFail["Authorization Error Detected"]
    AuthFail --> StopPoll["Stop Polling Timer"]
    StopPoll --> ShowAuthModal["Display Blocking Re-authentication Modal"]
    ShowAuthModal --> ReAuth["Operator re-enters credentials -> Re-verify"]
    
    Req -- "HTTP 5xx / Network Drop" --> NetFail["Connection Error Detected"]
    NetFail --> Overlay["Show 'TeamServer Unavailable' Overlay"]
    Overlay --> Retry["Continue Polling Timer every 2s (Retry Loop)"]
    Retry --> Req
```

1. **Transient Network Interruptions**: When network requests fail due to socket drops or server restarts, `AgentService` sets `_hasConnectionError = true`. The `<ConnectionErrorOverlay>` component displays a semi-transparent banner indicating automatic retry. The polling timer continues running, automatically dismissing the overlay once the server recovers.
2. **Permanent Credential Rejection**: When requests return 401 Unauthorized or 403 Forbidden, `AgentService` immediately disposes of the polling timer to prevent flooding the server with unauthorized requests. It sets `HasAuthorizationError = true`, triggering `<ConnectionErrorOverlay>` to prompt for immediate re-authentication.

For storage formats and configuration keys, see [Technical: Configuration & Storage](./configuration-and-storage.md).
