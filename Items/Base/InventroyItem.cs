using Mirror;
using UnityEngine;

[System.Serializable]
public class InventoryItem : NetworkBehaviour
{
    public string itemName;
    public Sprite icon;

    public bool isItem;
    public GameObject itemPrefab;
    public bool isWeapon;
    public GameObject weaponPrefab;

    private void Awake()
    {
        if(isItem == true)
        {
            itemPrefab = gameObject;
        }

        if(isWeapon == true)
        {
            weaponPrefab = gameObject;
        }
    }
}