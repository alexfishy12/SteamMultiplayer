using Godot;
using System;
using Steamworks;
using System.Collections.Generic;

public partial class LobbyManager : Node2D
{
    public static LobbyManager Instance { get; private set; }

    private CallResult<LobbyCreated_t> m_LobbyCreated;
    private CallResult<LobbyMatchList_t> m_LobbyMatchList;
    private CallResult<LobbyEnter_t> m_LobbyEnter;
    private CallResult<PersonaStateChange_t> m_PersonaStateChange;

    public event Action<Lobby> LobbyLeft;
    public event Action<Lobby> LobbyJoined;
    public event Action<List<Lobby>> LobbyListUpdated;
    public event Action<List<LobbyMember>> LobbyMembersUpdated;

    public LobbyMember CurrentPlayer;
    public Lobby CurrentLobby;

    private List<Lobby> LobbyList = new();
    private List<LobbyMember> LobbyMemberList = new();


    public override void _Ready()
    {
        if (Instance != null)
        {
            GD.PrintErr("An instance of LobbyManager already exists. Only one instance is allowed.");
            return;
        }
        Instance = this;

        CurrentPlayer = new LobbyMember(SteamUser.GetSteamID(), SteamFriends.GetPersonaName());

        GD.Print("LobbyManager initialized successfully.");
    }
    public void CreateLobby()
    {
        var call = SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, 4);
        m_LobbyCreated = CallResult<LobbyCreated_t>.Create(OnLobbyCreated);
        m_LobbyCreated.Set(call);
    }

    private void OnLobbyCreated(LobbyCreated_t result, bool bIOFailure)
    {
        if (bIOFailure || result.m_eResult != EResult.k_EResultOK)
        {
            GD.PrintErr("Failed to create lobby." + result.m_eResult);
            return;
        }

        CSteamID lobbyId = new CSteamID(result.m_ulSteamIDLobby);
        CurrentLobby = new Lobby(lobbyId, $"{CurrentPlayer.Name}'s lobby");
        GD.Print("Lobby created successfully with ID: " + CurrentLobby.Id);
        LobbyJoined?.Invoke(CurrentLobby);
    }

    public void SearchLobbies()
    {
        // optionally set filters here, e.g. SteamMatchimaking.AddRequestLobbyListStringFilter("game_mode", "deathmatch", ELobbyComparison.k_ELobbyComparisonEqual);
        var call = SteamMatchmaking.RequestLobbyList();
        m_LobbyMatchList = CallResult<LobbyMatchList_t>.Create(OnLobbyMatchList);
        m_LobbyMatchList.Set(call);
    }

    private void OnLobbyMatchList(LobbyMatchList_t result, bool bIOFailure)
    {
        if (bIOFailure)
        {
            GD.PrintErr("Lobby search failed due to IO failure.");
            return;
        }

        LobbyList.Clear(); // Clear previous results

        GD.Print($"Found {result.m_nLobbiesMatching} lobbies.");
        for (int i = 0; i < result.m_nLobbiesMatching; i++)
        {
            CSteamID lobbyId = SteamMatchmaking.GetLobbyByIndex(i);
            GD.Print($"Lobby {i}: ID = {lobbyId.m_SteamID}");

            LobbyList.Add(new Lobby(lobbyId, SteamMatchmaking.GetLobbyData(lobbyId, "name")));
        }

        if (result.m_nLobbiesMatching == 0)
        {
            GD.Print("No lobbies found. You may want to create one, or search again.");
        }

        LobbyListUpdated?.Invoke(LobbyList);
    }

    public void JoinLobby(CSteamID lobbyId)
    {
        var call = SteamMatchmaking.JoinLobby(lobbyId);
        m_LobbyEnter = CallResult<LobbyEnter_t>.Create(OnLobbyEntered);
        m_LobbyEnter.Set(call);
    }

    public void LeaveCurrentLobby()
    {
        SteamMatchmaking.LeaveLobby(CurrentLobby.Id);
        LobbyLeft?.Invoke(CurrentLobby);
        CurrentLobby = null; // Clear the current lobby reference
    }

    private void OnLobbyEntered(LobbyEnter_t result, bool bIOFailure)
    {
        if (bIOFailure)
        {
            GD.PrintErr("Failed to join lobby due to IO failure.");
            return;
        }

        CSteamID _lobbyId = new CSteamID(result.m_ulSteamIDLobby); // ulSteamIDLobby is the ID of the lobby we just joined
        GD.Print($"Successfully joined lobby with ID: {_lobbyId}");
        CurrentLobby = new Lobby(_lobbyId, SteamMatchmaking.GetLobbyData(_lobbyId, "name"));
        CurrentLobby.OwnerId = SteamMatchmaking.GetLobbyOwner(_lobbyId);
        CurrentLobby.OwnerName = SteamFriends.GetFriendPersonaName(CurrentLobby.OwnerId);

        LobbyJoined?.Invoke(CurrentLobby); // Here you can update the UI or notify other parts of your game that the lobby has been joined
        GetLobbyMembers(); // Fetch members of the lobby after joining
    }

    public void GetLobbyMembers()
    {
        if (CurrentLobby == null) 
        { 
            GD.Print("No active lobby to get members from.");
            return;
        }
        LobbyMemberList.Clear(); // Clear previous members


        GD.Print($"Getting members for lobby ID: {CurrentLobby.Id}");
        int numMembers = SteamMatchmaking.GetNumLobbyMembers(CurrentLobby.Id);

        GD.Print($"Number of members in lobby: {numMembers}");

        for (int i = 0; i < numMembers; i++)
        {
            CSteamID memberId = SteamMatchmaking.GetLobbyMemberByIndex(CurrentLobby.Id, i);
            string memberPersonaName = SteamFriends.GetFriendPersonaName(memberId);
            LobbyMemberList.Add(new LobbyMember(memberId, memberPersonaName));
        }

        LobbyMembersUpdated?.Invoke(LobbyMemberList);
    }

    //public void KickPlayer(ulong playerId)
    //{
    //    if (CurrentLobby == null)
    //    {
    //        GD.PrintErr("No active lobby to kick player from.");
    //        return;
    //    }
    //    CSteamID memberId = new CSteamID(playerId);
    //    if (SteamMatchmaking.KickMemberFromLobby(CurrentLobby.Id, memberId))
    //    {
    //        GD.Print($"Successfully kicked player with ID: {playerId} from lobby {CurrentLobby.Id}");
    //        GetLobbyMembers(); // Refresh the member list after kicking
    //    }
    //    else
    //    {
    //        GD.PrintErr($"Failed to kick player with ID: {playerId} from lobby {CurrentLobby.Id}");
    //    }
    //}

    private void OnPersonaStateChanged(PersonaStateChange_t result, bool bIOFailure)
    {
        if (bIOFailure)
        {
            GD.PrintErr("Failed to get persona state due to IO failure.");
            return;
        }
        CSteamID memberId = new CSteamID(result.m_ulSteamID);
        GD.Print($"Persona state changed for member ID: {memberId.m_SteamID}");
        GD.Print($"Member Name: {SteamFriends.GetFriendPersonaName(memberId)}");

    }
}
