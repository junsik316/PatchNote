using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// A draggable combined card whose direct children are flat visual segments.
// CardInstance.parentCard / childCard are independent of the UI hierarchy.
public class CombinedCardUI : MonoBehaviour
{
    [Header("블록 생성")]
    public CombinedBlockUI blockPrefab;

    private readonly List<CombinedBlockUI> renderedBlocks =
        new List<CombinedBlockUI>();

    public static int CountChain(CardInstance root)
    {
        var visited = new HashSet<CardInstance>();
        int count = 0;
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

    public void SetCard(CardInstance root, InventoryUI owner)
    {
        if (root == null || owner == null || blockPrefab == null)
        {
            Debug.LogError("합성 카드 UI 참조가 누락되었습니다.", this);
            return;
        }

        // The root receives drag/drop raycasts. Blocks are only visual children.
        Image dropArea = GetComponent<Image>();
        if (dropArea != null)
        {
            Color tint = dropArea.color;
            tint.a = 0f;
            dropArea.color = tint;
            dropArea.raycastTarget = true;
        }

        foreach (CombinedBlockUI oldBlock in renderedBlocks)
        {
            if (oldBlock == null)
                continue;
            oldBlock.gameObject.SetActive(false);
            Destroy(oldBlock.gameObject);
        }
        renderedBlocks.Clear();

        int index = 0;
        CardInstance current = root;
        var visited = new HashSet<CardInstance>();
        while (current != null && visited.Add(current) && index < 128)
        {
            CombinedBlockUI block = Instantiate(blockPrefab, transform, false);
            block.gameObject.name = string.Format("Block {0:00} - {1}",
                index + 1, CardInstanceDisplay.Title(current));

            RectTransform rect = block.transform as RectTransform;
            if (rect != null)
            {
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(1f, 1f);
                rect.pivot = new Vector2(0.5f, 1f);
                rect.sizeDelta = new Vector2(0f, ScratchBlockGraphic.BlockHeight);
                rect.anchoredPosition = new Vector2(0f,
                    -index * (ScratchBlockGraphic.BlockHeight -
                        ScratchBlockGraphic.ConnectorDepth));
            }

            block.SetBlock(current, owner, index > 0, current.childCard != null);
            renderedBlocks.Add(block);
            index++;
            current = current.childCard;
        }

        if (current != null)
            Debug.LogWarning("카드 체인 순환 또는 길이 제한으로 표시를 중단했습니다.", this);
    }
}
