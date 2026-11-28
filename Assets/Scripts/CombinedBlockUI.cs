using TMPro;
using UnityEngine;
using UnityEngine.UI;

// One visible card segment. The parent/child chain lives only in CardInstance.
// Icon, CardName, and UnlinkButton are direct children in CombinedBlock.prefab.
public class CombinedBlockUI : MonoBehaviour
{
    [Header("블록 UI")]
    public Image cardIcon;
    public Text cardName;
    public Button unlinkButton;

    [Header("카드 종류별 색상")]
    public Color numberColor = new Color(0.25f, 0.55f, 0.9f);
    public Color physicsColor = new Color(0.65f, 0.4f, 0.85f);
    public Color statColor = new Color(0.25f, 0.7f, 0.5f);

    public void SetBlock(CardInstance instance, InventoryUI owner,
        bool hasParent, bool hasChild)
    {
        if (instance == null)
            return;

        ScratchBlockGraphic graphic = GetComponent<ScratchBlockGraphic>();
        if (graphic == null)
            graphic = gameObject.AddComponent<ScratchBlockGraphic>();

        graphic.color = BlockColor(instance);
        graphic.SetConnectors(hasParent, hasChild);
        graphic.raycastTarget = false;

        if (cardIcon != null)
        {
            Sprite sprite = instance.cardData != null
                ? instance.cardData.cardImage : null;
            cardIcon.gameObject.SetActive(sprite != null);
            if (sprite != null)
                cardIcon.sprite = sprite;
            cardIcon.raycastTarget = false;
            cardIcon.preserveAspect = true;

            RectTransform rect = cardIcon.rectTransform;
            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.anchorMax = new Vector2(0f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(27f, 0f);
            rect.sizeDelta = new Vector2(34f, 34f);
        }

        if (cardName != null)
        {
            cardName.text = CardInstanceDisplay.Title(instance);
            cardName.alignment = TextAnchor.MiddleLeft;
            cardName.color = Color.white;
            cardName.fontSize = 17;
            cardName.resizeTextForBestFit = true;
            cardName.resizeTextMinSize = 11;
            cardName.resizeTextMaxSize = 17;
            cardName.raycastTarget = false;

            RectTransform rect = cardName.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(54f, 11f);
            rect.offsetMax = new Vector2(-41f, -11f);
        }

        if (unlinkButton != null)
        {
            unlinkButton.onClick.RemoveAllListeners();
            unlinkButton.gameObject.SetActive(hasParent);
            if (hasParent && owner != null)
                unlinkButton.onClick.AddListener(() => owner.DisconnectCard(instance));

            RectTransform rect = unlinkButton.transform as RectTransform;
            if (rect != null)
            {
                rect.anchorMin = new Vector2(1f, 0.5f);
                rect.anchorMax = new Vector2(1f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = new Vector2(-20f, 0f);
                rect.sizeDelta = new Vector2(29f, 29f);
            }

            Image image = unlinkButton.GetComponent<Image>();
            if (image != null)
            {
                image.color = new Color(0.08f, 0.10f, 0.16f, 0.8f);
                image.raycastTarget = true;
            }

            TMP_Text label = unlinkButton.GetComponentInChildren<TMP_Text>(true);
            if (label != null)
            {
                label.text = "×";
                label.fontSize = 22;
                label.alignment = TextAlignmentOptions.Center;
                label.raycastTarget = false;
            }
        }
    }

    private Color BlockColor(CardInstance instance)
    {
        switch (instance.cardType)
        {
            case CardType.Number: return numberColor;
            case CardType.Physics: return physicsColor;
            case CardType.Stat: return statColor;
            case CardType.Special: return new Color(0.88f, 0.59f, 0.24f);
            case CardType.Target: return new Color(0.79f, 0.38f, 0.40f);
            default: return Color.gray;
        }
    }
}

// Display values come from individual CardInstance objects, never templates.
public static class CardInstanceDisplay
{
    public static string Title(CardInstance instance)
    {
        if (instance == null)
            return "?";

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

        return instance.cardData != null ? instance.cardData.cardName : "?";
    }

    public static Color ColorFor(CardInstance instance)
    {
        if (instance == null)
            return Color.gray;

        switch (instance.cardType)
        {
            case CardType.Number: return new Color(0.25f, 0.55f, 0.9f);
            case CardType.Physics: return new Color(0.65f, 0.4f, 0.85f);
            case CardType.Stat: return new Color(0.25f, 0.7f, 0.5f);
            case CardType.Special: return new Color(0.88f, 0.59f, 0.24f);
            case CardType.Target: return new Color(0.79f, 0.38f, 0.40f);
            default: return Color.gray;
        }
    }
}
