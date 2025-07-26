using Godot;

namespace SteamMultiplayer;

public partial class PlayerListItem : PanelContainer
{
	[Export] private Label nameLabel;
	[Export] private Label pingLabel;
	[Export] private Button kickButton;

	private string _playerName;
	public string PlayerName
	{
		get => _playerName;
		set
		{
			_playerName = value;
			nameLabel.Text = _playerName;
		}
	}

	private string _ping;
	public string Ping
	{
		get => _ping;
		set
		{
			_ping = value;
			UpdatePingUi(_ping);
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
		GD.Print($"Kicking player: {_playerName}");
		// Example: LobbyManager.Instance.KickPlayer(_playerId);
	}

	private void UpdatePingUi(string newPing)
	{
		pingLabel.Text = $"({_ping}ms)";
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