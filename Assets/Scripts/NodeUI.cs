using UnityEngine;
using UnityEngine.UI;

public class NodeUI : MonoBehaviour
{
    public Image icon;

    public Sprite commonSprite;
    public Sprite hardSprite;
    public Sprite eliteSprite;
    public Sprite rewardSprite;
    public Sprite bossSprite;
    private MapTree.Node currentNode;

    public void SetNode(MapTree.Node node)
    {
        currentNode = node;
        switch (node.type)
        {
            case MapTree.NodeType.common:
                icon.sprite = commonSprite;
                break;

            case MapTree.NodeType.hard:
                icon.sprite = hardSprite;
                break;

            case MapTree.NodeType.elite:
                icon.sprite = eliteSprite;
                break;

            case MapTree.NodeType.reward:
                icon.sprite = rewardSprite;
                break;

            case MapTree.NodeType.boss:
                icon.sprite = bossSprite;
                break;
        }
    }

    

    
    public MapTree.Node GetNode()
    {
        return currentNode;
    }
}