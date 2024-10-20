using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Rewired;

[System.Serializable]
public class PlayerData
{
    public int maxHealthData = 100;
    public int currencyBalanceData = 0;
    public int currentHealthData;
    public List<InventorySlotData> inventoryItems = new();
}

[System.Serializable]
public class InventorySlotData
{
    public InventoryItem item;
    public int slotID;
} // TODO

public class PlayerManager : NetworkBehaviour
{
    private GameManager gameManager;
    // Player settings
    [Header("Player Settings")]
    [SerializeField] private Player inputSystem;
    [SerializeField] private GameObject playerPrefab;

    // Cameras
    [Header("Cameras")]
    public Camera mainCamera;
    public Camera spectator;
    private Transform spectatorCameraPosition;

    // Audio
    [Header("Audio")]
    public AudioClip[] hurtSFX;
    public AudioClip deathSFX;

    // UI Elements
    [Header("UI Elements")]
    public TMP_Text advanceToNextSceneReadyUpText;
    public TMP_Text isPlayerReadyForNextSceneText;
    public TMP_Text coordinates;
    public TMP_Text healthBarText;
    public TMP_Text currencyText;

    // Gameplay
    [Header("Gameplay")]
    public GameObject weaponLocation;
    public float fallOutWorldLevel = 10.0f;

    // Player State
    [Header("Player State")]
    [SyncVar(hook = nameof(OnHealthChanged))] public int maxHealth = 100;
    [SyncVar(hook = nameof(OnHealthChanged))] public int currentHealth;
    [SyncVar(hook = nameof(OnCurrencyChanged))] public int currencyBalance;
    [SyncVar(hook = nameof(OnPlayerDeathStateChanged))] public bool isDead = false;

    // Player Stats
    [Header("Player Stats")]
    [Range(1, 10)]
    public int healthStatPoints = 5;
    [Range(1, 10)]
    public int defenceStatPoints = 5;
    [Range(1, 10)]
    public int luckStatPoints = 5;
    [Range(1, 10)]
    public int mobilityStatPoints = 5;

    [Header("Weapon Stats")]
    // Weapon Modifer Stats
    [Range(1, 10)]
    public int rangedAttackStatPoints = 5;
    [Range(1, 10)]
    public int meleeAttackStatPoints = 5;

    [Header("Player Modifers")]
    public int startingMaxHealthModifier = 20;
    public int shopPurchaseMaxHealthModifier = 2; // Amount of health points modified
    public float defencePercentModifier = 0.1f;
    public float luckPercentModifier = 0.1f;
    public float mobilityPercentModifier = 0.1f;

    [Header("Weapon Modifers")]
    // Weapon Stat Modifiers
    public float rangedAttackPercentModifier = 0.1f;
    public float meleeAttackPercentModifier = 0.1f;

    // Player Traits
    [Header("Player Traits")]
    public int maxTraits = 5;
    [ReadOnly]
    public int aquiredTraits;

    // Gameplay Stats
    [Header("Gameplay Stats")]
    [ReadOnly] public float damageDealt; // TODO
    [ReadOnly] public float damageTaken;
    [ReadOnly] public float enemiesKilled; // TODO
    [ReadOnly] public float healthHealed;
    [ReadOnly] public float goldCollected;

    // Misc
    [Header("Misc")]
    private bool coordinatesVisible = false;

    #region PlayerData
    public static Dictionary<int, PlayerData> playerDataByConnectionId = new();
    public static void SavePlayerData(int connectionId, PlayerManager player)
    {
        PlayerData playerData = new()
        {
            maxHealthData = player.maxHealth,
            currencyBalanceData = player.currencyBalance,
            currentHealthData = player.currentHealth,
        };

        playerDataByConnectionId[connectionId] = playerData;
    }

    public static Dictionary<int, PlayerData> playerInventoryDataByConnectionId = new();
    public static void SaveInventoryData(int connectionId, PlayerManager player)
    {
    PlayerData playerData = new();
        foreach (var slotObj in player.GetComponentInChildren<InventoryManager>().inventorySlots)
        {
            SlotHandler slot = slotObj.GetComponent<SlotHandler>();
            if (!slot.IsSlotEmpty())
            {
                InventorySlotData slotData = new()
                {
                    item = slot.itemInSlot,
                    slotID = slot.slotID
                };
                playerData.inventoryItems.Add(slotData);
            }
        }
        playerInventoryDataByConnectionId[connectionId] = playerData;
    }

    public void LoadInventory(PlayerData playerData)
    {
        foreach (var slotData in playerData.inventoryItems)
        {
            GetComponentInChildren<InventoryManager>().AddItemToRespectiveSlot(slotData.item, slotData.slotID, slotData.item.icon);
        }
    }

    public static PlayerData LoadPlayerData(int connectionId)
    {
        if (playerDataByConnectionId.TryGetValue(connectionId, out PlayerData playerData))
        {
            return playerData;
        }
        return null; // Or return default data
    }
    #endregion

    void Start()
    {
        inputSystem = ReInput.players.GetPlayer(0);
        gameManager = GameManager.Singleton;
        if (isLocalPlayer)
        {
            UpdateHealthUI();
            UpdateCurrencyUI();
            GetComponentInParent<AudioListener>().enabled = true;
            StartCoroutine(DelayedWeaponSpawnRequest());
        }
        else
        {
            if (healthBarText != null)
            {
                healthBarText.gameObject.SetActive(false);
            }
            if(currencyText != null)
            {
                currencyText.gameObject.SetActive(false);
            }
        }

        // Set the player's health to max when the game starts
        if(gameManager.currentStage == 1 && !gameManager.InShop && isServer)
        {
            HealthStat();
        }
        
        spectatorCameraPosition = GameObject.FindGameObjectWithTag("SpectatorCameraPosition").transform;
    }

    void Update()
    {
        if (isLocalPlayer)
        {
            UpdateCoordinatesDisplay();
        }
        if (isLocalPlayer && inputSystem.GetButtonDown("Ready Up"))
        {
            CmdSetPlayerReady();
        }
        if(isLocalPlayer && inputSystem.GetButtonDown("Show Coordinates"))
        {
            coordinatesVisible = !coordinatesVisible;
            coordinates.gameObject.SetActive(coordinatesVisible);
        }
        if (isServer && Mathf.Abs(playerPrefab.transform.position.y - fallOutWorldLevel) <= 0.1f) // 0.1 is the tolerance
        {
            Die();
        }
    }

    #region Weapon Management

    IEnumerator DelayedWeaponSpawnRequest()
    {
        yield return new WaitForSeconds(1.0f); // We will need to Find a way to spawn weapons after all players are in the scene
        CmdRequestWeaponsToInventory();
    }

    public void CmdRequestWeaponsToInventory()
    {
        if (!isLocalPlayer) return;

        RpcRequestWeaponsToInventory();
    }

    [ClientRpc]
    void RpcRequestWeaponsToInventory()
    {
        var weaponManager = FindObjectOfType<StartingWeaponsManager>();
        if (weaponManager != null)
        {
            weaponManager.SpawnWeaponsForPlayer(netIdentity);
        }
        else
        {
            Debug.Log("Could not find WeaponManager");
        }
    }
    #endregion

    #region Player Methods
    private void UpdateCoordinatesDisplay()
    {
        if (coordinates != null)
        {
            Vector3 position = playerPrefab.transform.position;
            coordinates.text = $"X: {position.x:F1}, Y: {position.y:F1}, Z: {position.z:F1}";
        }
    }

    [Command]
    public void CmdSetPlayerReady()
    {
        // Currently sets the local player permanently Ready for the current scene
        gameManager.SetPlayerReady(connectionToClient, true);
    }

    [TargetRpc]
    public void TargetUpdateReadyStatus(NetworkConnection target, bool isReady)
    {
        if (isLocalPlayer)
        {
            isPlayerReadyForNextSceneText.gameObject.SetActive(true);
            isPlayerReadyForNextSceneText.text = isReady ? "Ready!" : "Not Ready";
            isPlayerReadyForNextSceneText.color = isReady ? Color.green : Color.red;
        }
    }

    [TargetRpc]
    public void TargetUpdateEnemiesClearedStatus(NetworkConnection target, bool isCleared)
    {
        if (isCleared && isLocalPlayer)
        {
            advanceToNextSceneReadyUpText.gameObject.SetActive(true);
            advanceToNextSceneReadyUpText.text = "All enemies cleared! Press Enter to proceed to the Shop.";
        }
    }
    #endregion

    #region Currency
    public void OnCurrencyChanged(int oldBalance, int newBalance)
    {
        if (isLocalPlayer)
        {
            UpdateCurrencyUI();
        }
    }
    void UpdateCurrencyUI()
    {
        if (currencyText != null)
            currencyText.text = currencyBalance.ToString();
    }

    [Server]
    public void ServerAddCurrency(int currencyAmount)
    {
        currencyBalance += currencyAmount;
        goldCollected += currencyAmount;
    }
    [Command]
    public void CmdDeductCurrency(int currencyAmount)
    {
        //shop = GameObject.Find("Shop").GetComponent<ShopMenu>();
        currencyBalance -= currencyAmount;
        //shop.TargetUpdateBalance(sender ?? connectionToClient);
    }
    #endregion

    #region Health System
    [Server]
    public void TakeDamage(int damage)
    {
        float damageResistance = DefenceStat();
        float takenDamage = damage - (damage * damageResistance);

        damageTaken += takenDamage;

        currentHealth -= Mathf.RoundToInt(takenDamage);
        currentHealth = Mathf.Max(currentHealth, 0);
        //Debug.Log($"Player {netId} took {damage} damage. New health: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
            //GetComponentInParent<AudioManager>().PlaySFX(deathSFX);
        }
        PlayHurtSFX();
    }

    [ClientRpc]
    public void PlayHurtSFX()
    {
        GetComponentInParent<AudioManager>().PlayRandomSFX(hurtSFX);
    }

    [Server]
    public void Heal(int amount)
    {
        currentHealth += amount;
        healthHealed += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
    }
    [Server]
    public void MaxHealthincrease(int newMaxHealth)
    {
        maxHealth += newMaxHealth;
        currentHealth += newMaxHealth;
    }

    [Command]
    public void CmdPurchaseHealth(int health)
    {
        Heal(health);
    }
    [Command]
    public void CmdSetMaxHealth(int newMaxHealth)
    {
        MaxHealthincrease(newMaxHealth);
    }

    [Server]
    public void ResetHealthToMax()
    {
        currentHealth = maxHealth;
    }

    void OnHealthChanged(int oldHealth, int newHealth)
    {
        if (isLocalPlayer)
        {
            UpdateHealthUI();
        }
    }
    void OnPlayerDeathStateChanged(bool oldValue, bool newValue)
    {
        //
    }

    void UpdateHealthUI()
    {
        if (healthBarText != null)
            healthBarText.text = currentHealth.ToString();
    }

    [Server]
    public void Die()
    {
        isDead = true;
        // Set player position to map so player doesn't constantly die (falling out of map)
        playerPrefab.transform.position = Vector3.zero;
        RpcHandleDeath();
    }

    [ClientRpc]
    private void RpcHandleDeath()
    {
        if(isLocalPlayer)
        {
            mainCamera.gameObject.SetActive(false);
            spectator.GetComponent<Camera>().enabled = true;
            spectator.GetComponent<SpectatorPlayer>().enabled = true;
            spectator.transform.position = spectatorCameraPosition.transform.position;
            currentHealth = maxHealth; // The is temp. until reviveallplayers is implmented
        }
        spectator.gameObject.SetActive(true);
        playerPrefab.SetActive(false);
    }

    [Server]
    public void Respawn() // Does nothing right now
    {
        isDead = false;
        RpcHandleRespawn();
    }

    [ClientRpc]
    public void RpcHandleRespawn()
    {
        spectator.gameObject.SetActive(false);
        playerPrefab.SetActive(true);

        if (isLocalPlayer)
        {
            mainCamera.gameObject.SetActive(true);
            spectator.GetComponent<Camera>().enabled = false;
            spectator.GetComponent<SpectatorPlayer>().enabled = false;
            currentHealth = maxHealth;
        }
    }
    #endregion

    #region Player Stats
    public void HealthStat()
    {
        if(isLocalPlayer)
        {
            maxHealth = healthStatPoints * startingMaxHealthModifier;
            currentHealth = maxHealth;
        }
    }

    public float DefenceStat()
    {
        if (isLocalPlayer)
        {
            var defenceStat = (defenceStatPoints - 5) * defencePercentModifier;
            return defenceStat;
        }
        else
        {
            return 0;
        }
    }

    public float LuckStat()
    {
        if (isLocalPlayer)
        {
            var luckStat = (luckStatPoints - 5) * luckPercentModifier;
            return luckStat;
        }
        else
        {
            return 0;
        }
    }

    public float MobilityStat()
    {
        if (isLocalPlayer)
        {
            var mobilityStat = (mobilityStatPoints - 5) * mobilityPercentModifier;
            return mobilityStat;
        }
        else
        {
            return 0;
        }
    }

    // Weapon Modifer Stats
    public float RangedAttackStat()
    {
        if (isLocalPlayer)
        {
            var attackStat = (rangedAttackStatPoints - 5) * rangedAttackPercentModifier;
            return attackStat;
        }
        else
        {
            return 0;
        }
    }

    public float meleeAttackStat()
    {
        if (isLocalPlayer)
        {
            var attackStat = (meleeAttackStatPoints - 5) * meleeAttackPercentModifier;
            return attackStat;
        }
        else
        {
            return 0;
        }
    }
    #endregion

    #region Player Traits
    // Significantly Increased Defence & Health | Slightly Decreased Attack & Mobility
    public void GuardiansResolve()
    {
        if(aquiredTraits < maxTraits)
        {
            defenceStatPoints = 8;
            healthStatPoints = 7;
            //attackStatPoints = 3; // TODO
            mobilityStatPoints = 3;
            aquiredTraits += 1;
        }
    }
    // Significantly Increased Attack | Slightly Decreased Health & Defence
    public void BeserkersFury()
    {
        if (aquiredTraits < maxTraits)
        {
            //attackStatPoints = 9; // TODO
            defenceStatPoints = 4;
            healthStatPoints = 3;
            aquiredTraits += 1;
        }
    }
    // Significantly Increased Luck | Slightly Decreased Mobility
    public void SagesWisdom()
    {
        if (aquiredTraits < maxTraits)
        {
            luckStatPoints = 8;
            mobilityStatPoints = 4;
            aquiredTraits += 1;
        }
    }
    // Significantly Increased Mobility | Slightly Decreased Health
    public void ScoutsAgility()
    {
        if (aquiredTraits < maxTraits)
        {
            mobilityStatPoints = 8;
            healthStatPoints = 3;
            aquiredTraits += 1;
        }
    }
    #endregion
}