using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CustomNetworkManager : NetworkRoomManager
{
    public HashSet<string> bannedIPs = new();
    public bool showStartButton;

    #region Instance Managment
    public static new CustomNetworkManager singleton => NetworkManager.singleton as CustomNetworkManager;

    #endregion

    #region ServerHost
    public void StartHostAndLoadInitialScene()
    {
        StartHost();
        Debug.Log("Host started. Loading initial scene...");
    }
    #endregion

    #region Server Shutdown
    public void serverShutdownProcess()
    {
        // Check if we are running as a host (both server and client active)
        if (NetworkServer.active && NetworkClient.isConnected)
        {
            // This method stops both the server and client
            StopHost();
            Debug.Log("Host stopped, returning to MainMenu.");
        }
        // Additional checks
        else if (NetworkServer.active)
        {
            StopServer();
            Debug.Log("Server stopped.");
        }
        else if (NetworkClient.isConnected)
        {
            StopClient();
            Debug.Log("Client stopped.");
        }
    }

    #endregion

    #region Ready System
    public override void OnRoomServerPlayersReady()
    {
        // calling the base method calls ServerChangeScene as soon as all players are in Ready state.
        if (Utils.IsHeadless())
        {
            base.OnRoomServerPlayersReady();
        }
        else
        {
            showStartButton = true;
        }
    }
    /*
    public override void OnGUI()
    {
        base.OnGUI();

        if (allPlayersReady && showStartButton && GUI.Button(new Rect(150, 300, 120, 20), "START GAME"))
        {
            // set to false to hide it in the game scene
            showStartButton = false;

            // Check if the server is in the room scene
            if (SceneManager.GetActiveScene().name == "Lobby")
            {
                ServerChangeScene(GameplayScene);
            }
            else
            {
                Debug.LogError("Server is not in the room scene. Cannot change scene.");
            }
        }
    } */
    #endregion

    #region Server Management

    // Handle Clients that disconnect
    public override void OnClientDisconnect()
    {
        base.OnClientDisconnect();
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
    }

    // Return mouse control when returning to the lobby
    public override void OnRoomClientSceneChanged()
    {
        base.OnRoomClientSceneChanged();

        if(SceneManager.GetActiveScene().name == "Lobby")
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
        }
    }

    // Check if player is banned from lobby etc.
    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        /* Get the IP address from the connection
        string ipAddress = GetIPAddress(conn);

        if (bannedIPs.Contains(ipAddress))
        {
            conn.Disconnect();
            return;
        }
        */
        base.OnServerConnect(conn);
    }

    // Handle Scene Changes & Player Data
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        // Get the saved player data
        PlayerData playerData = PlayerManager.LoadPlayerData(conn.connectionId);
        playerData ??= new PlayerData();

        // increment the index before adding the player, so first player starts at 1
        clientIndex++;

        if (Utils.IsSceneActive(RoomScene))
        {
            allPlayersReady = false;

            //Debug.Log("NetworkRoomManager.OnServerAddPlayer playerPrefab: {roomPlayerPrefab.name}");

            GameObject newRoomGameObject = OnRoomServerCreateRoomPlayer(conn);
            if (newRoomGameObject == null)
                newRoomGameObject = Instantiate(roomPlayerPrefab.gameObject, Vector3.zero, Quaternion.identity);

            NetworkServer.AddPlayerForConnection(conn, newRoomGameObject);
        }
        if (Regex.IsMatch(SceneManager.GetActiveScene().name, @"Stage \d+"))
        {
            // Re-instantiate the player using the saved data
            Transform startPos = GetStartPosition();
            GameObject player = startPos != null
                ? Instantiate(playerPrefab, startPos.position, startPos.rotation)
                : Instantiate(playerPrefab);

            player.name = $"{playerPrefab.name} [connId={conn.connectionId}]";
            NetworkServer.AddPlayerForConnection(conn, player);

            // Apply the saved data to the player
            PlayerManager playerComponent = player.GetComponent<PlayerManager>();

            playerComponent.maxHealth = playerData.maxHealthData;
            playerComponent.currencyBalance = playerData.currencyBalanceData;
            playerComponent.currentHealth = playerData.currentHealthData;
            playerComponent.LoadInventory(playerData);
        }
        if (SceneManager.GetActiveScene().name == "Shop")
        {
            // Re-instantiate the player using the saved data
            Transform startPos = GetStartPosition();
            GameObject player = startPos != null
                ? Instantiate(playerPrefab, startPos.position, startPos.rotation)
                : Instantiate(playerPrefab);

            player.name = $"{playerPrefab.name} [connId={conn.connectionId}]";
            NetworkServer.AddPlayerForConnection(conn, player);

            // Apply the saved data to the player
            PlayerManager playerComponent = player.GetComponent<PlayerManager>();

            playerComponent.maxHealth = playerData.maxHealthData;
            playerComponent.currencyBalance = playerData.currencyBalanceData;
            playerComponent.currentHealth = playerData.currentHealthData;
            playerComponent.LoadInventory(playerData);
        }
        if (SceneManager.GetActiveScene().name == "Victory")
        {
            // Re-instantiate the player using the saved data
            Transform startPos = GetStartPosition();
            GameObject player = startPos != null
                ? Instantiate(playerPrefab, startPos.position, startPos.rotation)
                : Instantiate(playerPrefab);

            player.name = $"{playerPrefab.name} [connId={conn.connectionId}]";
            NetworkServer.AddPlayerForConnection(conn, player);

            // Apply the saved data to the player
            PlayerManager playerComponent = player.GetComponent<PlayerManager>();

            playerComponent.maxHealth = playerData.maxHealthData;
            playerComponent.currencyBalance = playerData.currencyBalanceData;
            playerComponent.currentHealth = playerData.currentHealthData;
            playerComponent.LoadInventory(playerData);
        }
        if (SceneManager.GetActiveScene().name == "Gameover")
        {
            // Re-instantiate the player using the saved data
            Transform startPos = GetStartPosition();
            GameObject player = startPos != null
                ? Instantiate(playerPrefab, startPos.position, startPos.rotation)
                : Instantiate(playerPrefab);

            player.name = $"{playerPrefab.name} [connId={conn.connectionId}]";
            NetworkServer.AddPlayerForConnection(conn, player);

            // Apply the saved data to the player
            PlayerManager playerComponent = player.GetComponent<PlayerManager>();

            playerComponent.maxHealth = playerData.maxHealthData;
            playerComponent.currencyBalance = playerData.currencyBalanceData;
            playerComponent.currentHealth = playerData.currentHealthData;
            playerComponent.LoadInventory(playerData);
        }
    }

    // Set the Level Manager to be active so it can be DDOL
    public override void OnServerSceneChanged(string sceneName)
    {
        base.OnServerSceneChanged(sceneName);

        // Level Manger is Located in the Lobby Scene
        GameObject.Find("Level Manager").SetActive(true);
    }
    #endregion

    #region Debug
    public override void OnStopServer()
    {
        Debug.Log("Server stopping...");
        base.OnStopServer();
    }

    public override void OnStopHost()
    {
        Debug.Log("Host stopping...");
        base.OnStopHost();
    }

    public override void OnStopClient()
    {
        Debug.Log("Client stopping...");
        base.OnStopClient();
    }
    #endregion

}