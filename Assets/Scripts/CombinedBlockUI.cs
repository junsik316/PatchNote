using TMPro;
using UnityEngine;
using UnityEngine.UI;

// A single visual segment. CombinedCardUI arranges these as a flat stack;
// the logical links are always owned by CardInstance, not by the UI.
public class CombinedBlockUI : MonoBehaviour
{
    [Header("블록 UI")]
    public Image background;
    public Image cardIcon;
    public Text cardName;
    public Button unlinkButton;

    [Header("기존 프리팹 호환용 (새 방식에서는 사용하지 않음)")]
    public Transform childSlot;

    [Header("카드 종류별 색상")]
    public Color numberColor = new Color(0.25f, 0.55f, 0.9f);
    public Color physicsColor = new Color(0.65f, 0.4f, 0.85f);
    public Color statColor = new Color(0.25f, 0.7f, 0.5f);

    public void SetBlock(CardInstance instance, InventoryUI owner,
        bool hasParent, bool hasChild)
    {
        if (instance == null)
            return;

        ScratchBlockGraphic shape = GetComponent<ScratchBlockGraphic>();
        if (shape == null)
            shape = gameObject.AddComponent<ScratchBlockGraphic>();

        shape.raycastTarget = false;
        shape.color = GetBlockColor(instance);
        shape.SetConnectors(hasParent, hasChild);

        // The old prefab has a nested slot and a square background. Keep
        // their serialized references, but render segments side by side in
        // one VerticalLayoutGroup instead of nesting rectangles recursively.
        if (childSlot != null)
            childSlot.gameObject.SetActive(false);

        if (background != null)
        {
            background.enabled = false;
            RectTransform header = background.transform.parent as RectTransform;
            if (header != null)
            {
                header.anchorMin = Vector2.zero;
                header.anchorMax = Vector2.one;
                header.offsetMin = Vector2.zero;
                header.offsetMax = Vector2.zero;

                Graphic[] graphics = header.GetComponentsInChildren<Graphic>(true);
                foreach (Graphic graphic in graphics)
                    graphic.raycastTarget = false;
            }
        }

        if (cardIcon != null)
        {
            Sprite sprite = instance.cardData != null
                ? instance.cardData.cardImage : null;
            cardIcon.gameObject.SetActive(sprite != null);
            if (sprite != null)
                cardIcon.sprite = sprite;
            cardIcon.raycastTarget = false;

            RectTransform iconRect = cardIcon.rectTransform;
            iconRect.anchorMin = new Vector2(0f, 0.5f);
            iconRect.anchorMax = new Vector2(0f, 0.5f);
            iconRect.pivot = new Vector2(0.5f, 0.5f);
            iconRect.anchoredPosition = new Vector2(27f, 0f);
            iconRect.sizeDelta = new Vector2(34f, 34f);
            cardIcon.preserveAspect = true;
        }

        if (cardName != null)
        {
            cardName.text = CardInstanceDisplay.Title(instance);
            cardName.alignment = TextAnchor.MiddleLeft;
            cardName.fontSize = 17;
            cardName.resizeTextForBestFit = true;
            cardName.resizeTextMinSize = 11;
            cardName.resizeTextMaxSize = 17;
            cardName.color = Color.white;
            cardName.raycastTarget = false;

            RectTransform nameRect = cardName.rectTransform;
            nameRect.anchorMin = Vector2.zero;
            nameRect.anchorMax = Vector2.one;
            nameRect.offsetMin = new Vector2(54f, 11f);
            nameRect.offsetMax = new Vector2(-41f, -11f);
        }

        if (unlinkButton != null)
        {
            unlinkButton.onClick.RemoveAllListeners();
            unlinkButton.gameObject.SetActive(hasParent);
            if (hasParent)
                unlinkButton.onClick.AddListener(() => owner.DisconnectCard(instance));

            RectTransform unlinkRect = unlinkButton.transform as RectTransform;
            if (unlinkRect != null)
            {
                unlinkRect.anchorMin = new Vector2(1f, 0.5f);
                unlinkRect.anchorMax = new Vector2(1f, 0.5f);
                unlinkRect.pivot = new Vector2(0.5f, 0.5f);
                unlinkRect.anchoredPosition = new Vector2(-20f, 0f);
                unlinkRect.sizeDelta = new Vector2(29f, 29f);
            }

            Image unlinkImage = unlinkButton.GetComponent<Image>();
            if (unlinkImage != null)
            {
                unlinkImage.color = new Color(0.08f, 0.10f, 0.16f, 0.8f);
                unlinkImage.raycastTarget = true;
            }

            TMP_Text buttonLabel = unlinkButton.GetComponentInChildren<TMP_Text>(true);
            if (buttonLabel != null)
            {
                buttonLabel.text = "×";
                buttonLabel.fontSize = 22f;
                buttonLabel.alignment = TextAlignmentOptions.Center;
                buttonLabel.raycastTarget = false;
            }
            else
            {
                Text legacyLabel = unlinkButton.GetComponentInChildren<Text>(true);
                if (legacyLabel != null)
                {
                    legacyLabel.text = "×";
                    legacyLabel.alignment = TextAnchor.MiddleCenter;
                    legacyLabel.raycastTarget = false;
                }
            }
        }
    }

    private Color GetBlockColor(CardInstance instance)
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

// Both individual and combined cards display the VALUE of an instance,
// rather than only its ScriptableObject template name.
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
