using Godot;
using Steamworks;
using System;

public partial class LobbyListItem : PanelContainer
{
    [Export] private Label nameLabel;
    [Export] private Label pingLabel;
    [Export] private Button joinButton;

    private CSteamID _lobbyId;
    public CSteamID LobbyId
    {
        get => _lobbyId;
        set
        {
            _lobbyId = value;
            nameLabel.Text = _lobbyId.ToString();
        }
    }

    public string LobbyName
    {
        get => nameLabel.Text;
        set
        {
            nameLabel.Text = value;
        }
    }

    public string Ping
    {
        get => pingLabel.Text;
        set
        {
            pingLabel.Text = value;
            UpdatePingUi(value);
        }
    }


    public override void _Ready()
    {
        joinButton.Pressed += OnJoinButtonPressed;
    }

    private void OnJoinButtonPressed()
    {
        LobbyManager.Instance.JoinLobby(_lobbyId);
    }

    private void UpdatePingUi(string newPing)
    {
        int pingValue = int.Parse(newPing);
        if (pingValue < 50)
        {
            pingLabel.AddThemeColorOverride("font_color", new Color(0, 1, 0)); // Green
        }
        else if (pingValue < 100)
        {
            pingLabel.AddThemeColorOverride("font_color", new Color(1, 1, 0)); // Yellow
        }
        else
        {
            pingLabel.AddThemeColorOverride("font_color", new Color(1, 0, 0)); // Red
        }
    }
}
