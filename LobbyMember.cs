using Steamworks;

public class LobbyMember
{
    public CSteamID Id { get; set; }
    public string Name { get; set; }
    public int points { get; set; }
    public int Rank { get; set; }
    public int Kills { get; set; }
    public int Deaths { get; set; }
    public int Assists { get; set; }
    public int Ping { get; set; }
    public ulong CreatedAt { get; set; }
    public ulong UpdatedAt { get; set; }


    public LobbyMember(CSteamID Id, string Name)
    {
        this.Id = Id;
        this.Name = Name;
    }


}
