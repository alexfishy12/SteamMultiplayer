using Godot;
using Steamworks;

public class LobbyMember
{
    public CSteamID Id { get; set; } = CSteamID.Nil;
    public string Name { get; set; } = "Anonymous";

    public Image Avatar { get; set; } = null;
    public bool IsHost { get; set; } = false;
    public int points { get; set; } = 0;
    public int Rank { get; set; } = 0;
    public int Kills { get; set; } = 0;
    public int Deaths { get; set; } = 0;
    public int Assists { get; set; } = 0;
    public int Ping { get; set; } = 0;

    public LobbyMember(CSteamID Id)
    {
        this.Id = Id;
    }

    public override string ToString()
    {
        return $"{Name} ({Id})";
    }
}
