using Mirror;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class CustomNetworkRoomPlayer : NetworkRoomPlayer
{
    private CustomNetworkManager networkManager;

    public GameObject playerJoinedPrefab;

    public Transform ui;
    public Transform playerList;

    public GameObject readyUpPrefabRef;
    public GameObject cancelPrefabRef;
    public GameObject startPrefabRef;
    public GameObject kickPrefab;
    public GameObject banPrefab;

    private GameObject instantiatedReadyUpPrefab;
    private GameObject instantiatedCancelPrefab;
    private GameObject instantiatedStartPrefab;
    private GameObject instantiatedPlayerJoin;

    public override void OnStartLocalPlayer()
    {
        networkManager = CustomNetworkManager.singleton;

        base.OnStartLocalPlayer();

        if (SceneManager.GetActiveScene().name == "Lobby")
        {
            StartCoroutine(WaitForGameSelection());
        }

        IEnumerator WaitForGameSelection()
        {
            while (true)
            {
                var uiGameObject = GameObject.Find("UI");
                if (uiGameObject != null && uiGameObject.activeInHierarchy)
                {
                    // Initialize All Buttons
                    var uiRef = GameObject.Find("UI").transform;

                    instantiatedReadyUpPrefab = Instantiate(readyUpPrefabRef, uiRef);
                    instantiatedCancelPrefab = Instantiate(cancelPrefabRef, uiRef);

                    instantiatedReadyUpPrefab.GetComponent<Button>().onClick.AddListener(ReadyUp);
                    instantiatedCancelPrefab.GetComponent<Button>().onClick.AddListener(Cancel);

                    if (isServer)
                    {
                        instantiatedStartPrefab = Instantiate(startPrefabRef, uiRef);
                        instantiatedStartPrefab.GetComponent<Button>().onClick.AddListener(StartGame);
                        instantiatedStartPrefab.SetActive(false);
                    }

                    // Make sure button is disabled
                    instantiatedCancelPrefab.SetActive(false);

                    // Add joined player to player list
                    var playerJoinedList = GameObject.Find("Players Connected").transform;
                    playerJoinedPrefab.GetComponent<TMP_Text>().text = ($"Player {index + 1}");
                    instantiatedPlayerJoin = Instantiate(playerJoinedPrefab, playerJoinedList);

                    if (((isServer && index > 0) || isServerOnly))
                    {
                        var instantiatedkickPrefab = Instantiate(kickPrefab, instantiatedPlayerJoin.transform);
                        var instantiatedbanPreafab = Instantiate(banPrefab, instantiatedPlayerJoin.transform);
                        instantiatedkickPrefab.GetComponent<Button>().onClick.AddListener(KickPlayer);
                        instantiatedbanPreafab.GetComponent<Button>().onClick.AddListener(BanPlayer);
                    }

                    break;
                }
                yield return null;
            }
        }
    }

    #region Lobby System
    public void ReadyUp()
    {
        if(NetworkClient.active && isLocalPlayer)
        {
            CmdChangeReadyState(true);
            instantiatedReadyUpPrefab.SetActive(false);
            instantiatedCancelPrefab.SetActive(true);

            // Only allow the host to start game
            CheckAndShowStartButton();
        }
    }

    // This method starts the coroutine if conditions are met
    void CheckAndShowStartButton()
    {
        if (isServer)
        {
            StartCoroutine(WaitToShowStartButton());
        }
    }

    IEnumerator WaitToShowStartButton()
    {
        // Wait until showStartButton is true
        while (!networkManager.showStartButton)
        {
            yield return null;
        }
        instantiatedStartPrefab.SetActive(true);
    }

    public void Cancel()
    {
        if (NetworkClient.active && isLocalPlayer)
        {
            CmdChangeReadyState(false);
            instantiatedCancelPrefab.SetActive(false);
            instantiatedReadyUpPrefab.SetActive(true);

            if (isServer)
            {
                instantiatedStartPrefab.SetActive(false);
            }
        }
    }

    public void StartGame()
    {
        networkManager.showStartButton = false;
        networkManager.ServerChangeScene(networkManager.GameplayScene);
    }

    public void KickPlayer()
    {
        // Disconnect
        GetComponent<NetworkIdentity>().connectionToClient.Disconnect();
    }

    public void BanPlayer()
    {
        // Disconnect
        GetComponent<NetworkIdentity>().connectionToClient.Disconnect();
        /* Ban
        if (!networkManager.bannedIPs.Contains(player))
        {
            bannedIPs.Add(player);
        }
        */
    }
    #endregion
}