using Mirror;
using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    #region Spawner
    [Header("Spawner")]
    public GameObject enemy;
    public int enemiesToSpawn = 0;
    public float spawnDistance = 2f;

    [Header("Timing")]
    public bool recurringSpawn = false; // TODO
    [Space]
    public bool spawnImmediately = false;
    public bool timedSpawn = false;
    public float timeToSpawn = 0.0f;
    public bool proximitySpawn = false;
    public float proximitySpawnRange = 10.0f; // TODO

    private PlayerManager player;
    private GameManager gameManager;
    [ReadOnly] public bool enemiesSpawnedFlag = false;
    #endregion

    #region Enemy Stat Overrides
    [Header("Enemy Stats")]
    public int attackDamage = 20;
    public int maxHealth = 100;
    public float damageCooldown = 1.0f;
    public float windupAttack = 2.0f;
    #endregion

    #region Enemy Loot Overrides
    [Header("Loot")]
    public float healthPackDropChance = 10.0f;
    public float currencyDropChance = 25.0f;
    public GameObject[] commonLootTable;
    public GameObject[] uncommonLootTable;
    public GameObject[] rareLootTable;
    public GameObject[] ultraRareLootTable;
    public float commonLootChance = 25.0f;
    public float uncommonLootChance = 15.0f;
    public float rareLootChance = 5.0f;
    public float ultraRareChance = 1.0f;
    #endregion

    private void Awake()
    {
        gameManager = GameManager.Singleton;
    }

    private void Start()
    {
        if (spawnImmediately)
        {
            SpawnEnemies();
        }
        if (timedSpawn)
        {
            StartCoroutine(TimedSpawn());
        }
    }

    private void Update()
    {
        if (proximitySpawn && player != null)
        {
            SpawnEnemies();
        }
    }

    #region Helper Methods
    IEnumerator TimedSpawn()
    {
        yield return new WaitForSeconds(timeToSpawn);
        SpawnEnemies();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && other.GetComponentInParent<NetworkIdentity>().isLocalPlayer)
        {
            var tempPlayer = other.GetComponentInParent<PlayerManager>();
            if (tempPlayer != null)
            {
                player = tempPlayer;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        player = null;
    }
    #endregion

    #region Spawn Enemies
    [Server]
    public void SpawnEnemies()
    {
        if (enemiesSpawnedFlag) { return; }

        Vector3 spawnPosition = transform.position;

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            var spawnedEnemy = Instantiate(enemy, spawnPosition, Quaternion.identity);
            //Debug.Log($"Enemy spawned at: {spawnedEnemy.transform.position}");
            spawnPosition += new Vector3(0, 0, spawnDistance);

            // Enemy Stat overrides
            AIBaseLogic enemyAILogic = spawnedEnemy.GetComponent<AIBaseLogic>();
            enemyAILogic.attackDamage = attackDamage;
            enemyAILogic.maxHealth = maxHealth;
            enemyAILogic.damageCooldown = damageCooldown;
            enemyAILogic.windupAttack = windupAttack;

            // Loot overrides
            EnemyLoot enemyLoot = spawnedEnemy.GetComponent<EnemyLoot>();
            enemyLoot.healthPackDropChance = healthPackDropChance;
            enemyLoot.currencyDropChance = currencyDropChance;
            if (commonLootTable.Length != 0) { enemyLoot.commonLootTable = commonLootTable; }
            if (uncommonLootTable.Length != 0) { enemyLoot.uncommonLootTable = uncommonLootTable; }
            if (rareLootTable.Length != 0) { enemyLoot.rareLootTable = rareLootTable; }
            if (ultraRareLootTable.Length != 0) { enemyLoot.ultraRareLootTable = ultraRareLootTable; }
            enemyLoot.commonLootChance = commonLootChance;
            enemyLoot.uncommonLootChance = uncommonLootChance;
            enemyLoot.rareLootChance = rareLootChance;
            enemyLoot.ultraRareChance = ultraRareChance;

            NetworkServer.Spawn(spawnedEnemy);
            gameManager.enemiesAlive.Add(spawnedEnemy);
        }

        enemiesSpawnedFlag = true;
    }
    #endregion
}