using Godot;
using System;
using Steamworks;
using System.Collections.Generic;
using System.Linq;

public partial class LobbyManager : Node2D
{
    public static LobbyManager Instance { get; private set; }

    private CallResult<LobbyCreated_t> m_LobbyCreated;
    private CallResult<LobbyMatchList_t> m_LobbyMatchList;
    private CallResult<LobbyEnter_t> m_LobbyEnter;
    private Callback<PersonaStateChange_t> m_PersonaStateChange;
    private Callback<LobbyChatUpdate_t> m_LobbyChatUpdate;

    public event Action<Lobby> LobbyLeft;
    public event Action<Lobby> LobbyJoined;
    public event Action<List<Lobby>> LobbyListUpdated;
    public event Action<List<LobbyMember>> LobbyMembersUpdated;
    public event Action<ChatMessage> ChatMessageReceived;

    public LobbyMember CurrentPlayer;
    public Lobby CurrentLobby;

    private Dictionary<CSteamID, Lobby> LobbyList = new();
    private Dictionary<CSteamID, LobbyMember> LobbyMemberList = new();


    public override void _Ready()
    {
        if (Instance != null)
        {
            GD.PrintErr("An instance of LobbyManager already exists. Only one instance is allowed.");
            return;
        }
        Instance = this;

        CurrentPlayer = new LobbyMember(SteamUser.GetSteamID());
        CurrentPlayer.Name = SteamFriends.GetPersonaName();


        int avatarFlag = 0; // SteamFriends.GetSmallFriendAvatar(CurrentPlayer.Id);
        if (avatarFlag == 0)
        {
            GD.Print("No avatar found for the current player.");
        }
        else
        {
            SteamUtils.GetImageSize(avatarFlag, out uint width, out uint height);
            byte[] avatarData = new byte[width * height * 4];
            bool v = SteamUtils.GetImageRGBA(avatarFlag, avatarData, (int)(width * height * 4));
            if (v)
            {
                Image avatar = Image.CreateFromData((int)width, (int)height, false, Image.Format.Rgba8, avatarData);
                CurrentPlayer.Avatar = avatar;
            }
            else
            {
                GD.PrintErr("Failed to get avatar image data.");
            }
        }
        
        m_PersonaStateChange = Callback<PersonaStateChange_t>.Create(OnPersonaStateChanged);
        m_LobbyChatUpdate = Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdate);

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
        CurrentLobby = new Lobby(lobbyId, CurrentPlayer.Id, CurrentPlayer.Name);
        CurrentPlayer.IsHost = true;
        CurrentLobby.PushChanges();
        GD.Print("Lobby created successfully with ID: " + CurrentLobby.Id);
        LobbyJoined?.Invoke(CurrentLobby);
        GetLobbyMembers();
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
            CSteamID LobbyId = SteamMatchmaking.GetLobbyByIndex(i);
            GD.Print($"Lobby {i}: ID = {LobbyId.m_SteamID}");
            CSteamID OwnerId = new CSteamID(ulong.Parse(SteamMatchmaking.GetLobbyData(LobbyId, nameof(Lobby.OwnerId))));
            string OwnerName = SteamMatchmaking.GetLobbyData(LobbyId, nameof(Lobby.OwnerName));
            LobbyList[LobbyId] = new Lobby(LobbyId, OwnerId, OwnerName);
        }

        if (result.m_nLobbiesMatching == 0)
        {
            GD.Print("No lobbies found. You may want to create one, or search again.");
        }

        LobbyListUpdated?.Invoke(LobbyList.Values.ToList());
    }

    public void JoinLobby(CSteamID lobbyId)
    {
        var call = SteamMatchmaking.JoinLobby(lobbyId);
        m_LobbyEnter = CallResult<LobbyEnter_t>.Create(OnLobbyEntered);
        m_LobbyEnter.Set(call);
    }

    private void OnLobbyEntered(LobbyEnter_t result, bool bIOFailure)
    {
        if (bIOFailure)
        {
            GD.PrintErr("Failed to join lobby due to IO failure.");
            return;
        }

        CSteamID LobbyId = new CSteamID(result.m_ulSteamIDLobby); // ulSteamIDLobby is the ID of the lobby we just joined
        CSteamID OwnerId = new CSteamID(ulong.Parse(SteamMatchmaking.GetLobbyData(LobbyId, nameof(Lobby.OwnerId))));
        string OwnerName = SteamMatchmaking.GetLobbyData(LobbyId, nameof(Lobby.OwnerName));
        CurrentLobby = new Lobby(LobbyId, OwnerId, OwnerName);

        GD.Print($"Joined lobby successfully: {CurrentLobby.Name} (ID: {CurrentLobby.Id})");

        LobbyJoined?.Invoke(CurrentLobby); // Here you can update the UI or notify other parts of your game that the lobby has been joined
        GetLobbyMembers(); // Fetch members of the lobby after joining
    }

    public void LeaveCurrentLobby()
    {
        if (CurrentLobby != null)
        {
            SteamMatchmaking.LeaveLobby(CurrentLobby.Id);
            LobbyMemberList.Clear();
        }
        LobbyLeft?.Invoke(CurrentLobby);
        CurrentLobby = null; // Clear the current lobby reference
        CurrentPlayer.IsHost = false;
    }

    public void GetLobbyMembers()
    {
        if (CurrentLobby == null)
        {
            GD.Print("No active lobby to get members from.");
            return;
        }

        GD.Print($"Getting members for lobby ID: {CurrentLobby.Id}");
        int numMembers = SteamMatchmaking.GetNumLobbyMembers(CurrentLobby.Id);

        GD.Print($"Number of members in lobby: {numMembers}");

        for (int i = 0; i < numMembers; i++)
        {
            try
            {
                CSteamID memberId = SteamMatchmaking.GetLobbyMemberByIndex(CurrentLobby.Id, i);

                if (!memberId.IsValid())
                {
                    GD.PrintErr($"Invalid memberId at index {i}");
                    continue;
                }

                if (!LobbyMemberList.ContainsKey(memberId))
                {
                    LobbyMemberList[memberId] = new LobbyMember(memberId);
                }

                GD.Print("Requesting user information for memberId: " + memberId.m_SteamID);
                if (!SteamFriends.RequestUserInformation(memberId, false))
                {
                    GD.Print("We already have all the details for memberId: " + memberId.m_SteamID);
                    LobbyMemberList[memberId].Name = SteamFriends.GetFriendPersonaName(memberId);
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Exception in GetLobbyMembers loop: {ex}");
            }
        }

        LobbyMembersUpdated?.Invoke(LobbyMemberList.Values.ToList());
    }

    private void OnLobbyChatUpdate(LobbyChatUpdate_t result)
    {
        try
        {
            if (result.m_ulSteamIDLobby != CurrentLobby.Id.m_SteamID) return;

            CSteamID userChangedId = new CSteamID(result.m_ulSteamIDUserChanged);
            CSteamID userMakingChangeId = new CSteamID(result.m_ulSteamIDMakingChange);

            LobbyMember changedMember = LobbyMemberList.ContainsKey(userChangedId) ? 
                LobbyMemberList[userChangedId] : 
                new LobbyMember(userChangedId);

            LobbyMember makingChangeMember = LobbyMemberList.ContainsKey(userMakingChangeId) ?
                LobbyMemberList[userMakingChangeId] :
                new LobbyMember(userMakingChangeId);

            GD.Print($"OnLobbyChatUpdate called for lobby ID: {result.m_ulSteamIDLobby}");

            switch ((EChatMemberStateChange)result.m_rgfChatMemberStateChange)
            {
                case EChatMemberStateChange.k_EChatMemberStateChangeEntered:
                    GD.Print($"{changedMember} has entered the lobby.");
                    GD.Print($"Requesting user information for memberId: {changedMember}");
                    if (!SteamFriends.RequestUserInformation(changedMember.Id, false))
                    {
                        GD.Print("We already have all the details for memberId: " + changedMember);
                        changedMember.Name = SteamFriends.GetFriendPersonaName(changedMember.Id);
                    }
                    LobbyMemberList[changedMember.Id] = changedMember;
                    break;
                case EChatMemberStateChange.k_EChatMemberStateChangeLeft:
                    GD.Print($"{changedMember} has left the lobby.");
                    LobbyMemberList.Remove(changedMember.Id);
                    break;
                case EChatMemberStateChange.k_EChatMemberStateChangeDisconnected:
                    GD.Print($"{changedMember} has disconnected from the lobby.");
                    LobbyMemberList.Remove(changedMember.Id);
                    break;
                case EChatMemberStateChange.k_EChatMemberStateChangeKicked:
                    GD.Print($"{changedMember} has been kicked from the lobby.");
                    LobbyMemberList.Remove(changedMember.Id);
                    break;
                case EChatMemberStateChange.k_EChatMemberStateChangeBanned:
                    GD.Print($"{changedMember} has been banned from the lobby.");
                    LobbyMemberList.Remove(changedMember.Id);
                    break;
                default:
                    GD.Print($"{changedMember} has left the lobby or changed state.");
                    break;
            }

            GetLobbyMembers();
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Exception in OnLobbyChatUpdate: {ex}");
        }
    }

    private void OnPersonaStateChanged(PersonaStateChange_t result)
    {
        GD.Print("OnPersonaStateChanged called.");
        try
        {
            CSteamID memberId = new CSteamID(result.m_ulSteamID);
            //if (!LobbyMemberList.ContainsKey(memberId))
            //    return;
            string personaName;

            if (memberId == SteamUser.GetSteamID())
            {
                personaName = SteamFriends.GetPersonaName();
            }
            else
            {
                personaName = SteamFriends.GetFriendPersonaName(memberId);
            }

            if (!LobbyMemberList.ContainsKey(memberId))
            {
                GD.Print($"Member {memberId} not found in LobbyMemberList, creating new entry.");
                LobbyMemberList[memberId] = new LobbyMember(memberId);
            }

            LobbyMemberList[memberId].Name = personaName;
            GD.Print($"PersonaStateChanged: Updated member '{personaName}' in LobbyMemberList.");
            LobbyMembersUpdated?.Invoke(LobbyMemberList.Values.ToList());
            
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Exception in OnPersonaStateChanged: {ex}");
            return;
        }
    }

    public bool SendChatMessage(string message)
    {
        if (CurrentLobby == null)
        {
            GD.PrintErr("No active lobby to send a chat message.");
            return false;
        }
        if (string.IsNullOrWhiteSpace(message))
        {
            GD.PrintErr("Cannot send an empty message.");
            return false;
        }
        bool chatSent = SteamMatchmaking.SendLobbyChatMsg(CurrentLobby.Id, message.ToUtf8Buffer(), message.Length + 1);
        if (chatSent)
        {
            GD.Print($"Chat message sent successfully: {message}");
            return true;
        }
        else
        {
            GD.Print($"Failed to send chat message: {message}");
            return false;
        }
    }

    public void OnLobbyChatReceived(LobbyChatMsg_t result)
    {
        if (result.m_ulSteamIDLobby != CurrentLobby.Id.m_SteamID)
        {
            GD.PrintErr("Received chat message for a different lobby.");
            return;
        }

        GD.Print($"Chat message received in lobby {CurrentLobby.Id}: {result.m_iChatID}");

        CSteamID senderId;
        EChatEntryType chatEntryType;
        byte[] data = new byte[4096];

        int messageLength = SteamMatchmaking.GetLobbyChatEntry(
            CurrentLobby.Id,
            (int)result.m_iChatID,
            out senderId, 
            data, 
            data.Length,
            out chatEntryType
        );
        if (messageLength <= 0)
        {
            GD.PrintErr("Failed to retrieve chat message.");
            return;
        }
        // Convert byte array to string
        string message = System.Text.Encoding.UTF8.GetString(data, 0, messageLength);
        
        ChatMessage chatMsg = new ChatMessage
        (
            senderId,
            SteamFriends.GetFriendPersonaName(senderId),
            message
        );

        ChatMessageReceived?.Invoke(chatMsg);
    }
}