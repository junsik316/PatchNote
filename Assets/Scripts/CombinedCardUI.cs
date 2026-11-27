using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// A combined chain is rendered as ONE draggable inventory item.
public class CombinedCardUI : MonoBehaviour
{
    [Header("블록 생성")]
    public CombinedBlockUI blockPrefab;
    public Transform blockContainer;

    private readonly List<CombinedBlockUI> renderedBlocks =
        new List<CombinedBlockUI>();

    public static int CountChain(CardInstance root)
    {
        int count = 0;
        var visited = new HashSet<CardInstance>();
        CardInstance current = root;
        while (current != null && visited.Add(current) && count < 128)
        {
            count++;
            current = current.childCard;
        }
        return count;
    }

    public static float ChainHeight(int count)
    {
        return count <= 0 ? 0f :
            count * ScratchBlockGraphic.BlockHeight -
            (count - 1) * ScratchBlockGraphic.ConnectorDepth;
    }

    public void SetCard(CardInstance rootCard, InventoryUI owner)
    {
        if (rootCard == null || owner == null ||
            blockPrefab == null || blockContainer == null)
        {
            Debug.LogError("합성 카드 UI 참조가 누락되었습니다.", this);
            return;
        }

        VerticalLayoutGroup rootLayout = GetComponent<VerticalLayoutGroup>();
        if (rootLayout != null)
            rootLayout.enabled = false;

        ContentSizeFitter rootFitter = GetComponent<ContentSizeFitter>();
        if (rootFitter != null)
            rootFitter.enabled = false;

        Image hitArea = GetComponent<Image>();
        if (hitArea != null)
        {
            Color tint = hitArea.color;
            tint.a = 0f;
            hitArea.color = tint;
            hitArea.raycastTarget = true;
        }

        foreach (CombinedBlockUI block in renderedBlocks)
        {
            if (block == null)
                continue;
            block.gameObject.SetActive(false);
            Destroy(block.gameObject);
        }
        renderedBlocks.Clear();

        VerticalLayoutGroup stack = blockContainer.GetComponent<VerticalLayoutGroup>();
        if (stack == null)
            stack = blockContainer.gameObject.AddComponent<VerticalLayoutGroup>();
        stack.padding = new RectOffset(0, 0, 0, 0);
        stack.spacing = -ScratchBlockGraphic.ConnectorDepth;
        stack.childAlignment = TextAnchor.UpperCenter;
        stack.childControlWidth = true;
        stack.childControlHeight = false;
        stack.childForceExpandWidth = true;
        stack.childForceExpandHeight = false;

        RectTransform containerRect = blockContainer as RectTransform;
        if (containerRect != null)
        {
            containerRect.anchorMin = new Vector2(0f, 1f);
            containerRect.anchorMax = new Vector2(1f, 1f);
            containerRect.pivot = new Vector2(0.5f, 1f);
            containerRect.anchoredPosition = Vector2.zero;
            containerRect.sizeDelta =
                new Vector2(0f, ChainHeight(CountChain(rootCard)));
        }

        var visited = new HashSet<CardInstance>();
        CardInstance current = rootCard;
        while (current != null && visited.Add(current) && renderedBlocks.Count < 128)
        {
            CombinedBlockUI block = Instantiate(blockPrefab, blockContainer);
            RectTransform rect = block.transform as RectTransform;
            if (rect != null)
            {
                rect.anchorMin = new Vector2(0f, 0.5f);
                rect.anchorMax = new Vector2(1f, 0.5f);
                rect.sizeDelta =
                    new Vector2(0f, ScratchBlockGraphic.BlockHeight);
            }

            LayoutElement element = block.GetComponent<LayoutElement>();
            if (element == null)
                element = block.gameObject.AddComponent<LayoutElement>();
            element.minHeight = ScratchBlockGraphic.BlockHeight;
            element.preferredHeight = ScratchBlockGraphic.BlockHeight;
            element.flexibleHeight = 0f;

            block.SetBlock(current, owner,
                current != rootCard, current.childCard != null);
            renderedBlocks.Add(block);
            current = current.childCard;
        }

        if (current != null)
            Debug.LogWarning("카드 체인 순환 또는 길이 제한으로 표시를 중단했습니다.", this);
        if (containerRect != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(containerRect);
    }
}
