using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    public MainCampaign mainCampaignManager;
    public EternalQuest eternalQuestManager;

    #region Singleton
    public static GameManager Singleton { get; private set; }
    #endregion

    #region State Management
    private enum GameState { Stage, Shop }
    private GameState gameState = GameState.Stage;
    private enum GameMode { MainCampaign, EternalQuest }
    private GameMode gameMode;
    [HideInInspector]
    public bool InShop => gameState == GameState.Shop;
    [HideInInspector]
    public bool InStage => gameState == GameState.Stage;
    private bool isSceneChanging = false;
    private bool allPlayersReady = false;
    #endregion

    #region Stage Management
    [Header("Stage Management")]
    [ReadOnly]
    public int currentStage = 1;
    private string lastStageLoaded = "";
    #endregion

    #region Enemy Management
    [Header("Enemy Management")]
    [ReadOnly] public int enemiesAliveCounter = 0;
    public List<GameObject> enemiesAlive = new();
    [HideInInspector] public bool EnemiesCleared => enemiesAlive.Count == 0;
    #endregion

    private void Awake()
    {
        if (Singleton == null)
        {
            Singleton = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        mainCampaignManager = GetComponent<MainCampaign>();
        eternalQuestManager = GetComponent<EternalQuest>();

        SceneManager.sceneLoaded += OnSceneLoaded;
        isSceneChanging = false;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        enemiesAlive.Clear();

        if (SceneManager.GetActiveScene().name == "Lobby")
        {
            // Reset to Stage so the game is playable again when returning to the lobby
            gameState = GameState.Stage;
            currentStage = 1;
        }
        isSceneChanging = false;

        if (SceneManager.GetActiveScene().name != "MainMenu" && SceneManager.GetActiveScene().name != "Lobby")
        {
            LevelAudioManager.Instance.StopMusic();
        }
    }

    void OnDisable()
    {
        gameState = GameState.Stage;
    }

    void Update()
    {
        if (isServer)
        {
            if (allPlayersReady)
            {
                ProceedToNextScene();
            }
            if (enemiesAlive.Any())
            {
                CheckEnemiesCleared();
            }
            if (IsSinglePlayerMode() && IsSinglePlayerDead())
            {
                //KillAllEnemies();
            }
            if (IsAllPlayersDead())
            {
                //KillAllEnemies();
            }
        }
    }

    #region Enemies

    [Server]
    public void KillAllEnemies()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            NetworkServer.Destroy(enemy);
        }
    }

    [Server]
    public void CheckEnemiesCleared()
    {
        enemiesAlive.RemoveAll(item => item == null);
        enemiesAliveCounter = enemiesAlive.Count;

        if (enemiesAlive.Count == 0)
        {
            ReviveAllPlayers();
            NotifyPlayersEnemiesCleared();
        }
    }

    /*
    [Server]
    private void SpawnAtRandomPoint(GameObject prefab, float healthMultiplier, float damageMultiplier)
    {
        GameObject randomSpawnPoint = randomEnemySpawns[UnityEngine.Random.Range(0, randomEnemySpawns.Length)];
        Vector3 spawnPosition = randomSpawnPoint.transform.position;
        GameObject spawnedEntity = Instantiate(prefab, spawnPosition, Quaternion.identity);
        NetworkServer.Spawn(spawnedEntity);

        // Apply difficulty scaling
        AIBaseLogic enemyStats = spawnedEntity.GetComponent<AIBaseLogic>();
        if (enemyStats != null)
        {
            enemyStats.enemyStats(healthMultiplier, damageMultiplier);
        }

        enemiesAlive.Add(spawnedEntity);
    }
    */

    #endregion

    #region Player Methods
    [Server]
    private bool IsSinglePlayerMode()
    {
        return NetworkServer.connections.Count == 1;
    }

    [Server]
    private bool IsSinglePlayerDead()
    {
        var players = FindObjectsOfType<PlayerManager>();
        if (players.Length == 1)
        {
            return players[0].isDead;
        }
        return false;
    }

    [Server]
    private bool IsAllPlayersDead()
    {
        var players = FindObjectsOfType<PlayerManager>();
        foreach (var player in players)
        {
            if (!player.isDead)
            {
                return false;
            }
        }
        return true;
    }

    [Server]
    void SaveAllPlayerData()
    {
        var playerManagers = FindObjectsOfType<PlayerManager>();
        foreach (var playerManager in playerManagers)
        {
            var connId = playerManager.connectionToClient.connectionId;
            PlayerManager.SavePlayerData(connId, playerManager);
            PlayerManager.SaveInventoryData(connId, playerManager);
        }
    }

    private HashSet<NetworkConnectionToClient> readyPlayers = new();

    public void SetPlayerReady(NetworkConnectionToClient conn, bool ready)
    {
        if (ready && EnemiesCleared)
        {
            readyPlayers.Add(conn);
            var playerManager = conn.identity.GetComponent<PlayerManager>();
            playerManager.TargetUpdateReadyStatus(conn, true);
        }

        CheckAllPlayersReady();
    }
    void CheckAllPlayersReady()
    {
        allPlayersReady = readyPlayers.Count == NetworkServer.connections.Count;
    }
    [Server]
    void ResetPlayerReadiness()
    {
        readyPlayers.Clear();
        allPlayersReady = false;
    }

    [Server]
    private void ReviveAllPlayers()
    {
        var players = FindObjectsOfType<PlayerManager>();
        foreach (var player in players)
        {
            if (player.isDead)
            {
                player.Respawn();
            }
        }
    }

    [Server]
    private void NotifyPlayersEnemiesCleared()
    {
        foreach (var conn in NetworkServer.connections.Values)
        {
            if (conn.identity != null)
            {
                var player = conn.identity.GetComponent<PlayerManager>();
                if (player != null)
                {
                    player.TargetUpdateEnemiesClearedStatus(conn, true);
                }
            }
        }
    }
    #endregion

    #region Scene Management

    [Server]
    void ProceedToNextScene()
    {
        switch (gameState)
        {
            case GameState.Stage:
                LoadShopScene();
                break;
            case GameState.Shop:
                LoadNextStage();
                break;
        }
    }

    [Server]
    void LoadShopScene()
    {
        if (isSceneChanging) return;

        SaveAllPlayerData();
        ResetPlayerReadiness();

        gameState = GameState.Shop;
        isSceneChanging = true;
        CustomNetworkManager.singleton.ServerChangeScene("Shop");
    }

    [Server]
    void LoadNextStage()
    {
        if (isSceneChanging) return;

        // Increment stage only after leaving the shop to move to the next stage
        if (gameState == GameState.Shop)
        {
            currentStage += 1;
        }

        if(gameMode == GameMode.MainCampaign)
        {
            if (currentStage <= mainCampaignManager.initialStageCount)
            {
                // If still within the initial set stages, load the next stage sequentially
                string nextStageName = $"Stage {currentStage}";
                lastStageLoaded = nextStageName;
                gameState = GameState.Stage;
                AttemptSceneChange(nextStageName);
            }
            else
            {
                // Once initial stages are done, proceed with endless mode
                List<string> possibleNextStages = mainCampaignManager.stageNames.Where(stage => stage != lastStageLoaded).ToList();

                if (possibleNextStages.Count > 0)
                {
                    // Randomly select next stage
                    string nextStage = possibleNextStages[Random.Range(0, possibleNextStages.Count)];
                    lastStageLoaded = nextStage;
                    gameState = GameState.Stage;
                    AttemptSceneChange(nextStage);
                }
                else
                {
                    Debug.LogError("No available stages for random selection in endless mode.");
                }
            }
        }
        if(gameMode == GameMode.EternalQuest)
        {
            gameState = GameState.Stage;
            AttemptSceneChange("EternalQuest");
        }
    }

    [Server]
    public void AttemptSceneChange(string sceneName)
    {
        if (!isSceneChanging)
        {
            SaveAllPlayerData();
            ResetPlayerReadiness();
            isSceneChanging = true;
            StartCoroutine(ChangeScene(sceneName));
        }
    }

    IEnumerator ChangeScene(string sceneName)
    {
        CustomNetworkManager.singleton.ServerChangeScene(sceneName);
        yield return new WaitUntil(() => SceneManager.GetActiveScene().name == sceneName);
        isSceneChanging = false;
    }
    #endregion

    #region Gamemode Selector
    public void MainCampaign()
    {
        mainCampaignManager.enabled = true;
        eternalQuestManager.enabled = false;
        CustomNetworkManager.singleton.GameplayScene = "Stage 1";
        gameMode = GameMode.MainCampaign;
    }

    public void EternalQuest()
    {
        eternalQuestManager.enabled = true;
        mainCampaignManager.enabled = false;
        CustomNetworkManager.singleton.GameplayScene = "EternalQuest";
        gameMode = GameMode.EternalQuest;
    }
    #endregion
}