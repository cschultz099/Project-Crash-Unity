using Epic.OnlineServices.Lobby;
using UnityEngine;
using System.Collections.Generic;
using Mirror;
using TMPro;

public class LobbySystem : EOSLobby
{
    [Header("UI References")]
    public TMP_Text lobbyName;
    public GameObject lobbyItemPrefab;
    public Transform lobbyListParent;

    [Header("Lobby Data")]
    private List<LobbyDetails> foundLobbies = new List<LobbyDetails>();

    //register events
    private void OnEnable()
    {
        //subscribe to events
        CreateLobbySucceeded += OnCreateLobbySuccess;
        JoinLobbySucceeded += OnJoinLobbySuccess;
        FindLobbiesSucceeded += OnFindLobbiesSuccess;
        LeaveLobbySucceeded += OnLeaveLobbySuccess;
    }

    //deregister events
    private void OnDisable()
    {
        //unsubscribe from events
        CreateLobbySucceeded -= OnCreateLobbySuccess;
        JoinLobbySucceeded -= OnJoinLobbySuccess;
        FindLobbiesSucceeded -= OnFindLobbiesSuccess;
        LeaveLobbySucceeded -= OnLeaveLobbySuccess;
    }

    //when the lobby is successfully created, start the host
    private void OnCreateLobbySuccess(List<Attribute> attributes)
    {
        CustomNetworkManager.singleton.StartHost();
    }

    //when the user joined the lobby successfully, set network address and connect
    private void OnJoinLobbySuccess(List<Attribute> attributes)
    {
        NetworkManager netManager = CustomNetworkManager.singleton;
        netManager.networkAddress = attributes.Find((x) => x.Data.Key == hostAddressKey).Data.Value.AsUtf8;
        netManager.StartClient();
    }

    //callback for FindLobbiesSucceeded
    private void OnFindLobbiesSuccess(List<LobbyDetails> lobbiesFound)
    {
        foundLobbies = lobbiesFound;

        PopulateLobbyList();
    }

    //when the lobby was left successfully, stop the host/client
    private void OnLeaveLobbySuccess()
    {
        NetworkManager netManager = GetComponent<NetworkManager>();
        netManager.StopHost();
        netManager.StopClient();
    }

    public void LobbyCreate()
    {
        CreateLobby(4, LobbyPermissionLevel.Publicadvertised, false, new AttributeData[] { new AttributeData { Key = AttributeKeys[0], Value = lobbyName.text }, });
    }

    public void FindLobby()
    {
        FindLobbies();
    }

    public void LobbyLeave()
    {
        LeaveLobby();
    }

    void PopulateLobbyList()
    {
        foreach (Transform child in lobbyListParent)
        {
            Destroy(child.gameObject);
        }

        foreach(LobbyDetails lobby in foundLobbies)
        {
            GameObject item = Instantiate(lobbyItemPrefab, lobbyListParent);
            LobbyItem lobbyItem = item.GetComponent<LobbyItem>();
            if(lobbyItem != null)
            {
                Attribute lobbyNameAttribute;
                lobby.CopyAttributeByKey(new LobbyDetailsCopyAttributeByKeyOptions { AttrKey = AttributeKeys[0] }, out lobbyNameAttribute);
                string lobbyName = lobbyNameAttribute.Data.Value.AsUtf8;
                lobbyItem.Setup(lobbyName, () => { JoinLobby(lobby, AttributeKeys); });
            }
        }
    }
}
