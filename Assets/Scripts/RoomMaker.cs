using JetBrains.Annotations;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using static RoomMaker.Platform;

public class RoomMaker : MonoBehaviour
{
    //방 안의 플랫폼의 정보를 담는 클래스
    [System.Serializable]
    public class Platform
    {
        public enum PlatformType
        {
            Normal,
            Door
        }

        public enum PlatformElement
        {
            None,
            Enemy,
            Item
        }

        public List<PlatformElement> elements =
            new List<PlatformElement>();

        public PlatformType type;

        public Vector2Int position;

        public int width;

        // 문 플랫폼이 연결하는 방
        public MapTree.Node targetNode;

        public Platform(
            Vector2Int position,
            int width,
            PlatformType type)
        {
            this.position = position;
            this.width = width;
            this.type = type;
        }
    }
    
    public MapTree mapTree;
    private MapTree.Node StartNode;

    public Tilemap tilemap;
    public TileSet tileset;
    public Vector2Int Mapsize = new Vector2Int(80, 100);

    //방 안의 플랫폼을 담는 리스트
    public List<Platform> platforms = new List<Platform>();

    public int StartPlatformWidth = 10;

    public int NormalPlatformMinWidth = 5;
    public int NormalPlatformMaxWidth = 9;

    public int DoorPlatformWidth = 8;


    // 플랫폼 사이의 최대 점프 거리
    public int MaxJumpDistance = 15;
    public int MaxJumpHeight = 12;

    // 플랫폼 사이의 최소 높이 차이
    public int MinPlatformHeight = 6;

    // 플랫폼이 생성될 수 있는 맵의 여백
    public int PlatformMargin = 5;

    // 플랫폼이 실제로 생성될 중심 좌표
    public List<Vector2Int> platformPositions =
        new List<Vector2Int>();

    // Grid 크기
    public int GridCellWidth = 15;
    public int GridCellHeight = 12;

    // Grid 중심에서 얼마나 랜덤하게 움직일지
    public int PositionJitterX = 4;
    public int PositionJitterY = 3;



    public MapManager mapManager;

    // 현재 생성 중인 방
    private MapTree.Node currentNode;

    

    // 현재 방에서 생성한 문들
    private List<GameObject> spawnedDoors =
        new List<GameObject>();

    public GameObject doorPrefab;
    //시작 지점 생성
    private void MakeStartRoom(MapTree.Node node)
    {
        Debug.Log("Making Start Room");

        BuildRoom(node);
    }

    private void MakeRoom(MapTree.Node node)
    {
        Debug.Log("Making Room at node: " + node.type);
        Debug.Log("is MainRoute: " + node.isMainRoute);

        BuildRoom(node);
    }

   


    //플랫폼 생성
    private void GeneratePlatformData()
    {
        platforms.Clear();
        platformPositions.Clear();

        // 1. 플랫폼 위치 생성
        MakePlatformPositions();

        // 2. 실제 플랫폼으로 변환
        MakePlatformsFromPositions();

        // 3. 연결된 방에 따라 문 플랫폼 지정
        MakeDoorPlatform();

        // 4. 적과 아이템 배치
        MakePlatformElements();
    }

    private void MakePlatformPositions()
    {
        int startX = PlatformMargin;
        int endX = Mapsize.x - PlatformMargin;

        int startY = PlatformMargin;
        int endY = Mapsize.y - PlatformMargin;

        // 시작 위치는 맵 아래쪽 중앙
        Vector2Int startPosition = new Vector2Int(
            Mapsize.x / 2,
            startY
        );

        platformPositions.Add(startPosition);

        for (int y = startY; y < endY; y += GridCellHeight)
        {
            for (int x = startX; x < endX; x += GridCellWidth)
            {
                int centerX = x + GridCellWidth / 2;
                int centerY = y + GridCellHeight / 2;

                // Grid 중심에서 랜덤하게 이동
                int randomX = Random.Range(
                    -PositionJitterX,
                    PositionJitterX + 1
                );

                int randomY = Random.Range(
                    -PositionJitterY,
                    PositionJitterY + 1
                );

                int finalX = centerX + randomX;
                int finalY = centerY + randomY;

                // 맵 밖으로 나가지 않도록 제한
                finalX = Mathf.Clamp(
                    finalX,
                    PlatformMargin,
                    Mapsize.x - PlatformMargin
                );

                finalY = Mathf.Clamp(
                    finalY,
                    PlatformMargin,
                    Mapsize.y - PlatformMargin
                );

                Vector2Int position =
                    new Vector2Int(finalX, finalY);

                // 시작점과 너무 가까운 Grid 점은 제외
                if (Vector2Int.Distance(
                    position,
                    startPosition) < GridCellHeight)
                {
                    continue;
                }

                platformPositions.Add(position);
            }
        }
    }

    private void MakePlatformsFromPositions()
    {
        for (int i = 0; i < platformPositions.Count; i++)
        {
            Vector2Int position =
                platformPositions[i];

            int width;

            // 첫 번째는 시작 플랫폼
            if (i == 0)
            {
                width = StartPlatformWidth;
            }
            else
            {
                width = Random.Range(
                    NormalPlatformMinWidth,
                    NormalPlatformMaxWidth + 1
                );
            }

            Platform platform = new Platform(
                position,
                width,
                Platform.PlatformType.Normal
            );

            platforms.Add(platform);
        }
    }

    private void MakeDoorPlatform()
    {
        if (currentNode == null)
            return;

        List<MapTree.Node> connectedNodes =
            currentNode.connections;

        if (connectedNodes == null ||
            connectedNodes.Count == 0)
            return;

        List<Platform> possiblePlatforms =
            new List<Platform>();

        foreach (Platform platform in platforms)
        {
            // 시작 플랫폼 제외
            if (platform == platforms[0])
                continue;

            // 높은 위치의 플랫폼 우선 선택
            if (platform.position.y >= Mapsize.y * 0.35f)
            {
                possiblePlatforms.Add(platform);
            }
        }

        // 후보가 부족하면 나머지 플랫폼도 추가
        if (possiblePlatforms.Count < connectedNodes.Count)
        {
            foreach (Platform platform in platforms)
            {
                if (platform == platforms[0])
                    continue;

                if (possiblePlatforms.Contains(platform))
                    continue;

                possiblePlatforms.Add(platform);
            }
        }

        if (possiblePlatforms.Count < connectedNodes.Count)
        {
            Debug.LogError("문을 생성할 플랫폼이 부족합니다.");
            return;
        }

        foreach (MapTree.Node targetNode in connectedNodes)
        {
            int randomIndex = Random.Range(
                0,
                possiblePlatforms.Count
            );

            Platform doorPlatform =
                possiblePlatforms[randomIndex];

            possiblePlatforms.RemoveAt(randomIndex);

            doorPlatform.type = Platform.PlatformType.Door;
            doorPlatform.width = DoorPlatformWidth;

            // 연결 대상 저장
            doorPlatform.targetNode = targetNode;
        }
    }

    private void MakePlatformElements()
    {
        foreach (Platform platform in platforms)
        {
            // 문 플랫폼에는 요소를 넣지 않음
            if (platform.type ==
                Platform.PlatformType.Door)
            {
                continue;
            }

            int random = Random.Range(0, 100);

            if (random < 45)
            {
                platform.elements.Add(
                    Platform.PlatformElement.Enemy
                );
            }
            else if (random < 65)
            {
                platform.elements.Add(
                    Platform.PlatformElement.Item
                );
            }
        }
    }


    private void DrawPlatforms()
    {
        foreach (Platform platform in platforms)
        {
            int startX =
                platform.position.x -
                platform.width / 2;

            int endX =
                startX + platform.width;

            for (int x = startX; x < endX; x++)
            {
                tilemap.SetTile(
                    new Vector3Int(
                        x,
                        platform.position.y,
                        0
                    ),
                    tileset.ground
                );
            }
        }
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public Vector3 GenerateRoom(MapTree.Node node)
    {
        if (node == null)
            return Vector3.zero;

        if (mapManager == null || mapTree == null)
        {
            Debug.LogError("MapManager 또는 MapTree가 연결되지 않았습니다.");
            return Vector3.zero;
        }

        currentNode = node;

        // 이전 방 제거
        ClearPreviousRoom();

        // 방의 테두리 생성
        DrawRoomBoundary();

        // 해당 방의 저장 데이터 가져오기
        RoomData data = mapManager.GetRoomData(currentNode);

        if (data.isGenerated)
        {
            LoadRoomData(data);
            Debug.Log("기존 방 불러오기: " + node.type);
        }
        else
        {
            GeneratePlatformData();
            SaveRoomData(data);

            Debug.Log("새로운 방 생성: " + node.type);
        }

        // 플랫폼 그리기
        DrawPlatforms();

        // 문 생성
        CreateDoorsFromPlatforms();

        // 플레이어 시작 위치 반환
        if (platforms.Count == 0)
        {
            Debug.LogError("플랫폼이 생성되지 않았습니다.");
            return Vector3.zero;
        }

        return new Vector3(
            platforms[0].position.x,
            platforms[0].position.y + 2,
            0
        );
    }

    private void ClearPreviousRoom()
    {
        tilemap.ClearAllTiles();

        foreach (GameObject door in spawnedDoors)
        {
            if (door != null)
                Destroy(door);
        }

        spawnedDoors.Clear();

        platforms.Clear();
        platformPositions.Clear();
    }

    private void BuildRoom(MapTree.Node node)
    {
        // 방의 테두리 생성
        for (int i = 0; i < Mapsize.x; i++)
        {
            for (int j = 0; j < Mapsize.y; j++)
            {
                if (i == 0 ||
                    i == Mapsize.x - 1 ||
                    j == 0 ||
                    j == Mapsize.y - 2)
                {
                    tilemap.SetTile(
                        new Vector3Int(i, j, 0),
                        tileset.ground
                    );
                }
            }
        }

        // 플랫폼 생성
        GeneratePlatformData();

       
    }

    //테두리 생성
    private void DrawRoomBoundary()
    {
        for (int i = 0; i < Mapsize.x; i++)
        {
            for (int j = 0; j < Mapsize.y; j++)
            {
                if (i == 0 ||
                    i == Mapsize.x - 1 ||
                    j == 0 ||
                    j == Mapsize.y - 2)
                {
                    tilemap.SetTile(
                        new Vector3Int(i, j, 0),
                        tileset.ground
                    );
                }
            }
        }
    }

    //저장
    private void SaveRoomData(RoomData data)
    {
        data.platforms.Clear();

        foreach (Platform platform in platforms)
        {
            Platform copy = new Platform(
                platform.position,
                platform.width,
                platform.type
            );

            // 플랫폼에 배치된 요소 복사
            copy.elements = new List<PlatformElement>(
                platform.elements
            );

            // 문 목적지 복사
            copy.targetNode = platform.targetNode;

            data.platforms.Add(copy);
        }

        data.isGenerated = true;
    }

    //불러오기
    private void LoadRoomData(RoomData data)
    {
        platforms.Clear();

        foreach (Platform savedPlatform in data.platforms)
        {
            Platform copy = new Platform(
                savedPlatform.position,
                savedPlatform.width,
                savedPlatform.type
            );

            copy.elements = new List<PlatformElement>(
                savedPlatform.elements
            );

            copy.targetNode = savedPlatform.targetNode;

            platforms.Add(copy);
        }
    }


    private void CreateDoorsFromPlatforms()
    {
        RoomData roomData =
            mapManager.GetRoomData(currentNode);

        foreach (Platform platform in platforms)
        {
            if (platform.type != Platform.PlatformType.Door)
                continue;

            if (platform.targetNode == null)
                continue;

            Vector3 doorPosition = new Vector3(
                platform.position.x,
                platform.position.y + 2,
                0
            );

            GameObject doorObject = Instantiate(
                doorPrefab,
                doorPosition,
                Quaternion.identity
            );

            Door door = doorObject.GetComponent<Door>();

            if (door != null)
            {
                door.Initialize(
                    platform.targetNode,
                    this,
                    mapManager
                );

                // 현재 방의 클리어 여부에 따라 설정
                if (roomData.isCleared)
                    door.OpenDoor();
                else
                    door.CloseDoor();

                spawnedDoors.Add(doorObject);
            }
        }
    }
    public void SetAllDoorsOpen(bool isOpen)
    {
        foreach (GameObject doorObject in spawnedDoors)
        {
            if (doorObject == null)
                continue;

            Door door = doorObject.GetComponent<Door>();

            if (door == null)
                continue;

            if (isOpen)
                door.OpenDoor();
            else
                door.CloseDoor();
        }
    }
}



