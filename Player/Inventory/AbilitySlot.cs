using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AbilitySlot : MonoBehaviour
{
    [Header("Slot Data")]
    public InventoryItem itemInSlot;

    public bool IsSlotEmpty() => itemInSlot == null;

    public void SetIcon(Sprite icon)
    {
        Image iconSlot = transform.Find("Icon").GetComponent<Image>();
        iconSlot.gameObject.SetActive(true);
        iconSlot.sprite = icon;
    }
}
