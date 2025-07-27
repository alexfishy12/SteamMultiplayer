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
    [Export] private Label lobbyNameLabel;
    [Export] private Button inviteFriendButton;
    [Export] private VBoxContainer playerList;
    [Export] private PackedScene playerListItemScene;
    [Export] private Button startGameButton;
    [Export] private Button quitLobbyButton;

    [ExportGroup("Joining")]
    [Export] private Control joinGamePanel;
    [Export] private Button searchForLobbiesButton;
    [Export] private VBoxContainer lobbyList;
    [Export] private PackedScene lobbyListItemScene;
    [Export] private Button backButton;

    public override void _Ready()
    {
        hostGameButton.Pressed += OnHostGameButtonPressed;
        joinGameButton.Pressed += OnJoinGameButtonPressed;
        quitGameButton.Pressed += () => GetTree().Quit();

        inviteFriendButton.Pressed += OnInviteFriendButtonPressed;
        startGameButton.Pressed += OnStartGameButtonPressed;

        quitLobbyButton.Pressed += OnQuitLobbyButtonPressed;
        backButton.Pressed += OnBackButtonPressed;
        searchForLobbiesButton.Pressed += OnSearchForLobbiesButtonPressed;


        LobbyManager.Instance.LobbyListUpdated += OnLobbyListUpdated;
        LobbyManager.Instance.LobbyJoined += OnLobbyJoined;
        LobbyManager.Instance.LobbyMembersUpdated += OnLobbyMembersUpdated;
        LobbyManager.Instance.LobbyLeft += OnLobbyLeft;
        GD.Print("MultiplayerMenu initialized successfully.");
    }

    private void OnLobbyListUpdated(List<Lobby> lobbies)
    {
        ClearLobbyList();
        foreach (Lobby lobby in lobbies)
        {
            LobbyListItem lobbyListItem = lobbyListItemScene.Instantiate<LobbyListItem>();
            GD.Print($"Lobby found: {lobby.Name} (ID: {lobby.Id})");
            lobbyListItem.LobbyId = lobby.Id;
            lobbyListItem.LobbyName = lobby.Name;
            lobbyList.AddChild(lobbyListItem);
        }
    }

    private void OnLobbyJoined(Lobby lobby)
    {
        lobbyNameLabel.Text = lobby.Name;
        joinGamePanel.Visible = false;
        mainMenuPanel.Visible = false;
        lobbyPanel.Visible = true;
    }

    private void OnLobbyMembersUpdated(List<LobbyMember> members)
    {
        ClearMemberList(); // Clear previous members
        GD.Print($"Updating lobby members UI...");
        foreach (LobbyMember member in members)
        {
            GD.Print($"Adding player to UI: {member}");
            PlayerListItem playerItem = playerListItemScene.Instantiate<PlayerListItem>();
            playerItem.PlayerName = member.Name;
            playerItem.playerId = member.Id.m_SteamID;
            playerItem.PlayerAvatar = member.Avatar != null ? ImageTexture.CreateFromImage(member.Avatar): null;
            playerList.AddChild(playerItem);

        }
        GD.Print("Lobby members UI updated.");
    }

    private void OnLobbyLeft(Lobby lobby)
    {
        GD.Print("Left lobby: " + lobby.Name);
        lobbyPanel.Visible = false;
        joinGamePanel.Visible = true;
        mainMenuPanel.Visible = false;
        LobbyManager.Instance.CurrentLobby = null; // Reset current lobby
        ClearLobbyList(); // Clear the lobby list when leaving
        ClearMemberList(); // Clear the member list when leaving
    }


    private void ClearLobbyList()
    {
        GD.Print("Clearing lobby list.");
        foreach (Node child in lobbyList.GetChildren())
        {
            child.QueueFree();
        }
    }

    private void ClearMemberList()
    {
        GD.Print("Clearing member list.");
        foreach (Node child in playerList.GetChildren())
        {
            child.QueueFree();
        }
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
        LobbyManager.Instance.LeaveCurrentLobby();
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
}
