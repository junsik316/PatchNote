
using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [Header("인벤토리 UI")]
    public GameObject inventoryPanel;

    public GameObject inventoryCardPrefab;
    public GameObject combinedCardPrefab;

    public Transform cardContainer;
    public RectTransform dragLayer;

    [Header("조합 관리")]
    public CardCombinationManager combinationManager;

    private List<InventoryCardUI> displayedCards =
        new List<InventoryCardUI>();

    private Dictionary<CardInstance, InventoryCardUI> uiDictionary =
        new Dictionary<CardInstance, InventoryCardUI>();

    

    private List<CardInstance> inventoryData;
    private bool needsRefresh;

    public InventoryCardUI CurrentDraggingCard { get; private set; }

    public bool IsOpen => inventoryPanel.activeSelf;

    // 인벤토리 열기
    public void OpenInventory(List<CardInstance> inventory)
    {
        inventoryData = inventory;

        RefreshInventory();

        inventoryPanel.SetActive(true);
    }

    public void CloseInventory()
    {
        inventoryPanel.SetActive(false);
    }

    // 카드 UI 생성 및 연결 관계 복원
    private void RefreshInventory()
    {
        foreach (InventoryCardUI ui in displayedCards)
            Destroy(ui.gameObject);

        displayedCards.Clear();

        if (inventoryData == null)
            return;

        foreach (CardInstance instance in inventoryData)
        {
            // 다른 카드에 소속된 카드는 따로 생성하지 않음
            if (instance.parentCard != null)
                continue;

            // 자식 카드가 있다면 합성 카드로 표시
            bool isCombined = instance.childCard != null;

            GameObject prefab = isCombined
                ? combinedCardPrefab
                : inventoryCardPrefab;

            GameObject obj = Instantiate(prefab, cardContainer);

            InventoryCardUI ui =
                obj.GetComponent<InventoryCardUI>();

            ui.SetCard(instance, this);
            displayedCards.Add(ui);

            if (isCombined)
            {
                CombinedCardUI combinedUI =
                    obj.GetComponent<CombinedCardUI>();

                combinedUI.SetCard(instance, this);
            }
        }
    }
    public void BeginDrag(InventoryCardUI card)
    {
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

    // 카드 연결
    public void TryConnect(
    InventoryCardUI parentUI,
    InventoryCardUI childUI)
    {
        CardInstance parent = parentUI.CardInstance;
        CardInstance child = childUI.CardInstance;

        // 합성 카드라면 마지막 카드까지 이동
        while (parent.childCard != null)
            parent = parent.childCard;

        bool success = combinationManager.TryConnect(parent, child);

        if (!success)
        {
            Debug.Log("유효하지 않은 카드 조합입니다.");
            return;
        }

        // 연결 성공 후 물리 계수 갱신
        GameManager.Instance.uiManager.RecalculatePhysicsCoefficients();

       
        // 드래그 종료 후 UI 갱신
        needsRefresh = true;
    }

    // 카드 연결 해제
    public void DisconnectCard(CardInstance child)
    {
        if (child == null || child.parentCard == null)
            return;

        combinationManager.Disconnect(child);

        // 연결 해제 후 물리 계수 다시 계산
        GameManager.Instance.uiManager.RecalculatePhysicsCoefficients();

        RefreshInventory();
    }

    // 현재 UI에 등록된 카드 목록 반환
    private List<CardInstance> GetInventoryList()
    {
        return new List<CardInstance>(uiDictionary.Keys);
    }
}
