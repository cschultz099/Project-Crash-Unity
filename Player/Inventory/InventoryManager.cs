using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class InventoryManager : NetworkBehaviour
{
    [Header("Inventory Data")]
    public GameObject[] hotbarSlots;
    public GameObject[] inventorySlots;

    private Hotbar hotbar;

    private void Start()
    {
        if(isLocalPlayer)
            hotbar = GetComponentInChildren<Hotbar>();
    }

    [ClientRpc]
    public void RpcAddItemToInventory(InventoryItem item, Sprite icon)
    {
        AddItemToInventory(item, icon);
    }

    public void AddItemToInventory(InventoryItem item, Sprite icon)
    {
        foreach (var slotObj in hotbarSlots)
        {
            SlotHandler slot = slotObj.GetComponent<SlotHandler>();

            if(slot.IsSlotEmpty())
            {
                slot.itemInSlot = item;
                slot.SetSlotIcon(icon);
                hotbar.CheckCurrentSlotChanges();
                return;
            }
        }
        foreach (var slotObj in inventorySlots)
        {
            SlotHandler slot = slotObj.GetComponent<SlotHandler>();

            if (slot.IsSlotEmpty())
            {
                slot.itemInSlot = item;
                slot.SetSlotIcon(icon);
                hotbar.CheckCurrentSlotChanges();
                return;
            }
        }
        Debug.Log("Inventory and Hotbar are full");
    }

    public void AddItemToRespectiveSlot(InventoryItem item, int slotID , Sprite icon)
    {
        SlotHandler inventorySlot = inventorySlots[slotID].GetComponent<SlotHandler>();
        SlotHandler hotbarSlot = hotbarSlots[slotID].GetComponent<SlotHandler>();
        if (inventorySlot)
        {
            inventorySlot.itemInSlot = item;
            inventorySlot.SetSlotIcon(icon);
            hotbar.CheckCurrentSlotChanges();
        }
        else if (hotbarSlot)
        {
            hotbarSlot.itemInSlot = item;
            hotbarSlot.SetSlotIcon(icon);
            hotbar.CheckCurrentSlotChanges();
        }
    }

    public void RemoveItemFromInventory(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < inventorySlots.Length)
        {
            SlotHandler slot = inventorySlots[slotIndex].GetComponent<SlotHandler>();
            if (!slot.IsSlotEmpty())
            {
                slot.itemInSlot = null;
                hotbar.CheckCurrentSlotChanges();
            }
            else
            {
                Debug.Log($"Inventory slot {slotIndex} is already empty.");
            }
        }
    }

    public void RemoveItemFromHotbar(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < hotbarSlots.Length)
        {
            SlotHandler slot = hotbarSlots[slotIndex].GetComponent<SlotHandler>();
            if (!slot.IsSlotEmpty())
            {
                slot.itemInSlot = null;
                hotbar.CheckCurrentSlotChanges();
            }
            else
            {
                Debug.Log($"Hotbar slot {slotIndex} is already empty.");
            }
        }
    }

    public void MoveItem(SlotHandler originSlot, SlotHandler targetSlot)
    {
        if (originSlot == null || targetSlot == null) return;

        // Case when the origin slot has an item and the target slot is empty
        if (!originSlot.IsSlotEmpty() && targetSlot.IsSlotEmpty())
        {
            targetSlot.itemInSlot = originSlot.itemInSlot;
            targetSlot.SetSlotIcon(originSlot.itemInSlot.icon);
            originSlot.itemInSlot = null;
            originSlot.RemoveSlotIcon();

            hotbar.CheckCurrentSlotChanges();
        }
        // Case when both slots have items
        else if (!originSlot.IsSlotEmpty() && !targetSlot.IsSlotEmpty())
        {

            InventoryItem tempItem = targetSlot.itemInSlot;
            targetSlot.SetSlotIcon(originSlot.itemInSlot.icon);
            originSlot.SetSlotIcon(tempItem.icon);
            targetSlot.itemInSlot = originSlot.itemInSlot;
            originSlot.itemInSlot = tempItem;

            hotbar.CheckCurrentSlotChanges();
        }
        else
        {
            Debug.Log("Cannot move item to the target slot.");
        }
    }
}