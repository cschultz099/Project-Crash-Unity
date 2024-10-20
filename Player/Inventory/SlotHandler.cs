using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SlotHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Slot Data")]
    public InventoryItem itemInSlot;
    public int slotID;

    private InventoryManager inventoryManager;
    private GameObject dragIcon;

    private void Awake()
    {
        inventoryManager = GetComponentInParent<InventoryManager>();
    }

    public void SetSlotIcon(Sprite icon)
    {
        Image iconSlot = transform.Find("Icon").GetComponent<Image>();
        iconSlot.sprite = icon;
        iconSlot.gameObject.SetActive(true);
    }

    public void RemoveSlotIcon()
    {
        Image iconSlot = transform.Find("Icon").GetComponent<Image>();
        iconSlot.sprite = null;
        iconSlot.gameObject.SetActive(false);
    }

    public bool IsSlotEmpty() => itemInSlot == null;

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (IsSlotEmpty()) return;

        dragIcon = new GameObject("DragIcon");
        dragIcon.transform.SetParent(transform.parent, false);
        dragIcon.transform.SetAsLastSibling();
        Image iconImage = dragIcon.AddComponent<Image>();
        iconImage.sprite = transform.Find("Icon").GetComponent<Image>().sprite;
        iconImage.rectTransform.sizeDelta = new Vector2(50, 50);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if(dragIcon != null)
            dragIcon.transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (dragIcon != null)
            Destroy(dragIcon);

        if (eventData.pointerCurrentRaycast.gameObject.TryGetComponent<SlotHandler>(out var targetSlot))
        {
            inventoryManager.MoveItem(this, targetSlot);
        }
    }
}