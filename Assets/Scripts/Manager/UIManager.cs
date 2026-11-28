using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("UI 컴포넌트")]
    public InventoryUI inventoryUI;
    public CardSelectionUI cardSelectionUI;

    // The persistent CardManager owns the cards.
    private CardManager subscribedManager;

    private float previousTimeScale;

    private void OnEnable() { BindCardManager(); }
    private void Start()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.uiManager = this;
        BindCardManager();
    }
    private void OnDisable()
    {
        if (subscribedManager != null)
            subscribedManager.InventoryChanged -= OnInventoryChanged;
        subscribedManager = null;
    }
    private void BindCardManager()
    {
        CardManager manager = CardManager.Instance;
        if (subscribedManager == manager) return;
        if (subscribedManager != null)
            subscribedManager.InventoryChanged -= OnInventoryChanged;
        subscribedManager = manager;
        if (subscribedManager != null)
            subscribedManager.InventoryChanged += OnInventoryChanged;
    }
    private void OnInventoryChanged()
    {
        if (subscribedManager != null && inventoryUI != null && inventoryUI.IsOpen)
            inventoryUI.OpenInventory(subscribedManager.Inventory);
    }

    // 인벤토리에 카드 추가
    public void AddCard(CardInstance instance)
    {
        BindCardManager();
        if (subscribedManager != null)
            subscribedManager.AddCard(instance);
    }

    public void ToggleInventory()
    {
        if (cardSelectionUI.IsOpen)
            return;

        if (inventoryUI.IsOpen)
        {
            CloseInventory();
        }
        else
        {
            OpenInventory();
        }
    }

    private void OpenInventory()
    {
        BindCardManager();
        if (subscribedManager == null) return;
        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        inventoryUI.OpenInventory(subscribedManager.Inventory);
    }

    private void CloseInventory()
    {
        inventoryUI.CloseInventory();
        Time.timeScale = previousTimeScale;
    }

    public void ShowCardSelection(List<CardInstance> cards)
    {
        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        cardSelectionUI.ShowCards(cards);
    }

    public void CloseCardSelection()
    {
        cardSelectionUI.Close();
        Time.timeScale = previousTimeScale;
    }

    public void RecalculatePhysicsCoefficients()
    {
        BindCardManager();
        if (subscribedManager != null)
            subscribedManager.RefreshEffects(EffectTarget.Player);
    }

}