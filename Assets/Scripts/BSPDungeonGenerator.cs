using UnityEngine;
using UnityEngine.Tilemaps;

public class BSPDungeonGenerator : MonoBehaviour
{
    [Header("Map")]
    public int mapWidth = 50;
    public int mapHeight = 50;

    [Header("BSP")]
    public int minPartitionSize = 12;
    public int minRoomSize = 4;

    [Header("Tilemap")]
    public Tilemap tilemap;
    public TileBase floorTile;
    public TileBase wallTile;

    private int[,] map;

    class BSPNode
    {
        public RectInt area;

        public BSPNode left;
        public BSPNode right;

        public RectInt room;

        public BSPNode(RectInt area)
        {
            this.area = area;
        }

        public bool IsLeaf()
        {
            return left == null && right == null;
        }
    }

    void Start()
    {
        GenerateDungeon();
    }

    public void GenerateDungeon()
    {
        map = new int[mapWidth, mapHeight];

        BSPNode root =
            new BSPNode(
                new RectInt(
                    0,
                    0,
                    mapWidth,
                    mapHeight));

        Split(root);

        CreateRooms(root);

        ConnectRooms(root);

        DrawRooms(root);

        DrawTilemap();
    }

    void Split(BSPNode node)
    {
        if (node.area.width < minPartitionSize * 2 &&
            node.area.height < minPartitionSize * 2)
        {
            return;
        }

        bool splitHorizontal;

        if (node.area.width > node.area.height)
            splitHorizontal = false;
        else if (node.area.height > node.area.width)
            splitHorizontal = true;
        else
            splitHorizontal = Random.value > 0.5f;

        if (splitHorizontal)
        {
            if (node.area.height < minPartitionSize * 2)
                return;

            int split =
                Random.Range(
                    minPartitionSize,
                    node.area.height - minPartitionSize);

            node.left =
                new BSPNode(
                    new RectInt(
                        node.area.x,
                        node.area.y,
                        node.area.width,
                        split));

            node.right =
                new BSPNode(
                    new RectInt(
                        node.area.x,
                        node.area.y + split,
                        node.area.width,
                        node.area.height - split));
        }
        else
        {
            if (node.area.width < minPartitionSize * 2)
                return;

            int split =
                Random.Range(
                    minPartitionSize,
                    node.area.width - minPartitionSize);

            node.left =
                new BSPNode(
                    new RectInt(
                        node.area.x,
                        node.area.y,
                        split,
                        node.area.height));

            node.right =
                new BSPNode(
                    new RectInt(
                        node.area.x + split,
                        node.area.y,
                        node.area.width - split,
                        node.area.height));
        }

        Split(node.left);
        Split(node.right);
    }

    void CreateRooms(BSPNode node)
    {
        if (!node.IsLeaf())
        {
            CreateRooms(node.left);
            CreateRooms(node.right);
            return;
        }

        int margin = 1;

        int maxWidth =
            node.area.width - margin * 2;

        int maxHeight =
            node.area.height - margin * 2;

        if (maxWidth < minRoomSize ||
           maxHeight < minRoomSize)
            return;

        int roomWidth =
            Random.Range(
                minRoomSize,
                maxWidth + 1);

        int roomHeight =
            Random.Range(
                minRoomSize,
                maxHeight + 1);

        int roomX =
            Random.Range(
                node.area.x + margin,
                node.area.xMax - roomWidth - margin + 1);

        int roomY =
            Random.Range(
                node.area.y + margin,
                node.area.yMax - roomHeight - margin + 1);

        node.room =
            new RectInt(
                roomX,
                roomY,
                roomWidth,
                roomHeight);
        Debug.Log(node.room);
    }

    void DrawRooms(BSPNode node)
    {
        if (!node.IsLeaf())
        {
            DrawRooms(node.left);
            DrawRooms(node.right);
            return;
        }

        for (int x = node.room.x; x < node.room.xMax; x++)
        {
            for (int y = node.room.y; y < node.room.yMax; y++)
            {
                map[x, y] = 1;
            }
        }
    }

    RectInt GetRoom(BSPNode node)
    {
        if (node.IsLeaf())
            return node.room;

        if (Random.value > 0.5f)
            return GetRoom(node.left);

        return GetRoom(node.right);
    }

    Vector2Int GetCenter(RectInt room)
    {
        return new Vector2Int(
            room.x + room.width / 2,
            room.y + room.height / 2);
    }

    void ConnectRooms(BSPNode node)
    {
        if (node == null)
            return;

        if (node.IsLeaf())
            return;

        ConnectRooms(node.left);
        ConnectRooms(node.right);

        RectInt roomA = GetRoom(node.left);
        RectInt roomB = GetRoom(node.right);

        Vector2Int centerA = GetCenter(roomA);
        Vector2Int centerB = GetCenter(roomB);

        CreateCorridor(centerA, centerB);
    }

    void CreateCorridor(
        Vector2Int start,
        Vector2Int end)
    {
        int x = start.x;
        int y = start.y;

        while (x != end.x)
        {
            map[x, y] = 1;

            if (end.x > x)
                x++;
            else
                x--;
        }

        while (y != end.y)
        {
            map[x, y] = 1;

            if (end.y > y)
                y++;
            else
                y--;
        }

        map[end.x, end.y] = 1;
    }

    void DrawTilemap()
    {
        tilemap.ClearAllTiles();

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                Vector3Int pos =
                    new Vector3Int(x, y, 0);

                if (map[x, y] == 1)
                {
                    tilemap.SetTile(
                        pos,
                        floorTile);
                }
                else
                {
                    tilemap.SetTile(
                        pos,
                        wallTile);
                }
            }
        }
    }
}