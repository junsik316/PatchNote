
using UnityEngine;

public class CombinedCardUI : MonoBehaviour
{
    [Header("블록 생성")]
    public CombinedBlockUI blockPrefab;
    public Transform blockContainer;

    private CombinedBlockUI rootBlock;

    public void SetCard(
        CardInstance rootCard,
        InventoryUI owner)
    {
        if (rootBlock != null)
            Destroy(rootBlock.gameObject);

        // 합성 카드의 첫 번째 블록 생성
        rootBlock = Instantiate(
            blockPrefab,
            blockContainer
        );

        // 연결된 모든 카드를 재귀적으로 표시
        rootBlock.SetBlock(
            rootCard,
            owner,
            blockPrefab
        );
    }
}
