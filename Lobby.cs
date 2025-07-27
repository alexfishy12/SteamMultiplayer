using Steamworks;

public class Lobby
{
    public CSteamID Id { get; set; }
    public string Name { get; set; }
    public CSteamID OwnerId { get; set; }
    public string OwnerName { get; set; }
    public int MaxMembers { get; set; }
    public int CurrentMembers { get; set; }
    public int Ping { get; set; }
    public string GameMode { get; set; }
    public string Map { get; set; }
    public ulong CreatedAt { get; set; }
    public ulong UpdatedAt { get; set; }
    public Godot.Collections.Array Members { get; set; }


    public Lobby(CSteamID Id, string Name)
    {
        this.Id = Id;
        this.Name = Name;
        Members = new Godot.Collections.Array();
    }


}
