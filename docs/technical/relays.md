# Technical Documentation: Relays and Pivoting

This document describes the internal workings of relays (pivoting) in the FractalC2 architecture, covering both Agent-side and TeamServer-side implementations.

## 1. Overview

The relay system allows "chaining" agents via Peer-to-Peer (P2P) channels, such as Named Pipes (SMB) or direct TCP connections. This enables the TeamServer to reach machines that do not have direct Internet access by passing through one or more intermediate agents.

When Agent A connects to Agent B (Link), A becomes the parent and B the child. If B subsequently connects to C, A will serve as a relay to reach C.

## 2. Agent-Side Architecture

The core relay logic resides in the `Agent` class (`Agent/Agent.cs`) and the communication managers (`Agent/Communication/`).

### 2.1. P2P Communication Modules

Connections between agents are managed by classes inheriting from `P2PCommunicator`:

*   **PipeCommModule** (`Agent/Communication/PipeCommModule.cs`): Handles SMB communications (Named Pipes).
*   **TcpCommModule** (`Agent/Communication/TcpCommModule.cs`): Handles TCP communications (Bind or Reverse).

Each P2P module acts as a tunnel for network frames (`NetFrame`).

### 2.2. Connection Management (Routing Tables)

The agent maintains two main dictionaries for routing:

1.  **`_childrenComm`**:
    *   Maps the ID of a **direct child** agent to its communication module (`P2PCommunicator`).
    *   Used to send data to the "next hop".

2.  **`_relaysComm`**:
    *   Maps the ID of **any descendant** agent (child, grandchild, etc.) to the communication module of the direct child that enables reaching it.
    *   This is essentially a routing table: "To reach Agent X, I must go through the link to Agent Y".

### 2.3. Link Establishment

The connection process proceeds as follows (`link` command):

1.  The user initiates the `link` command on the Parent Agent.
2.  `LinkCommand` instantiates the appropriate `P2PCommunicator` (e.g., `PipeCommModule`) and calls `Agent.AddChildCommModule`.
3.  **Connection**: The module attempts to connect to the child's P2P listener.
4.  **Handshake**:
    *   The Parent sends a `Link` frame containing the task ID and the Parent ID.
    *   The Child receives this frame, updates its Parent ID, and responds with a `Link` frame (containing its info) and a `CheckIn` frame (its Metadata).
5.  **Routing Update**:
    *   Upon receiving the `Link` frame, the Parent updates `_childrenComm` by replacing the temporary task ID with the actual Child ID.
    *   It also adds the Child to `_relaysComm`.
    *   It notifies the TeamServer (or its own parent) via `SendRelays()`.

### 2.4. Packet Routing (Frames)

The `HandleFrame` method handles incoming packets (from TeamServer/Parent):
*   If `frame.Destination` is the current agent's ID: The packet is processed locally.
*   If `frame.Destination` is different: The agent consults `_relaysComm`. If a route exists, it forwards the packet to the corresponding P2P module (`child.SendFrame(frame)`).

The `OnFrameReceivedFromChild` method handles incoming packets from a Child:
*   It intercepts control frames (`Link`, `LinkRelay`) to update local routing tables.
*   For any other frame type (e.g., `TaskResult`), it forwards them to the `MasterCommunicator` (towards the TeamServer/Parent).

## 3. TeamServer-Side Architecture

The TeamServer must maintain the full topology to know which agent to route through to reach a target.

Key files are located in `TeamServer/FrameHandling/`.

### 3.1. Frame Handlers

*   **LinkFrameHandler**: Processes `Link` frames. It updates the parent agent model to add the child to its list of `Links`.
*   **UnlinkFrameHandler**: Handles link removal.
*   **LinkRelayFrameHandler**: Handles `LinkRelay` frames.
    *   These frames contain a list of agent IDs accessible via a given link.
    *   The TeamServer updates the `RelayId` property of the affected agents. The `RelayId` indicates the ID of the "Pivot" agent to which the TeamServer must send tasks to reach the final agent.

## 4. Complete Data Flow

### Scenario: Sending a command to a "Grandchild" Agent (C)
*Architecture: TeamServer -> Agent A -> Agent B -> Agent C*

1.  **TeamServer**: User sends a task to C. TS sees C is relayed by A. It encapsulates the task and sends it to A.
2.  **Agent A**: Receives the frame.
    *   Sees `Destination` = C.
    *   Consults `_relaysComm`: "For C, go through the module connected to B".
    *   Transmits the frame to B.
3.  **Agent B**: Receives the frame.
    *   Sees `Destination` = C.
    *   Consults `_childrenComm`: "C is my direct child".
    *   Transmits the frame to C.
4.  **Agent C**: Receives the frame.
    *   Sees `Destination` = C.
    *   Executes the command.

### Scenario: Return result from C

1.  **Agent C**: Completes the task. Sends `TaskResult` to its Parent (B).
2.  **Agent B**: Receives frame from C.
    *   `OnFrameReceivedFromChild`: Forwards the frame to its Parent (A).
3.  **Agent A**: Receives frame from B.
    *   `OnFrameReceivedFromChild`: Forwards the frame to the TeamServer.
4.  **TeamServer**: Receives the result and displays it.
