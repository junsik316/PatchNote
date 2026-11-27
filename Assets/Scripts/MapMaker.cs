using System.Collections.Generic;
using UnityEngine;

// 노드의 지도 좌표 계산 및 지도 UI 생성 담당
public class MapMaker : MonoBehaviour
{
    [Header("UI 설정")]
    public GameObject nodePrefab;
    public RectTransform nodeContainer;

    [Header("원형 맵 설정")]
    public float circleRadius = 450f;
    public float loopSpacing = 900f;

    [Header("가지 설정")]
    public float branchSpacing = 150f;
    public float branchSpreadAngle = 30f;

    [Header("지도 설정")]
    public float mapPadding = 100f;

    [Header("간선 설정")]
    public GameObject linePrefab;
    public float lineThickness = 5f;

    [Header("노드 간격 설정")]
    [Min(1f)] public float minNodeDistance = 120f;
    [Min(1)] public int spacingIterations = 40;

    private RectTransform lineContainer;

    private Dictionary<MapTree.Node, RectTransform> nodeUIDictionary
        = new Dictionary<MapTree.Node, RectTransform>();

    private Dictionary<MapTree.Node, Vector2> positions
        = new Dictionary<MapTree.Node, Vector2>();

    // DFS의 역방향 간선 하나마다 하나의 기본 루프를 저장한다.
    private List<List<MapTree.Node>> loops = new List<List<MapTree.Node>>();

    private Dictionary<MapTree.Node, MapTree.Node> parents
        = new Dictionary<MapTree.Node, MapTree.Node>();

    private Dictionary<MapTree.Node, int> depths
        = new Dictionary<MapTree.Node, int>();

    private HashSet<MapTree.Node> visited
        = new HashSet<MapTree.Node>();

    private HashSet<MapTree.Node> loopNodes
        = new HashSet<MapTree.Node>();

    private int loopCount;
    private MapTree.Node excludedStartNode;

    // =========================================================
    // 전체 맵 UI 생성
    // =========================================================

    public void GenerateMapUI(List<MapTree.Node> nodes, MapTree.Node startNode)
    {
        if (nodes == null || startNode == null)
        {
            Debug.LogError("맵 노드 또는 시작 노드가 없습니다.");
            return;
        }

        if (nodePrefab == null || nodeContainer == null)
        {
            Debug.LogError("노드 프리팹 또는 노드 컨테이너가 없습니다.");
            return;
        }

        if (!nodes.Contains(startNode))
        {
            Debug.LogError("시작 노드가 노드 목록에 포함되어 있지 않습니다.");
            return;
        }

        for (int i = nodeContainer.childCount - 1; i >= 0; i--)
            Destroy(nodeContainer.GetChild(i).gameObject);

        nodeUIDictionary.Clear();
        lineContainer = null;

        CalculateMapPositions(nodes, startNode);
        SetMapSize(nodes);

        foreach (MapTree.Node node in nodes)
        {
            GameObject obj = Instantiate(nodePrefab, nodeContainer);

            RectTransform rect = obj.GetComponent<RectTransform>();
            if (rect == null)
            {
                Debug.LogError("노드 프리팹에 RectTransform이 없습니다.");
                Destroy(obj);
                continue;
            }

            rect.anchoredPosition = node.mapPosition;

            NodeUI nodeUI = obj.GetComponent<NodeUI>();
            if (nodeUI != null)
            {
                nodeUI.SetNode(node);
                nodeUIDictionary[node] = rect;
            }
            else
            {
                Debug.LogError("노드 프리팹에 NodeUI 컴포넌트가 없습니다.");
            }
        }

        GenerateLines(nodes);
        nodeContainer.localScale = Vector3.one;
    }

    // =========================================================
    // 전체 좌표 계산
    // =========================================================

    private void CalculateMapPositions(
        List<MapTree.Node> nodes,
        MapTree.Node startNode)
    {
        positions.Clear();
        loops.Clear();
        parents.Clear();
        depths.Clear();
        visited.Clear();
        loopNodes.Clear();

        // 시작 노드는 루프 바깥에 고정한다.
        excludedStartNode = startNode;
        positions[startNode] = Vector2.zero;
        parents[startNode] = null;
        depths[startNode] = 0;

        // 시작 노드 자체를 루프에 포함하지 않고 탐색한다.
        visited.Add(startNode);

        foreach (MapTree.Node next in startNode.connections)
        {
            if (next == null || visited.Contains(next))
                continue;

            parents[next] = startNode;
            depths[next] = 1;
            FindLoops(next, startNode);
        }

        // 시작 노드와 연결되지 않은 그래프가 있다면 별도로 탐색한다.
        foreach (MapTree.Node node in nodes)
        {
            if (visited.Contains(node))
                continue;

            parents[node] = null;
            depths[node] = 0;
            FindLoops(node, null);
        }

        // 각 루프를 개별 원으로 배치한다.
        PlaceAllLoops(startNode);

        // 루프에 속하지 않는 노드는 원 바깥에 가지 형태로 배치한다.
        PlaceBranches(nodes, startNode);

        PlaceBranches(nodes, startNode);

        // 노드 간 최소 거리 보정
        ResolveNodeOverlaps(nodes, startNode);

        foreach (MapTree.Node node in nodes)
        {
            if (positions.TryGetValue(node, out Vector2 position))
                node.mapPosition = position;
        }
    }

    // =========================================================
    // 루프 탐색
    // DFS 기반 기본 루프 추출
    // =========================================================

    private void FindLoops(MapTree.Node node, MapTree.Node parent)
    {
        visited.Add(node);

        foreach (MapTree.Node next in node.connections)
        {
            if (next == null || next == parent || next == excludedStartNode)
                continue;

            if (!visited.Contains(next))
            {
                parents[next] = node;
                depths[next] = depths[node] + 1;
                FindLoops(next, node);
            }
            else if (depths.ContainsKey(next) &&
                     depths[next] < depths[node])
            {
                // 현재 노드에서 조상 노드까지의 경로가 하나의 루프가 된다.
                List<MapTree.Node> loop = BuildLoop(node, next);

                if (loop.Count >= 3)
                {
                    loops.Add(loop);

                    foreach (MapTree.Node loopNode in loop)
                        loopNodes.Add(loopNode);
                }
            }
        }
    }

    private List<MapTree.Node> BuildLoop(
        MapTree.Node node,
        MapTree.Node ancestor)
    {
        List<MapTree.Node> loop = new List<MapTree.Node>();

        MapTree.Node current = node;
        loop.Add(current);

        while (current != null && current != ancestor)
        {
            if (!parents.TryGetValue(current, out current))
                break;

            if (current != null)
                loop.Add(current);
        }

        return loop;
    }

    // =========================================================
    // 루프별 원형 배치
    // =========================================================

    private void PlaceAllLoops(MapTree.Node startNode)
    {
        loopCount = 0;

        foreach (List<MapTree.Node> loop in loops)
        {
            if (loop.Count < 3)
                continue;

            // 이미 배치된 노드가 있다면 그 노드를 루프의 접점으로 사용한다.
            MapTree.Node anchor = null;

            foreach (MapTree.Node node in loop)
            {
                if (positions.ContainsKey(node))
                {
                    anchor = node;
                    break;
                }
            }

            if (anchor == null)
            {
                // 시작 노드와 떨어진 그래프의 루프는 별도 영역에 배치한다.
                anchor = loop[0];

                Vector2 offset = new Vector2(
                    loopSpacing * (loopCount + 1),
                    0f
                );

                positions[anchor] = offset;
            }

            Vector2 center;
            Vector2 direction;

            if (anchor == startNode)
            {
                // 시작 노드는 원 바깥에 두고 루프는 오른쪽에 배치한다.
                direction = Vector2.right;
            }
            else
            {
                // 루프가 공유하는 접점이 있으면 그 지점에서 바깥 방향으로 확장한다.
                direction = GetOutwardDirection(anchor, loop);
            }

            float radius = Mathf.Max(
                circleRadius,
                loop.Count * branchSpacing / (2f * Mathf.PI)
            );

            center = positions[anchor] + direction * radius;

            float anchorAngle = Mathf.Atan2(
                -direction.y,
                -direction.x
            );

            // 실제 접점(anchor)을 첫 번째로 옮겨 원의 시작점에 맞춘다.
            List<MapTree.Node> orderedLoop = new List<MapTree.Node>();
            orderedLoop.Add(anchor);

            foreach (MapTree.Node node in loop)
            {
                if (node != anchor)
                    orderedLoop.Add(node);
            }

            // 이 루프에 속한 노드만 원 둘레에 배치한다.
            for (int i = 1; i < orderedLoop.Count; i++)
            {
                MapTree.Node node = orderedLoop[i];

                // 다른 루프와 공유하는 노드는 한 번만 배치한다.
                if (positions.ContainsKey(node))
                    continue;

                float angle = anchorAngle +
                    2f * Mathf.PI * i / orderedLoop.Count;

                positions[node] = center + new Vector2(
                    Mathf.Cos(angle) * radius,
                    Mathf.Sin(angle) * radius
                );
            }

            loopCount++;
        }
    }

    private Vector2 GetOutwardDirection(
        MapTree.Node anchor,
        List<MapTree.Node> loop)
    {
        Vector2 average = Vector2.zero;
        int count = 0;

        foreach (MapTree.Node node in loop)
        {
            if (node == anchor || !positions.ContainsKey(node))
                continue;

            average += positions[node] - positions[anchor];
            count++;
        }

        if (count > 0 && average.sqrMagnitude > 0.001f)
            return (-average).normalized;

        return Vector2.right;
    }

    // =========================================================
    // 루프 바깥의 일반 노드 배치
    // =========================================================

    private void PlaceBranches(
        List<MapTree.Node> nodes,
        MapTree.Node startNode)
    {
        Queue<MapTree.Node> queue = new Queue<MapTree.Node>();
        HashSet<MapTree.Node> branchVisited = new HashSet<MapTree.Node>();

        queue.Enqueue(startNode);
        branchVisited.Add(startNode);

        // 이미 원에 배치된 루프 노드도 가지 탐색의 출발점으로 사용한다.
        foreach (MapTree.Node node in nodes)
        {
            if (loopNodes.Contains(node) && positions.ContainsKey(node))
            {
                queue.Enqueue(node);
                branchVisited.Add(node);
            }
        }

        while (queue.Count > 0)
        {
            MapTree.Node parent = queue.Dequeue();

            foreach (MapTree.Node next in parent.connections)
            {
                if (next == null || branchVisited.Contains(next))
                    continue;

                branchVisited.Add(next);

                // 루프 노드는 원형 배치 좌표를 유지한다.
                if (loopNodes.Contains(next))
                {
                    queue.Enqueue(next);
                    continue;
                }

                Vector2 direction = GetBranchDirection(parent, next);
                positions[next] = positions[parent] +
                    direction * branchSpacing;

                queue.Enqueue(next);
            }
        }

        // 그래프와 연결되지 않은 노드도 빠뜨리지 않는다.
        int isolatedIndex = 0;

        foreach (MapTree.Node node in nodes)
        {
            if (positions.ContainsKey(node))
                continue;

            positions[node] = new Vector2(
                loopSpacing * (loopCount + 1),
                isolatedIndex * branchSpacing
            );

            isolatedIndex++;
        }
    }

    private Vector2 GetBranchDirection(
        MapTree.Node parent,
        MapTree.Node child)
    {
        Vector2 direction = Vector2.right;

        if (loopNodes.Contains(parent))
        {
            // 루프에서 바깥쪽으로 가지가 뻗도록 한다.
            Vector2 center = Vector2.zero;
            int count = 0;

            foreach (MapTree.Node node in parent.connections)
            {
                if (positions.ContainsKey(node))
                {
                    center += positions[node];
                    count++;
                }
            }

            if (count > 0)
            {
                center /= count;
                direction = (positions[parent] - center).normalized;
            }
        }
        else if (parents.ContainsKey(parent) &&
                 parents[parent] != null &&
                 positions.ContainsKey(parents[parent]))
        {
            direction = (
                positions[parent] - positions[parents[parent]]
            ).normalized;
        }

        if (direction.sqrMagnitude < 0.001f)
            direction = Vector2.right;

        return RotateVector(direction, branchSpreadAngle);
    }

    // =========================================================
    // 벡터 회전
    // =========================================================

    private Vector2 RotateVector(Vector2 vector, float angle)
    {
        float radian = angle * Mathf.Deg2Rad;

        float x =
            vector.x * Mathf.Cos(radian) -
            vector.y * Mathf.Sin(radian);

        float y =
            vector.x * Mathf.Sin(radian) +
            vector.y * Mathf.Cos(radian);

        return new Vector2(x, y).normalized;
    }

    // =========================================================
    // 맵 크기 계산
    // =========================================================

    private void SetMapSize(List<MapTree.Node> nodes)
    {
        if (nodes.Count == 0)
            return;

        float minX = float.MaxValue;
        float maxX = float.MinValue;
        float minY = float.MaxValue;
        float maxY = float.MinValue;

        foreach (MapTree.Node node in nodes)
        {
            minX = Mathf.Min(minX, node.mapPosition.x);
            maxX = Mathf.Max(maxX, node.mapPosition.x);
            minY = Mathf.Min(minY, node.mapPosition.y);
            maxY = Mathf.Max(maxY, node.mapPosition.y);
        }

        nodeContainer.sizeDelta = new Vector2(
            maxX - minX + mapPadding * 2f,
            maxY - minY + mapPadding * 2f
        );
    }

    // =========================================================
    // 노드 UI 반환
    // =========================================================

    public RectTransform GetNodeUI(MapTree.Node node)
    {
        if (nodeUIDictionary.TryGetValue(node, out RectTransform rect))
            return rect;

        return null;
    }

    // =========================================================
    // 간선 생성
    // =========================================================

    private void GenerateLines(List<MapTree.Node> nodes)
    {
        if (linePrefab == null)
        {
            Debug.LogError("간선 프리팹이 지정되지 않았습니다.");
            return;
        }

        if (lineContainer != null)
            Destroy(lineContainer.gameObject);

        GameObject container = new GameObject(
            "LineContainer",
            typeof(RectTransform)
        );

        lineContainer = container.GetComponent<RectTransform>();
        lineContainer.SetParent(nodeContainer, false);
        lineContainer.anchorMin = Vector2.zero;
        lineContainer.anchorMax = Vector2.one;
        lineContainer.pivot = new Vector2(0.5f, 0.5f);
        lineContainer.offsetMin = Vector2.zero;
        lineContainer.offsetMax = Vector2.zero;

        for (int i = 0; i < nodes.Count; i++)
        {
            MapTree.Node node = nodes[i];

            foreach (MapTree.Node connected in node.connections)
            {
                int connectedIndex = nodes.IndexOf(connected);

                if (connectedIndex < 0 || i >= connectedIndex)
                    continue;

                RectTransform startUI = GetNodeUI(node);
                RectTransform endUI = GetNodeUI(connected);

                if (startUI == null || endUI == null)
                    continue;

                GameObject obj = Instantiate(linePrefab, lineContainer);

                RectTransform lineRect = obj.GetComponent<RectTransform>();
                if (lineRect != null)
                {
                    lineRect.anchorMin = new Vector2(0.5f, 0.5f);
                    lineRect.anchorMax = new Vector2(0.5f, 0.5f);
                    lineRect.pivot = new Vector2(0.5f, 0.5f);
                }

                MapLine line = obj.GetComponent<MapLine>();
                if (line == null)
                {
                    Debug.LogError("간선 프리팹에 MapLine 컴포넌트가 없습니다.");
                    Destroy(obj);
                    continue;
                }

                Vector2 startPosition =
                    lineContainer.InverseTransformPoint(startUI.position);

                Vector2 endPosition =
                    lineContainer.InverseTransformPoint(endUI.position);

                line.SetLine(startPosition, endPosition, lineThickness);
            }
        }

        lineContainer.SetAsFirstSibling();
    }

    private void ResolveNodeOverlaps(
    List<MapTree.Node> nodes,
    MapTree.Node fixedNode)
    {
        float minDistance = Mathf.Max(1f, minNodeDistance);

        for (int iteration = 0; iteration < spacingIterations; iteration++)
        {
            bool moved = false;

            for (int i = 0; i < nodes.Count; i++)
            {
                MapTree.Node a = nodes[i];

                for (int j = i + 1; j < nodes.Count; j++)
                {
                    MapTree.Node b = nodes[j];

                    Vector2 difference = positions[b] - positions[a];
                    float distance = difference.magnitude;

                    if (distance >= minDistance)
                        continue;

                    Vector2 direction;

                    if (distance < 0.001f)
                        direction = Vector2.right;
                    else
                        direction = difference / distance;

                    float push = (minDistance - distance) * 0.5f;

                    if (a == fixedNode)
                        positions[b] += direction * push * 2f;
                    else if (b == fixedNode)
                        positions[a] -= direction * push * 2f;
                    else
                    {
                        positions[a] -= direction * push;
                        positions[b] += direction * push;
                    }

                    moved = true;
                }
            }

            if (!moved)
                break;
        }
    }
}
