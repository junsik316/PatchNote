using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("UI 컴포넌트")]
    public InventoryUI inventoryUI;
    public CardSelectionUI cardSelectionUI;

    [Header("인벤토리 데이터")]
    public List<CardInstance> inventory =
        new List<CardInstance>();

    private float previousTimeScale;

    // 인벤토리에 카드 추가
    public void AddCard(CardInstance instance)
    {
        inventory.Add(instance);

        RecalculatePhysicsCoefficients();

        if (inventoryUI.IsOpen)
            inventoryUI.OpenInventory(inventory);
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
        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        inventoryUI.OpenInventory(inventory);
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
        PlayerPhysicsEffects player =
            FindFirstObjectByType<PlayerPhysicsEffects>();

        if (player == null)
            return;

        player.Recalculate(inventory);
    }

}