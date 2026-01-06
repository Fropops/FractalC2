# FractalC2 Technical Architecture

This document provides a high-level technical overview of the "FractalC2" framework architecture, describing the system's components and their interactions.

## Overview

FractalC2 consists of four main components:
1.  **WebCommander** : The web-based user interface (Client).
2.  **Commander** : The command-line interface (CLI) client.
3.  **TeamServer** : The central management server (C2 Server).
4.  **Agent** : The implant deployed on target machines.

These components interact via defined protocols (HTTP/REST) to enable remote control and task execution.

```mermaid
graph TD
    User[Operator] -->|HTTPS| Web[WebCommander (Blazor WASM)]
    User[Operator] -->|HTTPS| CLI[Commander (CLI)]
    Web -->|REST API| TS[TeamServer (ASP.NET Core)]
    CLI -->|REST API| TS
    TS <-->|SQLite| DB[(Database)]
    Agent[Agent (.NET)] -->|HTTP/TCP Beacon| TS
    Agent[Agent (.NET)] -->|PIPE/TCP Beacon| Agent
```

## Components

### 1. WebCommander (Web Client)
The operator interface is a **Blazor WebAssembly** application hosted on the client side. It allows operators to manage agents, listeners, and send commands through a modern web UI.

*   **Architecture**: SPA (Single Page Application) running in the browser.
*   **Communication**: Uses `HttpClient` via the `TeamServerClient` service to interact with the TeamServer REST API.
*   **Key Services**:
    *   `AuthService`: Authentication management (JWT).
    *   `AgentService`: Agent state management.
    *   `TerminalHistoryService`: Command history.

### 2. Commander (CLI Client)
A command-line interface (CLI) alternative to WebCommander, providing similar capabilities for operators who prefer terminal environments.

*   **Architecture**: .NET Console Application.
*   **Communication**: Shares the same REST API integration as WebCommander to communicate with the TeamServer.
*   **Structure**:
    *   `Terminal`: Handles user input and output rendering.
    *   `Executor`: Manages the command loop and execution flow.
    *   `ApiCommModule`: Handles HTTP communication with the TeamServer.

### 3. TeamServer (C2 Server)
The core of the system, an **ASP.NET Core** application, responsible for global state management, data persistence, and communication with clients and agents.

*   **API**: Exposes a REST API via `Controllers` (e.g., `AgentsController`, `ListenersController`, `TasksController`).
*   **Persistence**: Uses **SQLite** via a DAO pattern (`AgentDao`, `TaskDao`, `ResultDao`).
*   **Agent Management**: Receives agent connections (beacons), delivers pending tasks, and stores results.
*   **Listeners**: Manages network entry points (e.g., HTTP, TCP) for agents.

### 4. Agent (Implant)
The component deployed on the target, written in **.NET** (Console App). It is designed to be modular.

*   **Lifecycle**:
    1.  Metadata generation (Host, User, Integrity, etc.).
    2.  Connection to TeamServer via a "Communicator" (via `CommunicationFactory`).
    3.  Main Loop: Polling for tasks -> Execution -> Sending results.
*   **Internal Services**:
    *   `ConfigService`: Agent configuration (Encryption keys, URL).
    *   `NetworkService`, `FileService`: Basic capabilities.
    *   `JobService`: Long-running job management.
    *   `ProxyService`, `ReversePortForwardService`: Advanced network features.
*   **Agent Names**: Randomly generated as "Quality-Animal" (e.g., "Brave-Lion").

## Data Flow

### Tasking
1.  **Creation**: The user enters a command in WebCommander or Commander CLI.
2.  **Dispatch**: The client calls the TeamServer API `POST /Agents/{id}` with the command and parameters.
3.  **Storage**: The TeamServer stores the task in the database (`tasks` table) and queues it for the agent.
4.  **Polling**: The Agent contacts the TeamServer (Check-in). If a task is available, it downloads it.
5.  **Execution**: The Agent executes the task locally.
6.  **Result**: The Agent sends the result back to the TeamServer.
7.  **Review**: The TeamServer stores the result. Clients poll for the result or display it upon user request.

## Shared Libraries
The project uses several shared libraries to ensure data consistency:
*   **Common / Shared**: Contains shared data models (`AgentMetadata`, `AgentTask`, `AgentTaskResult`) and interfaces.

## Security
*   **Authentication**: Bearer Tokens (JWT) for Client -> TeamServer API access.
*   **Encryption**: Agent <-> TeamServer communications (frames) are encrypted (supported via `CryptoService` and `configService.EncryptFrames = true`).
