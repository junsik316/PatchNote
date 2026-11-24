
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

        // 카드 정보 표시
        cardName.text = GetCardName(instance);

        if (cardIcon != null)
        {
            Sprite sprite = instance.cardData.cardImage;

            cardIcon.gameObject.SetActive(sprite != null);

            if (sprite != null)
                cardIcon.sprite = sprite;
        }

        // 카드 종류별 색상
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
        }

        // 루트 카드에는 해제 버튼이 없음
        bool canUnlink = instance.parentCard != null;

        unlinkButton.gameObject.SetActive(canUnlink);
        unlinkButton.onClick.RemoveAllListeners();

        if (canUnlink)
        {
            unlinkButton.onClick.AddListener(() =>
            {
                inventoryUI.DisconnectCard(cardInstance);
            });
        }

        // 자식 카드가 있다면 블록 안에 생성
        if (instance.childCard != null)
        {
            childSlot.gameObject.SetActive(true);

            CombinedBlockUI childBlock = Instantiate(
                blockPrefab,
                childSlot
            );

            childBlock.SetBlock(
                instance.childCard,
                owner,
                blockPrefab
            );
        }
        else
        {
            childSlot.gameObject.SetActive(false);
        }
    }

    private string GetCardName(CardInstance instance)
    {
        if (instance.cardType == CardType.Number)
        {
            string op = instance.cardOperator.ToString();

            switch (op)
            {
                case "Multiply":
                    op = "*";
                    break;
                case "Divide":
                    op = "/";
                    break;
                case "Add":
                    op = "+";
                    break;
                case "Subtract":
                    op = "-";
                    break;
            }

            return op + " " + instance.numberValue.ToString("0.##");
        }

        if (instance.cardType == CardType.Physics)
            return instance.physicsType;

        if (instance.cardType == CardType.Stat)
            return instance.statType;

        return instance.cardData.cardName;
    }
}
