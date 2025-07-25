using Godot;
using System;
using Steamworks;

public partial class LobbyManager : Node2D
{
    public static LobbyManager Instance { get; private set; }

    private CallResult<LobbyCreated_t> m_LobbyCreated;
    private CallResult<LobbyMatchList_t> m_LobbyMatchList;
    private CallResult<LobbyEnter_t> m_LobbyEnter;
    private CallResult<PersonaStateChange_t> m_PersonaStateChange;

    [Signal] public delegate void LobbyFoundEventHandler(ulong lobbyId);
    [Signal] public delegate void LobbyJoinedEventHandler(ulong lobbyId);
    private CSteamID _LobbyId;

    public override void _Ready()
    {
        if (Instance != null)
        {
            GD.PrintErr("An instance of LobbyManager already exists. Only one instance is allowed.");
            return;
        }
        Instance = this;
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

        _LobbyId = new CSteamID(result.m_ulSteamIDLobby);
        GD.Print("Lobby created successfully with ID: " + _LobbyId);
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

        GD.Print($"Found {result.m_nLobbiesMatching} lobbies.");
        for (int i = 0; i < result.m_nLobbiesMatching; i++)
        {
            var lobbyId = SteamMatchmaking.GetLobbyByIndex(i);
            GD.Print($"Lobby {i}: ID = {lobbyId.m_SteamID}");


            EmitSignal(SignalName.LobbyFound, lobbyId.m_SteamID);
        }

        if (result.m_nLobbiesMatching == 0)
        {
            GD.Print("No lobbies found. You may want to create one, or search again.");
        }
    }

    public void JoinLobby(CSteamID lobbyId)
    {
        var call = SteamMatchmaking.JoinLobby(lobbyId);
        m_LobbyEnter = CallResult<LobbyEnter_t>.Create(OnLobbyEntered);
        m_LobbyEnter.Set(call);
        if (true)
        {
            GD.Print("Joined lobby successfully.");
        }
        else
        {
            GD.PrintErr("Failed to join lobby.");
        }
    }

    private void OnLobbyEntered(LobbyEnter_t result, bool bIOFailure)
    {
        if (bIOFailure)
        {
            GD.PrintErr("Failed to join lobby due to IO failure.");
            return;
        }

        _LobbyId = new CSteamID(result.m_ulSteamIDLobby);
        GD.Print($"Successfully joined lobby with ID: {_LobbyId}");
        EmitSignal(SignalName.LobbyJoined, _LobbyId.m_SteamID);
        GetLobbyMembers();
        // Here you can update the UI or notify other parts of your game that the lobby has been joined
    }

    public void GetLobbyMembers()
    {
        if (!_LobbyId.IsValid()) 
        { 
            GD.Print("No active lobby to get members from.");
            return;
        }

        GD.Print($"Getting members for lobby ID: {_LobbyId}");

        int numMembers = SteamMatchmaking.GetNumLobbyMembers(_LobbyId);

        GD.Print($"Number of members in lobby: {numMembers}");

        for (int i = 0; i < numMembers; i++)
        {
            CSteamID memberId = SteamMatchmaking.GetLobbyMemberByIndex(_LobbyId, i);

            SteamFriends.RequestUserInformation(memberId, false);

            string memberPersonaName = SteamFriends.GetFriendPersonaName(memberId);
            //EPersonaState memberPersonaState = SteamFriends.GetFriendPersonaState(memberId);


            GD.Print($"Member {i}: ID = {memberId.m_SteamID}, Name = {memberPersonaName}");


        }
    }

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
