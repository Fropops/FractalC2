# Release Notes

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