# FractalC2 TeamServer — Functional Documentation

## Overview

The **FractalC2 TeamServer** is the central command-and-control (C2) operations server of the FractalC2 platform. It serves as the master coordinator, communication gateway, intelligence clearinghouse, and orchestration engine for cyber operations, red team assessments, and authorized adversary simulation exercises.

Acting as the single source of truth, the TeamServer bridges field-deployed implants (**Agents**) operating within target network environments and human operators interacting through client interfaces (**Commander UI / CLI**). It ensures that all mission telemetry, exfiltrated files, and operational history are securely tracked, coordinated in real-time across multiple collaborating operators, and reliably persisted.

```mermaid
graph TD
    subgraph Operators["Operator Tier"]
        Op1["Lead Operator (Commander UI)"]
        Op2["Analyst / Operator (Web / CLI)"]
    end

    subgraph Server["FractalC2 TeamServer (Core Controller)"]
        Auth["Authentication & Session State"]
        Dispatcher["Frame Routing & Task Dispatcher"]
        ListenerMgr["C2 Listeners & Web Staging"]
        PivotingMgr["SOCKS & Reverse Port Forwarding"]
        LootRepo["Loot & Artifact Repository"]
        PayloadFactory["On-Demand Payload Generator"]
        Database[("SQLite Operational State")]
    end

    subgraph TargetNetwork["Target Infrastructure"]
        EdgeAgent["Perimeter Agent (Egress Gateway)"]
        RelayAgent1["Internal Agent A (P2P Child)"]
        RelayAgent2["Internal Agent B (P2P Mesh)"]
    end

    Op1 <== "REST API / JWT Session" ==> Server
    Op2 <== "REST API / JWT Session" ==> Server

    Server <== "HTTPS / HTTP (Encrypted NetFrames)" ==> EdgeAgent
    EdgeAgent <== "TCP / Named Pipe (P2P Relay)" ==> RelayAgent1
    RelayAgent1 <== "TCP / Named Pipe (P2P Relay)" ==> RelayAgent2
```

---

## Core Capabilities & Functional Areas

The capabilities of the TeamServer are organized into modular, purpose-built functional areas:

| Functional Area | Description | Functional Guide | Technical Reference |
| :--- | :--- | :--- | :--- |
| **Agent Lifecycle & Mesh Tracking** | Registration, heartbeat tracking, metadata telemetry, and multi-tier peer-to-peer relay mesh discovery. | [Read Feature Guide](./agent-management.md) | [Agent Subsystem](../../Technical/TeamServer/agent-and-relay-system.md) |
| **Tasking & Automated Interception** | Dispatching operator tasks, automated payload assembly, shellcode conversion, and result aggregation. | [Read Feature Guide](./task-execution.md) | [Tasking Engine](../../Technical/TeamServer/tasking-and-interception.md) |
| **Listener & Ingress Channels** | Dynamic HTTP/HTTPS ingress listeners, port management, TLS handling, and unified C2/staging endpoints. | [Read Feature Guide](./listener-management.md) | [Listener Architecture](../../Technical/TeamServer/listener-subsystem.md) |
| **Implant & Payload Factory** | On-demand compilation of Windows, Linux, and Python implants, shellcode generation, and DLL crafting. | [Read Feature Guide](./implant-generation.md) | [Payload & Tools](../../Technical/TeamServer/payload-and-tools.md) |
| **Network Pivoting & Tunneling** | Integrated SOCKS4 proxy servers and Reverse Port Forwarding to tunnel operator tools into target enclaves. | [Read Feature Guide](./network-pivoting.md) | [Network Forwarding](../../Technical/TeamServer/network-forwarding.md) |
| **Loot & Exfiltration Management** | File exfiltration repository, automatic screenshot capture processing, and thumbnail image preview generation. | [Read Feature Guide](./loot-and-artifacts.md) | [Loot & WebHost](../../Technical/TeamServer/loot-and-webhost.md) |
| **Operator Tool Repository** | Central repository for offensive binaries (.NET assemblies, native EXEs, PowerShell scripts) with type inspection. | [Read Feature Guide](./tools-repository.md) | [Payload & Tools](../../Technical/TeamServer/payload-and-tools.md) |
| **Web Hosting & Staging** | On-the-fly public file hosting for payloads, staging delivery stubs, and web download telemetry logging. | [Read Feature Guide](./web-hosting.md) | [Loot & WebHost](../../Technical/TeamServer/loot-and-webhost.md) |
| **Multi-User Collaboration & Auditing** | Multi-operator access control via JWT, real-time delta change sync, and time-stamped operational audit trails. | [Read Feature Guide](./multi-user-and-audit.md) | [Security & Audit](../../Technical/TeamServer/security-auth-and-audit.md) |

---

## Target Audience & Operational Roles

- **Engagement Leads & Product Owners**: Gain visibility into assessment progress, operational boundaries, security posture, and compliance telemetry.
- **Red Team Operators & Campaign Specialists**: Coordinate multi-operator actions, deploy resilient C2 infrastructure, bypass segmentation via mesh relaying, and manage target intelligence.
- **Blue Teamers & Security Analysts**: Understand the server-side architectural patterns, network transport characteristics, and behavioral indicators of modern C2 servers.

For developer-focused architectural deep dives, class diagrams, database schemas, and implementation specifics, see the [Technical Documentation Index](../../Technical/TeamServer/index.md).
