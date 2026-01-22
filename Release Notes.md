# Release Notes

## [22/01/2026]
### WebCommander 2.4.0 / Commander 2.4.0 / TeamServer 2.4.0 / Agent (Windows) 2.4.0 / Agent (Linux) 2.4.0
* Rework ps command on Windows Agent (using NtQuerySystemInformation)
* Rework and reactivation of Implant Injection
 - by pid
 - by procname
 - by spawning
* Rework Topology & Map not well working with P2P links

## [15/01/2026]
### WebCommander 2.3.1 / Commander 2.3.1
* Implementation JumpPsExecCommand & JumpWinRMCommand (+ review of elevate & GetSystem)
### Agent 2.3.1
* Improving PsExec Command

## [15/01/2026]
### WebCommander 2.3.0 / Commander 2.3.0
* API Client Centralization and Refactoring

Complete refactoring of API calls with centralization in Common.APIClient. Both Commander (CLI) and WebCommander (Blazor WASM) now rely on this unified API client, ensuring consistency across all client implementations and simplifying maintenance.

## [13/01/2026]
### WebCommander 2.2.3
* Fix TaskResult displayed in the all Terminals

## [13/01/2026]
### WebCommander 2.2.2 / Commander 2.2.2
* Fix Link Command

## [13/01/2026]
### WebCommander 2.2.1 / Commander 2.2.1
* Fix rportfwd Command

## [13/01/2026]
### WebCommander 2.2.0 / Commander 2.2.0
* Complete Command System Overhaul

This release introduces a comprehensive redesign and standardization of the entire command system across all components (Agents, TeamServer, and Clients), improving consistency, maintainability, and extensibility.

## [07/01/2026]
### Agent 2.1.1
* Command rportfwd was lacking result when succeess
### WebCommander 2.1.1 
* Command rportfwd : show & stop not working
* Tasking powershell command was showing agentId instead of Agent name