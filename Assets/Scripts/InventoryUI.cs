using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [Header("인벤토리 UI")]
    public GameObject inventoryPanel;
    public GameObject inventoryCardPrefab;
    public GameObject combinedCardPrefab;
    public Transform cardContainer;
    public RectTransform dragLayer;

    // CardManager is the sole entry point for connecting cards.

    private readonly List<InventoryCardUI> displayedCards =
        new List<InventoryCardUI>();
    private IReadOnlyList<CardInstance> inventoryData;
    private bool needsRefresh;

    public InventoryCardUI CurrentDraggingCard { get; private set; }
    public bool IsOpen => inventoryPanel != null && inventoryPanel.activeSelf;

    public void OpenInventory(IReadOnlyList<CardInstance> inventory)
    {
        if (inventoryPanel == null || cardContainer == null)
        {
            Debug.LogError("InventoryUI: 인벤토리 패널 또는 카드 영역이 없습니다.", this);
            return;
        }

        inventoryData = inventory;
        inventoryPanel.SetActive(true);
        ConfigureLayout();
        RefreshInventory();
    }

    public void CloseInventory()
    {
        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);
    }

    // The prefab used GridLayoutGroup with fixed 100x100 cells. Scratch
    // chains have different heights, so use a vertical list instead.
    private void ConfigureLayout()
    {
        if (cardContainer == null)
            return;

        GridLayoutGroup grid = cardContainer.GetComponent<GridLayoutGroup>();
        if (grid != null)
            grid.enabled = false;

        HorizontalLayoutGroup horizontal =
            cardContainer.GetComponent<HorizontalLayoutGroup>();
        if (horizontal != null)
            horizontal.enabled = false;

        VerticalLayoutGroup column =
            cardContainer.GetComponent<VerticalLayoutGroup>();
        if (column == null)
            column = cardContainer.gameObject.AddComponent<VerticalLayoutGroup>();

        column.padding = new RectOffset(12, 12, 12, 12);
        column.spacing = 12f;
        column.childAlignment = TextAnchor.UpperCenter;
        column.childControlWidth = false;
        column.childControlHeight = true;
        column.childForceExpandWidth = false;
        column.childForceExpandHeight = false;

        RectTransform rect = cardContainer as RectTransform;
        if (rect != null)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(18f, 18f);
            rect.offsetMax = new Vector2(-18f, -18f);
        }
    }

    private void RefreshInventory()
    {
        if (CurrentDraggingCard != null)
        {
            needsRefresh = true;
            return;
        }

        foreach (InventoryCardUI ui in displayedCards)
        {
            if (ui == null)
                continue;

            // Remove the old items from layout immediately.
            ui.gameObject.SetActive(false);
            Destroy(ui.gameObject);
        }
        displayedCards.Clear();

        if (inventoryData == null || cardContainer == null)
            return;

        foreach (CardInstance instance in inventoryData)
        {
            if (instance == null || instance.parentCard != null)
                continue;

            bool combined = instance.childCard != null;
            GameObject prefab = combined ? combinedCardPrefab : inventoryCardPrefab;
            if (prefab == null)
            {
                Debug.LogError("인벤토리 카드 프리팹이 설정되지 않았습니다.", this);
                continue;
            }

            GameObject obj = Instantiate(prefab, cardContainer);
            InventoryCardUI ui = obj.GetComponent<InventoryCardUI>();
            if (ui == null)
            {
                Debug.LogError("카드 프리팹에 InventoryCardUI가 없습니다.", obj);
                Destroy(obj);
                continue;
            }

            ui.SetCard(instance, this);
            if (combined)
            {
                CombinedCardUI combinedUI = obj.GetComponent<CombinedCardUI>();
                if (combinedUI != null)
                    combinedUI.SetCard(instance, this);
                else
                    Debug.LogError("합성 카드 프리팹에 CombinedCardUI가 없습니다.", obj);
            }

            int length = combined ? CombinedCardUI.CountChain(instance) : 1;
            float height = combined
                ? CombinedCardUI.ChainHeight(length)
                : ScratchBlockGraphic.BlockHeight;

            RectTransform rect = obj.transform as RectTransform;
            if (rect != null)
                rect.sizeDelta = new Vector2(270f, height);

            LayoutElement element = obj.GetComponent<LayoutElement>();
            if (element == null)
                element = obj.AddComponent<LayoutElement>();
            element.minWidth = 200f;
            element.preferredWidth = 270f;
            element.preferredHeight = height;
            element.flexibleHeight = 0f;

            displayedCards.Add(ui);
        }

        if (cardContainer is RectTransform containerRect)
            LayoutRebuilder.ForceRebuildLayoutImmediate(containerRect);
    }

    public void BeginDrag(InventoryCardUI card)
    {
        if (CurrentDraggingCard == null)
            CurrentDraggingCard = card;
    }

    public void EndDrag()
    {
        CurrentDraggingCard = null;
        if (needsRefresh)
        {
            needsRefresh = false;
            RefreshInventory();
        }
    }

    public void TryConnect(InventoryCardUI parentUI, InventoryCardUI childUI)
    {
        if (parentUI == null || childUI == null ||
            parentUI == childUI || CardManager.Instance == null)
            return;

        CardInstance tail = parentUI.CardInstance;
        CardInstance childRoot = childUI.CardInstance;
        if (tail == null || childRoot == null ||
            childRoot.parentCard != null ||
            !CardManager.Instance.ContainsCard(tail) ||
            !CardManager.Instance.ContainsCard(childRoot))
            return;

        var visited = new HashSet<CardInstance>();
        while (tail.childCard != null)
        {
            if (!visited.Add(tail))
            {
                Debug.LogWarning("부모 카드 체인에 순환이 있습니다.");
                return;
            }
            tail = tail.childCard;
        }

        if (!CardManager.Instance.TryConnect(tail, childRoot))
        {
            Debug.Log("유효하지 않은 카드 조합입니다.");
            return;
        }

        // InventoryChanged is handled by UIManager; RefreshInventory
        // automatically defers until the drag is completed.
    }

    public void DisconnectCard(CardInstance child)
    {
        if (child == null || child.parentCard == null ||
            CardManager.Instance == null)
            return;

        CardManager.Instance.Disconnect(child);
    }

}
