# Implant Generation Process

This document details the technical process of generating implants within the FractalC2 framework.

## Overview

Implant generation is handled by the **TeamServer**, specifically via the `ImplantsController` which delegates the core logic to the `PayloadGenerator` class in `Common.Payload.Generation`.

The process involves:
1.  **Configuration**: Receiving parameters (URL, Listener, Arch, Type).
2.  **Assembly Patching**: Embedding configuration into the base Agent binary.
3.  **Obfuscation/Encryption**: Encrypting the payload to evade detection.
4.  **Packaging/Encapsulation**: Wrapping the payload into the final artifact (EXE, DLL, PS1, etc.).

## Generation Flow

```mermaid
graph TD
    Request[WebCommander/CLI Request] -->|Config JSON| TS[TeamServer Controller]
    TS -->|ImplantConfig| Generator[PayloadGenerator]
    
    subgraph Core Preparation
        Base[Agent.exe] -->|Resource Patching| Configured[Configured Agent]
        Configured -->|Encryption (AES)| Encrypted[Encrypted Agent]
        Encrypted -->|Embed in Starter| Staged[Staged Agent (Starter.exe)]
    end
    
    subgraph Encapsulation
        Staged -->|Resource Replace| EXE[Executable (.exe)]
        Staged -->|Base64 + Template| PS1[PowerShell (.ps1)]
        AgentLinux -->|Binary Pattern Patch| ELF[Linux (.elf)]
    end

    Generator -->|Binaries| TS
    TS -->|Store/Download| User
```

## Base Agent Preparation (`PrepareAgent`)

For Windows implants (.NET), the generation follows these steps:

1.  **Load Reference Assembly**: `Agent.exe` is loaded from the templates folder.
2.  **Configuration Patching**: The `EndPoint` (URL) and `ServerKey` are injected into the assembly's resources using `AssemblyEditor.ReplaceRessources`.
3.  **Encryption**: The configured agent is encrypted (AES).
4.  **Patcher Creation**: A `Patcher.dll` is created and encrypted. This component is responsible for decrypting and loading the agent in memory.
5.  **Starter Creation**: A `Starter.exe` (or `Service.exe` for services) is loaded.
    *   The encrypted Agent and Patcher are embedded as resources into the Starter.
    *   The Starter acts as a stager: upon execution, it extracts, decrypts, and runs the Agent in memory.

## Encapsulation Methods

Once the "Core" agent (the Starter) is ready, it is encapsulated based on the requested output format:

### Executable (.exe)
*   **Method**: Resource Replacement.
*   **Loader**: `ResourceAssemblyLoader.exe`.
*   **Process**: The system uses a Python script (`replace-resource.py`) to embed the generated Staged Agent into the `ResourceAssemblyLoader` binary.
*   **Result**: An EXE file that, when run, loads the embedded assembly.

### PowerShell (.ps1)
*   **Method**: Base64 String Replacement.
*   **Template**: `payload.ps1`.
*   **Process**:
    1.  The Staged Agent is encoded in Base64.
    2.  The `[[PAYLOAD]]` placeholder in `payload.ps1` is replaced with this Base64 string.
*   **Result**: A PowerShell script that decodes and loads the assembly from memory.

### Linux (.elf)
The process for Linux is different as it is a native binary, not a .NET assembly wrapper (in the same sense).
*   **Source**: `AgentLinux` binary.
*   **Method**: Binary Pattern Patching.
*   **Process**:
    1.  Reads the compiled `AgentLinux` binary.
    2.  Locates placeholders `[KEY]` and `[ENDPOINT]` (padded with `*`).
    3.  Overwrites these placeholders directly in the binary file with the actual configuration values.

## Key Classes & Files

*   **`Common.Payload.PayloadGenerator`**: Main entry point for generation logic.
    *   `GenerateImplant()`: Orchestrates the process.
    *   `PrepareAgent()`: Handles the Core Preparation (Config + Encryption + Stager).
    *   `ExecutableEncapsulation()`, `PowershellEncapsulation()`, `ElfPrepare()`:  Format-specific logic.
*   **`Common.Payload.AssemblyEditor`**: Helper for modifying .NET assemblies (renaming, resource injection).
*   **`TeamServer.Controllers.ImplantsController`**: API endpoint receiving the creation request.
