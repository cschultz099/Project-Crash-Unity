using Mirror;
using UnityEngine;

public class EnemyLoot : MonoBehaviour
{
    public GameObject currency;
    public GameObject healthPack;
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

    public void SpawnLoot(float luckModifier)
    {
        // Luck Stat Modifier
        rareLootChance += rareLootChance * luckModifier;
        ultraRareChance += ultraRareChance * luckModifier;

        float randomChance = Random.Range(0, 100);
        GameObject[] selectedLootTable;
        if (randomChance <= ultraRareChance)
        {
            selectedLootTable = ultraRareLootTable;
        }
        else if(randomChance <= rareLootChance)
        {
            selectedLootTable = rareLootTable;
        }
        else if(randomChance <= uncommonLootChance)
        {
            selectedLootTable = uncommonLootTable;
        }
        else
        {
            selectedLootTable = commonLootTable;
        }

        if(selectedLootTable != null && selectedLootTable.Length > 0)
        {
            GameObject lootToSpawn = selectedLootTable[Random.Range(0, selectedLootTable.Length)];
            GameObject lootInstance = Instantiate(lootToSpawn, transform.position, Quaternion.identity);
            NetworkServer.Spawn(lootInstance);
        }
    }

    public void SpawnCurrency(float luckModifier)
    {
        // If the local player with luck picks up that coin, it is localized to that player
        float randomChance = Random.Range(0f, 100f);
        if(randomChance <= currencyDropChance)
        {
            if (currency != null)
            {
                GameObject currencyInstance = Instantiate(currency, transform.position, Quaternion.identity);
                NetworkServer.Spawn(currencyInstance);
            }
        }
    }

    public void SpawnHealthPack(float luckModifier)
    {
        healthPackDropChance += healthPackDropChance * luckModifier;

        float randomchance = Random.Range(0f, 100f);
        if(randomchance <= healthPackDropChance)
        {
            if(healthPack != null)
            {
                GameObject healthPackInstance = Instantiate(healthPack, transform.position, Quaternion .identity);
                NetworkServer.Spawn(healthPackInstance);
            }
        }
    }
}
