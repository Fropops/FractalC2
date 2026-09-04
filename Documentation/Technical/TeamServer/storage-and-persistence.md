# Storage & Persistence Layer — Technical Guide

## System Overview

The **Storage & Persistence Layer** provides the durable data store for the TeamServer. It is built upon `sqlite-net-pcl`, a lightweight, zero-configuration, cross-platform SQLite ORM.

The architecture combines **In-Memory Speed** with **Durable Disk Persistence**:
- High-frequency operational read/write actions operate against optimized in-memory collections (`Dictionary`, `List`).
- Modifications are asynchronously written to the local SQLite database (`data.db`).
- On server restart, a reflective hydration engine (`IStorable`) reads all tables from SQLite and repopulates the in-memory state.

```mermaid
graph TD
    subgraph AppStart["Server Startup Hydration"]
        Startup["Startup.LoadFromDB()"] --> Scan["Scan Assembly for IStorable Interfaces"]
        Scan --> CallLoad["Invoke service.LoadFromDB()"]
    end

    subgraph MemoryCaches["Singleton In-Memory Caches"]
        AgentCache["AgentService._agents"]
        TaskCache["TaskService._tasks & _agentTasks"]
        ResultCache["TaskResultService._results"]
        ListenerCache["ListenerService._listeners"]
        ImplantCache["ImplantService._implants"]
        WebCache["WebHostService.files & logs"]
    end

    subgraph DBService["Database Layer (DatabaseService.cs)"]
        AsyncConn["SQLiteAsyncConnection (data.db)"]
        DAOMap["Implicit Operators (Model <--> DAO)"]
    end

    subgraph Tables["SQLite Database Tables (data.db)"]
        T_Agents[("agents")]
        T_Tasks[("tasks")]
        T_Results[("results")]
        T_Listeners[("http_handlers")]
        T_Implants[("implants")]
        T_WebFiles[("web_host_file")]
        T_WebLogs[("web_host_log")]
    end

    CallLoad --> AsyncConn
    AsyncConn --> Tables
    Tables --> DAOMap
    DAOMap --> MemoryCaches
    MemoryCaches <== "Asynchronous Write-Through" ==> AsyncConn
```

---

## Database Schema & Entity Relationships

```mermaid
erDiagram
    agents ||--o{ tasks : "receives"
    tasks ||--o| results : "produces"
    http_handlers ||--o{ agents : "receives ingress from"
    implants }o--|| http_handlers : "configured for"
    web_host_file ||--o{ web_host_log : "generates access logs"

    agents {
        string id PK
        string name
        datetime first_seen
        datetime last_seen
        bool is_deleted
        string hostname
        string username
        string process_name
        int process_id
        byte integrity
        string architecture
        string endpoint
        string version
        blob address
        int sleep_interval
        int sleep_jitter
    }

    tasks {
        string id PK
        string agent_id FK
        byte command_id
        string command_label
        datetime date
        bool is_deleted
    }

    results {
        string id PK
        string output
        blob objects
        string error
        string info
        byte status
        bool is_deleted
    }

    http_handlers {
        string id PK
        string name
        int port
        string address
        bool secure
    }

    implants {
        string id PK
        string name
        blob data
        string config
        string listener
        bool isDeleted
    }

    web_host_file {
        string path PK
        string description
        bool is_powershell
        blob data
    }

    web_host_log {
        string id PK
        datetime date
        string url
        string path
        string user_agent
        int status_code
    }
```

---

## The `IDatabaseService` Contract (`DatabaseService.cs`)

`DatabaseService` manages table initialization and provides generic, asynchronous CRUD primitives:

```csharp
[InjectableService]
public interface IDatabaseService
{
    Task<List<T>> Load<T>() where T : TeamServerDao, new();
    Task Insert<T>(T item) where T : TeamServerDao, new();
    Task Update<T>(T item) where T : TeamServerDao, new();
    Task<int> Remove<T>(T item) where T : TeamServerDao, new();
    Task<int> Clear<T>() where T : TeamServerDao, new();
    Task<T> Get<T>(Expression<Func<T, bool>> expr) where T : TeamServerDao, new();
}
```

### Connection Management
- During initialization, a synchronous `SQLiteConnection` executes `conn.CreateTable<T>()` for all eight entity DAOs, guaranteeing the database file (`Folders:DBFolder/data.db`) and tables exist before any service executes queries.
- Operational queries use a non-blocking `SQLiteAsyncConnection`, preventing thread contention during heavy agent polling.

---

## Data Access Objects (DAOs) & Implicit Conversions

To maintain complete architectural separation between domain logic and persistence concerns, the codebase implements the **DAO Mapping Pattern** with C# implicit type conversion operators (`implicit operator`).

### Example: `AgentDao.cs`

```csharp
[Table("agents")]
public sealed class AgentDao : TeamServerDao
{
    [PrimaryKey, Column("id")]
    public string Id { get; set; }
    [Column("hostname")]
    public string Hostname { get; set; }
    // ... other columns

    // Convert Domain Agent to SQLite DAO
    public static implicit operator AgentDao(Agent agent)
    {
        var dao = new AgentDao { Id = agent.Id, FirstSeen = agent.FirstSeen, LastSeen = agent.LastSeen };
        if (agent.Metadata != null)
        {
            dao.Hostname = agent.Metadata.Hostname;
            dao.UserName = agent.Metadata.UserName;
            // flatten metadata into table columns
        }
        return dao;
    }

    // Convert SQLite DAO to Domain Agent
    public static implicit operator Agent(AgentDao dao)
    {
        if (dao == null) return null;
        var agent = new Agent(dao.Id) { FirstSeen = dao.FirstSeen, LastSeen = dao.LastSeen };
        if (!string.IsNullOrEmpty(dao.EndPoint))
        {
            agent.Metadata = new AgentMetadata { Hostname = dao.Hostname, ... };
        }
        return agent;
    }
}
```

### Conversion Advantages
- Services interact purely with business domain objects (`Agent`, `Listener`, `TeamServerAgentTask`).
- Persistence calls simply cast the entity: `_dbService.Insert((AgentDao)agent)` or `_dbService.Update((TaskDao)task)`.
- Complex nested objects (e.g., `ImplantConfig` inside `Implant`) are cleanly serialized to JSON strings inside the DAO conversion methods.

---

## Startup Hydration Pattern (`IStorable`)

Services that own persistent state implement `IStorable`:

```csharp
public interface IStorable
{
    Task LoadFromDB();
}
```

In `Startup.cs`, `LoadFromDB(app)` dynamically locates every registered interface extending `IStorable` and calls `LoadFromDB()`:

```csharp
private void LoadFromDB(IApplicationBuilder app)
{
    var assembly = Assembly.GetExecutingAssembly();
    var storableInterfaceTypes = assembly.GetTypes()
        .Where(t => t.IsInterface &&
                   typeof(IStorable).IsAssignableFrom(t) &&
                   t != typeof(IStorable));

    foreach(var storableInterface in storableInterfaceTypes)
    {
        var service = app.ApplicationServices.GetService(storableInterface) as IStorable;
        service?.LoadFromDB();
    }
}
```

### Hydration Sequence
1. **`AgentService.LoadFromDB()`**: Loads all non-deleted agents (`IsDeleted == false`).
2. **`TaskService.LoadFromDB()`**: Restores tasks and builds the `_agentTasks` secondary index.
3. **`TaskResultService.LoadFromDB()`**: Loads task outputs.
4. **`ListenerService.LoadFromDB()`**: Restores listener configurations and calls `listener.Start()` to reopen Kestrel ports.
5. **`ImplantService.LoadFromDB()`**: Rebuilds the staged implant catalog.
6. **`WebHostService.LoadFromDB()`**: Reloads hosted files and historical access logs.

---

## Soft Deletion Strategy

To prevent data loss and preserve forensic integrity:
- Entities such as `AgentDao`, `TaskDao`, and `ResultDao` include an `is_deleted` column (`bool IsDeleted`).
- Calling `RemoveAgent()` or `StopAgent()` sets `dao.IsDeleted = true` and updates the record instead of executing a SQL `DELETE`.
- Hydration routines filter out records where `IsDeleted == true`, ensuring deleted entities do not appear in operator consoles while retaining their database audit trail.

---

## Technical Reference Links

- **Application Startup Hook**: [Architecture, Hosting & DI](./architecture-and-di.md)
- **Agent Lifecycle**: [Agent & Relay System](./agent-and-relay-system.md)
- **Task & Result Persistence**: [Tasking & Interception Engine](./tasking-and-interception.md)
- **Functional Overview**: [Functional Index](../../Functional/TeamServer/index.md)
