
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    private List<MapTree.Node> mapNodes =
        new List<MapTree.Node>();

    public MapTree.Node currentNode;

    [Header("맵 관리")]
    public MapTree mapTree;
    public RoomMaker roomMaker;
    public MapMaker mapMaker;
    public MiniMap miniMap;
    public CardManager cardManager;

    [Header("적 관리")]
    public RoomEnemySpawner enemySpawner;

    // 방별 데이터 저장
    private Dictionary<MapTree.Node, RoomData> roomDataDictionary =
        new Dictionary<MapTree.Node, RoomData>();

    [Header("스테이지 상태")]
    public bool isStageCleared = false;
    public bool isStageChecking = false;

    [Header("플레이어 관리")]
    public GameObject playerPrefab;

    private void Awake()
    {
        if (enemySpawner != null)
        {
            enemySpawner.AllEnemiesDefeated +=
                OnAllEnemiesDefeated;
        }
    }

    private void Start()
    {
        InitializeMap();
    }

    private void OnDestroy()
    {
        if (enemySpawner != null)
        {
            enemySpawner.AllEnemiesDefeated -=
                OnAllEnemiesDefeated;
        }
    }

    private void InitializeMap()
    {
        mapTree.GenerateMap();

        mapNodes = mapTree.GetRooms();
        currentNode = mapTree.GetStartNode();

        // 시작 방은 적 없이 이동할 수 있도록
        // 최초부터 클리어 상태로 처리
        PrepareRoomState();

        Vector3 startPos =
            roomMaker.GenerateRoom(currentNode);

        GameObject playerObject = Instantiate(
    playerPrefab,
    startPos,
    Quaternion.identity
);

        Playercontroller spawnedPlayer =
            playerObject.GetComponent<Playercontroller>();

        GameManager.Instance.RegisterPlayer(spawnedPlayer);

        if (GameManager.Instance.Player != null)
        {
            GameManager.Instance.Player.cardManager = cardManager;
        }
    }

    // 방에 들어갈 때 클리어 기록 복원
    private void PrepareRoomState()
    {
        RoomData data = GetRoomData(currentNode);

        // 시작 방은 보상 없이 문을 열어 둠
        if (currentNode == mapTree.GetStartNode())
            data.isCleared = true;

        isStageCleared = data.isCleared;
        isStageChecking = false;
    }

    public List<MapTree.Node> GetConnectedRooms()
    {
        return currentNode.connections;
    }

    public void SetCurrentRoom(MapTree.Node node)
    {
        if (node == null)
            return;

        // 이전 방의 적 제거
        if (enemySpawner != null)
            enemySpawner.ClearEnemies();

        currentNode = node;

        // 해당 방의 클리어 여부 복원
        PrepareRoomState();

        // 기존 방 구조 생성 또는 불러오기
        Vector3 spawnPos =
            roomMaker.GenerateRoom(currentNode);

        Playercontroller player = GameManager.Instance.Player;

        if (player != null)
        {
            player.transform.position = spawnPos;
        }

        // 미클리어 방에서만 적 생성
        SpawnCurrentRoomEnemies();

        if (miniMap != null)
        {
            // miniMap.UpdatePlayerNodeUI();
        }
    }

    private void SpawnCurrentRoomEnemies()
    {
        // 이전에 클리어한 방은 적 생성 X
        if (isStageCleared)
            return;

        Playercontroller player = GameManager.Instance.Player;

        if (enemySpawner == null || player == null)
        {
            Debug.LogWarning("적 생성에 필요한 참조가 없습니다.");
            return;
        }

        enemySpawner.SpawnEnemies(
            currentNode,
            roomMaker.platforms,
            roomMaker.tilemap,
            player.transform
        );

        int enemyCount =
            enemySpawner.GetAliveEnemyCount();

        if (enemyCount > 0)
        {
            isStageChecking = true;
        }
        else
        {
            // 적이 없는 방은 바로 클리어
            ClearStage();
        }
    }

    public RoomData GetRoomData(MapTree.Node node)
    {
        if (!roomDataDictionary.ContainsKey(node))
        {
            RoomData data = new RoomData();
            data.node = node;

            roomDataDictionary.Add(node, data);
        }

        return roomDataDictionary[node];
    }

    // 마지막 적 사망 이벤트
    private void OnAllEnemiesDefeated()
    {
        if (!isStageChecking || isStageCleared)
            return;

        ClearStage();
    }

    private void ClearStage()
    {
        if (currentNode == null || isStageCleared)
            return;

        RoomData data = GetRoomData(currentNode);

        // 이미 클리어된 방이면 중복 실행 방지
        if (data.isCleared)
            return;

        // 클리어 상태 저장
        data.isCleared = true;

        isStageCleared = true;
        isStageChecking = false;

        Debug.Log(
            $"스테이지 클리어: {currentNode.type}"
        );

        // 문 개방
        OpenDoors();

        // 카드 선택 화면 표시 (최초 1회)
        if (cardManager != null)
            cardManager.ShowCardSelection(currentNode);
    }

    private void OpenDoors()
    {
        roomMaker.SetAllDoorsOpen(true);
    }

    private void Update()
    {
        // 테스트: C키를 누르면 현재 방의 적 전부 처치
        if (Input.GetKeyDown(KeyCode.C) &&
            isStageChecking &&
            !isStageCleared)
        {
            if (enemySpawner != null)
                enemySpawner.KillAllEnemies();
        }
    }
}
