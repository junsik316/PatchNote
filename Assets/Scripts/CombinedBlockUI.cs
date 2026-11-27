using UnityEngine;
using UnityEngine.UI;

public class CombinedBlockUI : MonoBehaviour
{
    [Header("블록 UI")]
    public Image background;
    public Image cardIcon;
    public Text cardName;
    public Button unlinkButton;

    [Header("자식 블록 영역")]
    public Transform childSlot;

    [Header("카드 종류별 색상")]
    public Color numberColor = new Color(0.25f, 0.55f, 0.9f);
    public Color physicsColor = new Color(0.65f, 0.4f, 0.85f);
    public Color statColor = new Color(0.25f, 0.7f, 0.5f);

    private CardInstance cardInstance;
    private InventoryUI inventoryUI;

    public void SetBlock(
        CardInstance instance,
        InventoryUI owner,
        CombinedBlockUI blockPrefab)
    {
        cardInstance = instance;
        inventoryUI = owner;

        cardName.text = GetCardName(instance);
        if (cardIcon != null)
        {
            Sprite sprite = instance.cardData.cardImage;
            cardIcon.gameObject.SetActive(sprite != null);
            if (sprite != null)
                cardIcon.sprite = sprite;
        }

        // 기존 프리팹의 색상 필드/참조는 변경하지 않는다.
        switch (instance.cardType)
        {
            case CardType.Number:
                background.color = numberColor;
                break;
            case CardType.Physics:
                background.color = physicsColor;
                break;
            case CardType.Stat:
                background.color = statColor;
                break;
                // Special/Target의 색상은 UI 설계 단계에서 추가.
        }

        bool canUnlink = instance.parentCard != null;
        unlinkButton.gameObject.SetActive(canUnlink);
        unlinkButton.onClick.RemoveAllListeners();
        if (canUnlink)
            unlinkButton.onClick.AddListener(() => inventoryUI.DisconnectCard(cardInstance));

        if (instance.childCard != null)
        {
            childSlot.gameObject.SetActive(true);
            CombinedBlockUI childBlock = Instantiate(blockPrefab, childSlot);
            childBlock.SetBlock(instance.childCard, owner, blockPrefab);
        }
        else
        {
            childSlot.gameObject.SetActive(false);
        }
    }

    private string GetCardName(CardInstance instance)
    {
        if (instance is NumberCardInstance number)
        {
            string op;
            switch (number.cardOperator)
            {
                case CardOperator.Multiply: op = "×"; break;
                case CardOperator.Divide: op = "÷"; break;
                case CardOperator.Add: op = "+"; break;
                case CardOperator.Subtract: op = "−"; break;
                default: op = number.cardOperator.ToString(); break;
            }
            return op + " " + number.numberValue.ToString("0.##");
        }
        if (instance is PhysicsCardInstance physics)
            return physics.physicsType.ToString();
        if (instance is StatCardInstance stat)
            return stat.statType.ToString();
        if (instance is SpecialCardInstance special)
            return special.specialType.ToString();
        if (instance is TargetCardInstance target)
            return target.target.ToString();
        return instance.cardData.cardName;
    }
}
