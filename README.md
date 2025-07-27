# SteamMultiplayer

SteamMultiplayer is a Godot 4.4.1 project using .NET 8 that integrates Steamworks.NET to provide Steam multiplayer features for your game. This project demonstrates how to set up Steam networking, lobby management, and multiplayer menus in a Godot C# environment.

## Features

- Steamworks.NET integration for Godot .NET
- Steam lobby creation, joining, and management
- Multiplayer menu UI for hosting and joining games
- Cross-platform support (Windows, Linux, macOS) with platform-specific native library handling
- Example scripts for managing lobbies and Steamworks initialization

## Requirements

- [Godot Engine 4.4.1 (Mono/.NET version)](https://godotengine.org/download)
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Steamworks.NET](https://github.com/rlabrecque/Steamworks.NET) native libraries and managed DLLs
- A Steam App ID (for actual deployment/testing on Steam)

## Setup

1. **Clone the repository** 
```bash 
git clone https://github.com/alexfishy12/SteamMultiplayer.git
```

2. **Install dependencies**  
- Ensure you have Godot 4.4.1 (Mono) and .NET 8 SDK installed.
- Download and place the Steamworks.NET native libraries and managed DLLs in the `addons/Steamworks.NET` directory as structured in the project.

3. **Configure platform**  
- Set the `SteamworksTargetPlatform` property in your `.csproj` to match your build target (`win-x64`, `linux-x64`, or `osx-arm64`).

4. **Build the project**  
- Build the project in Godot using the build button:
  ```sh
  dotnet build
  ```

5. **Run the project**  
- Click play in the Godot editor.

## Project Structure

- `SteamMultiplayer.csproj` – Project configuration and build logic.
- `addons/Steamworks.NET/` – Contains Steamworks.NET managed and native libraries.
- `SteamworksManager.cs` – Initializes and manages the Steamworks.NET API.
- `LobbyManager.cs` – Manages lobby creation, joining, and Steam callbacks.

## Notes

- Native Steamworks libraries are automatically copied to the output and project root as needed for development and deployment.
- Ensure your Steam App ID is set up correctly for testing Steam features.
- For cross-platform builds, adjust the `SteamworksTargetPlatform` property and ensure the correct native libraries are present.
---