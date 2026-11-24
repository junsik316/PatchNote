using System.Collections.Generic;
using UnityEngine;


//맵 그래프 구조를 생성하는 클래스
public class MapTree : MonoBehaviour
{
    public enum NodeType
    {
        common,
        hard,
        elite,
        reward,
        boss
    }

    [Header("Map")]
    public int MapDepth = 8;

    [Header("Branch")]
    public int MinBranchLength = 1;
    public int MaxBranchLength = 6;
    public int BranchCount = 5;

    [Header("Extra Connection")]
    public int ExtraConnectionCount = 5;

    public class Node
    {


        public NodeType type;

        // 이제 트리가 아니라 그래프이므로
        // children보다는 connections라는 이름이 더 적절함
        public List<Node> connections = new List<Node>();

        // 메인 루트에 포함되어 있는지
        public bool isMainRoute;

        // 이 노드가 속한 곁가지의 특성
        public int branchLength;
        public int mainRouteLength;

        //미니맵 관련 정보
        public Vector2 mapPosition;

        public bool isVisited = false;
    }

    // 모든 방
    List<Node> rooms = new List<Node>();

    // Start → Boss 메인 루트
    List<Node> mainRoute = new List<Node>();

    // 생성된 곁가지
    List<List<Node>> branches = new List<List<Node>>();


    // =========================================================
    // 연결
    // =========================================================

    private void Connect(Node a, Node b)
    {
        if (a == b)
            return;

        // 이미 연결되어 있으면 중복 연결하지 않음
        if (a.connections.Contains(b))
            return;

        a.connections.Add(b);
        b.connections.Add(a);
    }


    // =========================================================
    // 메인 루트 생성
    // Start → 방 → 방 → ... → Boss
    // =========================================================

    private void MakeBoss(Node node, int mapDepth)
    {
        if (mapDepth == 1)
        {
            Node bossRoom = new Node();

            bossRoom.type = NodeType.boss;
            bossRoom.isMainRoute = true;

            Connect(node, bossRoom);

            rooms.Add(bossRoom);
            mainRoute.Add(bossRoom);

            return;
        }

        Node newNode = new Node();

        newNode.type = NodeType.common;
        newNode.isMainRoute = true;

        Connect(node, newNode);

        rooms.Add(newNode);
        mainRoute.Add(newNode);

        MakeBoss(newNode, mapDepth - 1);
    }


    // =========================================================
    // 곁가지 생성
    //
    // startIndex → branch → endIndex
    // =========================================================

    private void MakeBranch(int startIndex, int endIndex)
    {
        Node startNode = mainRoute[startIndex];
        Node endNode = mainRoute[endIndex];

        // 곁가지 길이는 메인 루트와 독립적으로 결정
        int branchLength = Random.Range(
            MinBranchLength,
            MaxBranchLength + 1
        );

        int mainRouteLength = endIndex - startIndex;

        List<Node> branch = new List<Node>();

        Node currentNode = startNode;

        // -----------------------------------------------------
        // 곁가지 방 생성
        // -----------------------------------------------------

        for (int i = 0; i < branchLength; i++)
        {
            Node newNode = new Node();

            newNode.branchLength = branchLength;
            newNode.mainRouteLength = mainRouteLength;

            Connect(currentNode, newNode);

            rooms.Add(newNode);
            branch.Add(newNode);

            currentNode = newNode;
        }

        // 마지막 곁가지 방 → 메인 루트 도착점
        Connect(currentNode, endNode);

        branches.Add(branch);

        // -----------------------------------------------------
        // 곁가지 길이에 따라 타입 결정
        // -----------------------------------------------------

        SetBranchTypes(branch, branchLength, mainRouteLength);
    }


    // =========================================================
    // 곁가지 난이도 / 보상 배정
    //
    // branchLength < mainRouteLength
    //     → 짧은 지름길 → Hard / Elite 증가
    //
    // branchLength == mainRouteLength
    //     → 균형
    //
    // branchLength > mainRouteLength
    //     → 긴 길 → Reward 증가
    // =========================================================

    private void SetBranchTypes(
        List<Node> branch,
        int branchLength,
        int mainRouteLength)
    {
        int difference = branchLength - mainRouteLength;

        foreach (Node node in branch)
        {
            int random;

            // -------------------------------------------------
            // 메인 루트보다 짧은 경우
            // 빠르지만 위험한 길
            // -------------------------------------------------

            if (difference < 0)
            {
                random = Random.Range(0, 100);

                if (random < 40)
                {
                    node.type = NodeType.hard;
                }
                else if (random < 65)
                {
                    node.type = NodeType.elite;
                }
                else if (random < 80)
                {
                    node.type = NodeType.reward;
                }
                else
                {
                    node.type = NodeType.common;
                }
            }

            // -------------------------------------------------
            // 같은 길이
            // -------------------------------------------------

            else if (difference == 0)
            {
                random = Random.Range(0, 100);

                if (random < 45)
                {
                    node.type = NodeType.common;
                }
                else if (random < 70)
                {
                    node.type = NodeType.hard;
                }
                else if (random < 90)
                {
                    node.type = NodeType.reward;
                }
                else
                {
                    node.type = NodeType.elite;
                }
            }

            // -------------------------------------------------
            // 메인 루트보다 긴 경우
            // 오래 걸리지만 보상이 좋은 길
            // -------------------------------------------------

            else
            {
                random = Random.Range(0, 100);

                if (random < 40)
                {
                    node.type = NodeType.reward;
                }
                else if (random < 75)
                {
                    node.type = NodeType.common;
                }
                else if (random < 90)
                {
                    node.type = NodeType.hard;
                }
                else
                {
                    node.type = NodeType.elite;
                }
            }
        }
    }


    // =========================================================
    // 두 노드를 연결할 가중치
    // =========================================================

    private int GetConnectionWeight(Node a, Node b)
    {
        // 자기 자신
        if (a == b)
            return 0;

        // 이미 연결됨
        if (a.connections.Contains(b))
            return 0;

        // ---------------------------------------------
        // Reward ↔ Reward
        // ---------------------------------------------

        if (a.type == NodeType.reward &&
            b.type == NodeType.reward)
        {
            return 0;
        }

        // ---------------------------------------------
        // Boss와의 추가 연결
        // ---------------------------------------------

        if (a.type == NodeType.boss ||
            b.type == NodeType.boss)
        {
            return 1;
        }

        // ---------------------------------------------
        // Common
        // ---------------------------------------------

        if (a.type == NodeType.common &&
            b.type == NodeType.common)
        {
            return 5;
        }

        if (a.type == NodeType.common &&
            b.type == NodeType.reward)
        {
            return 4;
        }

        if (a.type == NodeType.reward &&
            b.type == NodeType.common)
        {
            return 4;
        }

        // ---------------------------------------------
        // Hard
        // ---------------------------------------------

        if (a.type == NodeType.hard &&
            b.type == NodeType.reward)
        {
            return 5;
        }

        if (a.type == NodeType.reward &&
            b.type == NodeType.hard)
        {
            return 5;
        }

        if (a.type == NodeType.hard &&
            b.type == NodeType.common)
        {
            return 4;
        }

        if (a.type == NodeType.common &&
            b.type == NodeType.hard)
        {
            return 4;
        }

        // ---------------------------------------------
        // Elite
        // ---------------------------------------------

        if (a.type == NodeType.elite &&
            b.type == NodeType.reward)
        {
            return 6;
        }

        if (a.type == NodeType.reward &&
            b.type == NodeType.elite)
        {
            return 6;
        }

        if (a.type == NodeType.elite &&
            b.type == NodeType.common)
        {
            return 3;
        }

        if (a.type == NodeType.common &&
            b.type == NodeType.elite)
        {
            return 3;
        }

        // Elite ↔ Elite
        if (a.type == NodeType.elite &&
            b.type == NodeType.elite)
        {
            return 1;
        }

        return 1;
    }


    // =========================================================
    // 가중치에 따라 랜덤 노드 선택
    // =========================================================

    private Node GetWeightedRandomNode(Node source)
    {
        List<Node> candidates = new List<Node>();
        List<int> weights = new List<int>();

        int totalWeight = 0;

        foreach (Node node in rooms)
        {
            int weight = GetConnectionWeight(source, node);

            if (weight <= 0)
                continue;

            candidates.Add(node);
            weights.Add(weight);

            totalWeight += weight;
        }

        if (candidates.Count == 0)
            return null;

        int randomValue = Random.Range(0, totalWeight);

        for (int i = 0; i < candidates.Count; i++)
        {
            randomValue -= weights[i];

            if (randomValue < 0)
                return candidates[i];
        }

        return null;
    }


    // =========================================================
    // 추가 간선 생성
    //
    // 랜덤 노드 하나를 고르고
    // 가중치에 따라 다른 노드와 연결
    // =========================================================

    private void MakeExtraConnections()
    {
        for (int i = 0; i < ExtraConnectionCount; i++)
        {
            if (rooms.Count < 2)
                return;

            Node startNode =
                rooms[Random.Range(0, rooms.Count)];

            Node endNode =
                GetWeightedRandomNode(startNode);

            if (endNode == null)
                continue;

            Connect(startNode, endNode);
        }
    }

    public Node GetStartNode()
    {
        return rooms[0];
    }

    // =========================================================
    // Start
    // =========================================================

    public void GenerateMap()
    {
        // -----------------------------------------------------
        // 1. 시작 방 생성
        // -----------------------------------------------------

        Node root = new Node();

        root.type = NodeType.common;
        root.isMainRoute = true;

        rooms.Add(root);
        mainRoute.Add(root);


        // -----------------------------------------------------
        // 2. Start → Boss 메인 루트 생성
        // -----------------------------------------------------

        MakeBoss(root, MapDepth);


        // -----------------------------------------------------
        // 3. 곁가지 생성
        // -----------------------------------------------------

        for (int i = 0; i < BranchCount; i++)
        {
            // Start와 Boss는 제외
            int startIndex = Random.Range(
                1,
                mainRoute.Count - 2
            );

            // 반드시 start보다 뒤에 있는 노드를 선택
            int endIndex = Random.Range(
                startIndex + 1,
                mainRoute.Count - 1
            );

            MakeBranch(startIndex, endIndex);
        }


        // -----------------------------------------------------
        // 4. 곁가지끼리 / 전체 노드 추가 연결
        // -----------------------------------------------------

        MakeExtraConnections();
    }


    //연결 정보 반환
    public List<Node> GetRooms()
    {
        return rooms;
    }
}