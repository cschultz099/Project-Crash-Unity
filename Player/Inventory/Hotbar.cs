using UnityEngine;
using Mirror;
using Rewired;

public class Hotbar : MonoBehaviour
{
    [Header("Utilized")]
    private Player inputSystem;
    public PlayerManager playerManager;
    private InventoryManager inventoryManager;

    [Header("Internal Mechanics")]
    [SerializeField] private int currentSelectedSlot = 0;
    [ReadOnly] public int currentSlot = 0;
    private GameObject currentWeaponInstance;

    private void Start()
    {
        inputSystem = ReInput.players.GetPlayer(0);
        playerManager = GetComponentInParent<PlayerManager>();
        inventoryManager = GetComponentInParent<InventoryManager>();
    }

    private void Update()
    {
        for (int i = 0; i < 5; i++)
        {
            if (inputSystem.GetButtonDown("Select Slot" + (i + 1)))
            {
                SelectSlot(i);
                return;
            }
        }

        // Handle cycling through slots with controller
        if (inputSystem.GetButtonDown("Next Slot"))
        {
            currentSlot++;
            if (currentSlot >= 4) currentSlot = 0; // Wrap around to the first slot
            SelectSlot(currentSlot);
        }
        else if (inputSystem.GetButtonDown("Previous Slot"))
        {
            currentSlot--;
            if (currentSlot < 0) currentSlot = 4; // Wrap around to the last slot
            SelectSlot(currentSlot);
        }

        float scroll = inputSystem.GetAxis("Scroll Weapons");
        if (scroll != 0)
        {
            if (scroll > 0)
            {
                currentSelectedSlot--;
                if (currentSelectedSlot < 0)
                {
                    currentSelectedSlot = 4;
                }
            }
            else if (scroll < 0)
            {
                currentSelectedSlot++;
                if (currentSelectedSlot > 4)
                {
                    currentSelectedSlot = 0;
                }
            }
            SelectSlot(currentSelectedSlot);
        }
    }

    private void SelectSlot(int slotIndex)
    {
        for (int i = 0; i < inventoryManager.hotbarSlots.Length; i++)
        {
            Transform iconTransform = inventoryManager.hotbarSlots[i].transform.Find("Selected");
            iconTransform.gameObject.SetActive(i == slotIndex);
        }
        currentSelectedSlot = slotIndex;

        if (playerManager.isLocalPlayer)
        {
            SlotHandler slotHandler = inventoryManager.hotbarSlots[slotIndex].GetComponent<SlotHandler>();
            if (slotHandler && !slotHandler.IsSlotEmpty())
            {
                InventoryItem item = slotHandler.itemInSlot;
                if (item != null && item.isWeapon)
                {
                    SwitchWeapon(item.weaponPrefab);
                }
            }
            else
            {
                // If there's no weapon in the selected slot, remove the current weapon.
                RemoveCurrentWeapon();
            }
        }
    }

    private void SwitchWeapon(GameObject weaponPrefab)
    {
        // Remove current weapon if exists
        RemoveCurrentWeapon();

        if (weaponPrefab != null)
        {
            Transform weaponLocation = playerManager.weaponLocation.transform;
            GameObject weaponInstance = Instantiate(weaponPrefab, weaponLocation.position, Quaternion.identity, weaponLocation);
            NetworkServer.Spawn(weaponInstance, playerManager.connectionToClient);
            currentWeaponInstance = weaponInstance;

            // Set Weapon Ownership if applicable
            var weaponOwnership = weaponInstance.GetComponent<WeaponOwnership>();
            if (weaponOwnership != null)
            {
                weaponOwnership.SetOwner(playerManager.gameObject);
            }

            // Fix weapon position
            Vector3 parentScale = weaponLocation.transform.localScale;
            weaponInstance.transform.localScale = new Vector3(1 / parentScale.x, 1 / parentScale.y, 1 / parentScale.z);
            weaponInstance.transform.localPosition = Vector3.zero;
            weaponInstance.transform.localRotation = Quaternion.identity;
        }
    }

    private void RemoveCurrentWeapon()
    {
        if (currentWeaponInstance != null)
        {
            NetworkServer.Destroy(currentWeaponInstance);
            currentWeaponInstance = null; // Clear the reference
        }
    }

    public void CheckCurrentSlotChanges()
    {
        // Check for weapon changes
        if (playerManager.isLocalPlayer)
        {
            SlotHandler selectedSlot = inventoryManager.hotbarSlots[currentSelectedSlot].GetComponent<SlotHandler>();
            if (selectedSlot.itemInSlot != null && selectedSlot.itemInSlot.isWeapon)
            {
                SwitchWeapon(selectedSlot.itemInSlot.weaponPrefab);
            }
            else
            {
                RemoveCurrentWeapon();
            }
        }
    }
}