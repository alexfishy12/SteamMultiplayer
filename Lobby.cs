using Steamworks;

public class Lobby
{
    public CSteamID Id { get; set; }
    public CSteamID OwnerId { get; set; }
    public string OwnerName { get; set; }
    public string Name { get; set; }

    public Lobby(CSteamID lobbyId, CSteamID OwnerId, string OwnerName)
    {
        this.Id = lobbyId;
        this.OwnerId = OwnerId;
        this.OwnerName = OwnerName;
        this.Name = $"{OwnerName}'s Lobby";
    }

    // pushes the current state of the lobby to Steam
    public void PushChanges()
    {
        SteamMatchmaking.SetLobbyData(Id, nameof(OwnerId), OwnerId.m_SteamID.ToString());
        SteamMatchmaking.SetLobbyData(Id, nameof(OwnerName), OwnerName);
        SteamMatchmaking.SetLobbyData(Id, nameof(Name), Name);
    }

}
