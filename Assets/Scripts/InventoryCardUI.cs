using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryCardUI : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [Header("카드 UI")]
    public Image cardImage;
    public TextMeshProUGUI cardName;
    public Transform childContainer;

    private CardInstance cardInstance;
    private InventoryUI inventoryUI;
    private CanvasGroup canvasGroup;
    private Transform originalParent;
    private int originalSiblingIndex;

    public CardInstance CardInstance => cardInstance;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void SetCard(CardInstance instance, InventoryUI owner)
    {
        cardInstance = instance;
        inventoryUI = owner;
        if (instance == null)
            return;

        // The CombinedCard prefab draws all segments separately.
        if (GetComponent<CombinedCardUI>() != null)
            return;

        ScratchBlockGraphic shape = GetComponent<ScratchBlockGraphic>();
        if (shape == null)
            shape = gameObject.AddComponent<ScratchBlockGraphic>();
        shape.color = CardInstanceDisplay.ColorFor(instance);
        shape.SetConnectors(false, false);
        shape.raycastTarget = true;

        if (cardName != null)
        {
            cardName.text = CardInstanceDisplay.Title(instance);
            cardName.fontSize = 22f;
            cardName.alignment = TextAlignmentOptions.MidlineLeft;
            cardName.raycastTarget = false;
            RectTransform rect = cardName.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(54f, 10f);
            rect.offsetMax = new Vector2(-12f, -10f);
        }

        if (cardImage != null)
        {
            Sprite sprite = instance.cardData != null
                ? instance.cardData.cardImage : null;
            cardImage.gameObject.SetActive(sprite != null);
            if (sprite != null)
                cardImage.sprite = sprite;
            cardImage.raycastTarget = false;
            cardImage.preserveAspect = true;
            RectTransform rect = cardImage.rectTransform;
            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.anchorMax = new Vector2(0f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(27f, 0f);
            rect.sizeDelta = new Vector2(34f, 34f);
        }

        Transform description = transform.Find("Description");
        if (description != null)
            description.gameObject.SetActive(false);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (cardInstance == null || cardInstance.parentCard != null ||
            inventoryUI == null || inventoryUI.dragLayer == null)
            return;

        inventoryUI.BeginDrag(this);
        if (inventoryUI.CurrentDraggingCard != this)
            return;

        originalParent = transform.parent;
        originalSiblingIndex = transform.GetSiblingIndex();
        transform.SetParent(inventoryUI.dragLayer, true);
        transform.SetAsLastSibling();
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (inventoryUI == null || inventoryUI.CurrentDraggingCard != this)
            return;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            inventoryUI.dragLayer, eventData.position,
            eventData.pressEventCamera, out Vector2 point))
            ((RectTransform)transform).localPosition = point;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (inventoryUI == null || inventoryUI.CurrentDraggingCard != this)
            return;

        canvasGroup.blocksRaycasts = true;
        // Restore the dragged UI before rebuilding the inventory.
        if (originalParent != null)
        {
            transform.SetParent(originalParent, false);
            transform.SetSiblingIndex(originalSiblingIndex);
        }
        inventoryUI.EndDrag();
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (inventoryUI == null)
            return;

        InventoryCardUI dragged = inventoryUI.CurrentDraggingCard;
        if (dragged != null && dragged != this)
            inventoryUI.TryConnect(this, dragged);
    }

    // Kept for compatibility with existing callers/prefabs.
    public void AttachChild(InventoryCardUI child)
    {
        if (child == null || childContainer == null)
            return;
        child.transform.SetParent(childContainer, false);
        child.transform.localScale = Vector3.one;
    }
}
