using UnityEngine;

public class Door : MonoBehaviour
{
    public bool isOpen = false;

    private Collider2D doorCollider;
    private SpriteRenderer spriteRenderer;

    public MapTree.Node targetRoom;

    private RoomMaker roomMaker;
    private MapManager mapManager;

    private void Awake()
    {
        doorCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        UpdateDoor();
    }

    public void Initialize(
        MapTree.Node node,
        RoomMaker maker,
        MapManager manager)
    {
        targetRoom = node;
        roomMaker = maker;
        mapManager = manager;
    }

    public void OpenDoor()
    {
        isOpen = true;
        UpdateDoor();
    }

    public void CloseDoor()
    {
        isOpen = false;
        UpdateDoor();
    }

    private void UpdateDoor()
    {
        // 트리거 Collider는 계속 활성화
        if (doorCollider != null)
        {
            doorCollider.enabled = true;
            doorCollider.isTrigger = true;
        }

        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = isOpen ? 0.4f : 1f;
            spriteRenderer.color = color;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isOpen)
            return;

        if (!other.CompareTag("Player"))
            return;

        if (targetRoom == null || roomMaker == null)
            return;

        // 현재 방 정보 갱신
        if (mapManager != null)
        {
            mapManager.SetCurrentRoom(targetRoom);
        }

        
    }
}