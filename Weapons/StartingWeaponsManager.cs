using Mirror;
using UnityEngine;

public class StartingWeaponsManager : NetworkBehaviour
{
    public GameObject[] startingWeapons;

    public void SpawnWeaponsForPlayer(NetworkIdentity playerId)
    {
        foreach (GameObject weaponPrefab in startingWeapons)
        {
            InventoryItem weapon = weaponPrefab.GetComponent<InventoryItem>();
            AddItemToInventory(weapon, playerId);
        }
    }

    void AddItemToInventory(InventoryItem item, NetworkIdentity player)
    {
        player.GetComponentInChildren<InventoryManager>().RpcAddItemToInventory(item, item.icon);
    }
}
