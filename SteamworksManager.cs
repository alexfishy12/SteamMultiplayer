using System;
using Godot;
using Steamworks;

namespace SteamMultiplayer;

public partial class SteamworksManager : Node
{
	public static SteamworksManager Instance { get; private set; }

	public override void _Ready()
	{
		if (Instance != null)
		{
			GD.PrintErr("An instance of SteamScript already exists. Only one instance is allowed.");
			return;
		}
		GD.Print(SteamAPI.IsSteamRunning() ? "Steam is running." : "Steam is not running. Please start Steam to use the Steam API.");
		try
		{
			if (!SteamAPI.Init())
			{
				GD.PrintErr("Failed to initialize Steam API.");
				return;
			}
			Instance = this;
			GD.Print("Steam API initialized successfully.");
			GD.Print($"Logged in as: {SteamFriends.GetPersonaName()}");
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