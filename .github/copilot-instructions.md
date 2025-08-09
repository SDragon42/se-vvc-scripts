# Copilot Instructions for VVC-Scripts

## Project Overview
This repository contains C# scripts for the Void Velocity Challenge (VVC) in Space Engineers. The codebase is organized into multiple projects, each targeting a specific in-game programmable block or function:
- `VVC.Checkpoint/`: Handles checkpoint logic and validation.
- `VVC.RaceTimer/`: Manages race timing, racer state, and results.
- `VVC.RaceTimeSign/`: Displays race times and related information.
- `VVC.Shared/`: Contains shared utilities and logic (e.g., logging, extensions, tags).

## Architecture & Patterns
- **Component Structure:** Each main folder is a separate C# project, typically with a single entry-point class (e.g., `*Program.cs`). Shared code is factored into `VVC.Shared/` and included via project references.
- **MDK Integration:** Scripts are designed for [Malware's Development Kit (MDK²-SE)](https://github.com/malforge/mdk2), enabling deployment to Space Engineers programmable blocks. Each project contains `.mdk.ini` files for MDK configuration.
- **Logging:** Use `DebugLogging` and `Logging` from `VVC.Shared/` for consistent debug output. Avoid direct `Echo` calls outside these utilities.
- **Data Exchange:** Inter-project communication (e.g., between timer and checkpoint) is handled via IGC tags defined in `IGCTags.cs`.

## Developer Workflows
- **Build:**
  - Use `dotnet build` from the repo root to build all projects.
  - Output DLLs are placed in each project's `bin/Debug/netframework48/` directory.
- **Deploy to Space Engineers:**
  - Use MDK's Visual Studio integration or copy the relevant `*Program.cs` to the in-game programmable block editor.
- **Debugging:**
  - Use the logging utilities for in-game debug output. Adjust log levels in `DebugLogging` as needed.

## Conventions & Best Practices
- **Entry Point:** Each project has a `*Program.cs` as the main script file. Keep all block logic within this file or in files included via MDK.
- **Shared Code:** Place reusable logic in `VVC.Shared/` and reference it from other projects.
- **Naming:** Use PascalCase for classes and methods. Prefix IGC tags with `VVC_` as in `IGCTags.cs`.
- **No External NuGet Packages:** Only use assemblies compatible with Space Engineers scripting and MDK.

## Key Files
- `VVC.Shared/Logging.cs`, `VVC.Shared/DebugLogging.cs`: Logging patterns.
- `VVC.Shared/IGCTags.cs`: Defines all IGC communication tags.
- `VVC.Checkpoint/CheckpointProgram.cs`, `VVC.RaceTimer/RaceTimerProgram.cs`, `VVC.RaceTimeSign/RaceTimeSignProgram.cs`: Main entry points for each script.

## Example: Logging Usage
```csharp
// In any script
Logging.AppendLine("Checkpoint reached");
DebugLogging.AppendLine("Timer started");
```

## References
- [MDK²-SE Documentation](https://github.com/malforge/mdk2)
- [Space Engineers Scripting API](https://github.com/malware-dev/MDK-SE/wiki)

---
If you are unsure about a workflow or pattern, check the `README.md` or the `Instructions.readme` in each project folder for more details.
