using Godot;
using System;
using System.Collections.Generic;

public partial class MultiplayerMenu : Control
{
    [ExportGroup("Main")]
    [Export] private Control mainMenuPanel;
    [Export] private Button hostGameButton;
    [Export] private Button joinGameButton;
    [Export] private Button quitGameButton;

    [ExportGroup("Lobby")]
    [Export] private Control lobbyPanel;
    [Export] private Button inviteFriendButton;
    [Export] private VBoxContainer playerListContainer;
    [Export] private PackedScene playerListItemScene;
    [Export] private Button startGameButton;
    [Export] private Button quitLobbyButton;

    [ExportGroup("Joining")]
    [Export] private Control joinGamePanel;
    [Export] private Button searchForLobbiesButton;
    [Export] private VBoxContainer lobbyListContainer;
    [Export] private PackedScene lobbyListItemScene;
    [Export] private Button backButton;

    private List<LobbyListItem> lobbyListItems = new();

    public override void _Ready()
    {
        hostGameButton.Pressed += OnHostGameButtonPressed;
        joinGameButton.Pressed += OnJoinGameButtonPressed;

        inviteFriendButton.Pressed += OnInviteFriendButtonPressed;
        startGameButton.Pressed += OnStartGameButtonPressed;

        quitLobbyButton.Pressed += OnQuitLobbyButtonPressed;
        backButton.Pressed += OnBackButtonPressed;
        searchForLobbiesButton.Pressed += OnSearchForLobbiesButtonPressed;
        LobbyManager.Instance.LobbyFound += OnLobbyFound;
        LobbyManager.Instance.LobbyJoined += OnLobbyJoined;
        GD.Print("MultiplayerMenu initialized successfully.");
    }

    private void OnLobbyJoined(ulong lobbyId)
    {
        lobbyPanel.Visible = true;
        joinGamePanel.Visible = false;
        mainMenuPanel.Visible = false;
    }

    private void OnHostGameButtonPressed()
    {
        GD.Print("Host Game button pressed.");
        // Here you would typically call a method to create a lobby or start hosting
        lobbyPanel.Visible = true;
        joinGamePanel.Visible = false;
        mainMenuPanel.Visible = false;
        LobbyManager.Instance.CreateLobby();
    }

    private void OnJoinGameButtonPressed()
    {
        GD.Print("Join Game button pressed.");
        // Here you would typically call a method to search for lobbies and display them
        lobbyPanel.Visible = false;
        joinGamePanel.Visible = true;
        mainMenuPanel.Visible = false;
    }

    private void OnInviteFriendButtonPressed()
    {
        GD.Print("Invite Friend button pressed.");
        // Logic to invite a friend to the lobby
        // This could involve opening a friend list or sending an invite through Steam
    }

    private void OnStartGameButtonPressed()
    {
        GD.Print("Start Game button pressed.");
        // Logic to start the game, such as transitioning to the game scene
        // This could involve checking if all players are ready
    }

    private void OnQuitLobbyButtonPressed()
    {
        GD.Print("Quit button pressed.");
        // Logic to quit the game or return to the main menu
        lobbyPanel.Visible = false;
        joinGamePanel.Visible = false;
        mainMenuPanel.Visible = true;

        // reset lobby manager instance?
    }

    private void OnBackButtonPressed()
    {
        GD.Print("Back button pressed.");
        // Logic to return to the previous menu or main menu
        joinGamePanel.Visible = false;
        lobbyPanel.Visible = false;
        mainMenuPanel.Visible = true;
    }

    private void OnSearchForLobbiesButtonPressed()
    {
        GD.Print("Search for Lobbies button pressed.");
        // Logic to search for available lobbies
        // This could involve calling a method in LobbyManager to fetch and display lobbies
        ClearLobbyList(); // Clear previous results
        LobbyManager.Instance.SearchLobbies();
    }

    private void OnLobbyFound(ulong lobbyId)
    {
        // get lobby ping and name from lobbyId
        GD.Print($"Lobby found with ID: {lobbyId}");
        var lobbyListItem = lobbyListItemScene.Instantiate<LobbyListItem>();
        lobbyListItem.LobbyId = new Steamworks.CSteamID(lobbyId);
        // Assuming you have a method to get the ping for the lobby
        lobbyListItem.Ping = "100"; // Replace with actual ping retrieval logic
        lobbyListContainer.AddChild(lobbyListItem);
    }

    private void ClearLobbyList()
    {
        GD.Print("Clearing lobby list.");
        foreach (var item in lobbyListItems)
        {
            item.QueueFree();
        }
        lobbyListItems.Clear();
    }
}
