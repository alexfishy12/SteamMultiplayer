using Godot;
using System;
using Steamworks;

public partial class SteamworksManager : Node
{
    public static SteamworksManager Instance { get; private set; }

    private AppId_t appId =  new(3879030);

    public override void _Ready()
    {
        if (Instance != null)
        {
            GD.PrintErr("An instance of SteamworksManager already exists. Only one instance is allowed.");
            return;
        }
        Instance = this;
        try
        {
            if (!SteamAPI.Init()) // initializes the Steam API
            {
                GD.PrintErr("Failed to initialize Steam API.");
                return;
            }
            if (!SteamAPI.IsSteamRunning()) // checks if Steam is running
            {
                GD.PrintErr("Steam is not running. Please start Steam to use the Steam API.");
                return;
            }
            if (SteamAPI.RestartAppIfNecessary(appId)) // restarts if game didn't start using Steam
            {
                GD.Print("Restarting app to ensure Steam API is initialized correctly.");
                GetTree().Quit();
                return;
            }
            // Steamworks has been initialized successfully
            GD.Print($"Steam API initialized successfully. App ID: {appId}");
            GD.Print($"Logged in as: {SteamFriends.GetPersonaName()}");
            GD.Print("SteamworksManager initialized successfully.");
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Error initializing Steam API: {ex.Message}");
        }
    }

    public override void _Process(double delta)
    {
        if (Instance == null) return;
        SteamAPI.RunCallbacks();
        MainThreadDispatcher.ExecutePending(); // process any actions queued for the main thread
    }

    public override void _ExitTree()
    {
        try
        {
            SteamAPI.Shutdown();
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Error shutting down Steam API: {ex.Message}");
        }
    }
}
