using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryCardUI :
    MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler,
    IDropHandler
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

        if (instance == null || instance.cardData == null)
        {
            Debug.LogError("카드 데이터가 비어 있습니다.");
            return;
        }

        // 기본 카드 프리팹에서만 사용
        if (cardName != null)
            cardName.text = instance.cardData.cardName;

        if (cardImage != null)
            cardImage.sprite = instance.cardData.cardImage;
    }


    public void OnBeginDrag(PointerEventData eventData)
    {
        if (cardInstance.parentCard != null)
            return;

        inventoryUI.BeginDrag(this);

        originalParent = transform.parent;
        originalSiblingIndex = transform.GetSiblingIndex();

        transform.SetParent(inventoryUI.dragLayer, true);
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (inventoryUI.CurrentDraggingCard != this)
            return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            inventoryUI.dragLayer,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPosition
        );

        GetComponent<RectTransform>().localPosition = localPosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (inventoryUI.CurrentDraggingCard != this)
            return;

        canvasGroup.blocksRaycasts = true;

        if (cardInstance.parentCard == null)
        {
            transform.SetParent(originalParent, false);
            transform.SetSiblingIndex(originalSiblingIndex);
        }

        inventoryUI.EndDrag();
    }

    public void OnDrop(PointerEventData eventData)
    {
        InventoryCardUI draggedCard =
            inventoryUI.CurrentDraggingCard;

        if (draggedCard == null || draggedCard == this)
            return;

        inventoryUI.TryConnect(this, draggedCard);
    }

    public void AttachChild(InventoryCardUI child)
    {
        child.transform.SetParent(childContainer, false);
        child.transform.localScale = Vector3.one;
    }
}