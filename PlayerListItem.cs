using Godot;
using System;

public partial class PlayerListItem : PanelContainer
{
    [Export] public ulong playerId;
    [Export] private TextureRect playerAvatar;
    [Export] private Label nameLabel;
    [Export] private Label pingLabel;
    [Export] private Button kickButton;

    public Texture2D PlayerAvatar
    {
        get => playerAvatar.Texture;
        set
        {
            playerAvatar.Texture = value;
            if (value != null)
            {
                playerAvatar.Visible = true;
            }
            else
            {
                playerAvatar.Texture = new PlaceholderTexture2D();
                playerAvatar.Visible = false;
            }
        }
    }
    public string PlayerName
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
            pingLabel.Text = $"({value}ms)";
            UpdatePingUi(value);
        }
    }


    public override void _Ready()
    {
        kickButton.Pressed += OnKickButtonPressed;
    }

    private void OnKickButtonPressed()
    {
        // Logic to kick the player from the lobby
        // This could involve calling a method in your lobby manager or network manager
        GD.Print($"[EXPERIMENTAL] Kicking player: {nameLabel.Text}");
        // Example: LobbyManager.Instance.KickPlayer(_playerId);
        //LobbyManager.Instance.KickPlayer(_playerId);
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
