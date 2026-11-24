
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum EnemyRoomType
{
    Common,
    Hard,
    Elite,
    Reward,
    Boss
}

[Serializable]
public class EnemySpawnEntry
{
    public EnemyRoomType roomType;
    public EnemyController enemyPrefab;

    [Min(0)]
    public int count = 1;
}

public class RoomEnemySpawner : MonoBehaviour
{
    [Header("노드별 적 생성 설정")]
    public List<EnemySpawnEntry> spawnRules =
        new List<EnemySpawnEntry>();

    [Header("생성 위치")]
    public float spawnYOffset = 1f;

    [Min(0)]
    public int edgePadding = 1;

    [Header("생성된 적의 부모")]
    public Transform enemyContainer;

    private List<EnemyController> spawnedEnemies =
        new List<EnemyController>();
    // 현재 방의 모든 적이 죽으면 발생
    public event Action AllEnemiesDefeated;

    // 현재 방에 적 생성
    public void SpawnEnemies(
        MapTree.Node node,
        List<RoomMaker.Platform> platforms,
        Tilemap tilemap,
        Transform player)
    {
        // 이전 방의 적 제거
        ClearEnemies();

        if (node == null || platforms == null ||
            tilemap == null || player == null)
        {
            Debug.LogWarning("적 생성에 필요한 정보가 없습니다.");
            return;
        }

        // 노드 종류 확인
        if (!Enum.TryParse(
            node.type.ToString(),
            true,
            out EnemyRoomType roomType))
        {
            Debug.Log("적 생성 규칙이 없는 노드입니다.");
            return;
        }

        // 적을 생성할 수 있는 플랫폼 찾기
        List<RoomMaker.Platform> enemyPlatforms =
            new List<RoomMaker.Platform>();

        foreach (RoomMaker.Platform platform in platforms)
        {
            // 시작 플랫폼 제외
            if (platform == platforms[0])
                continue;

            // 문 플랫폼 제외
            if (platform.type !=
                RoomMaker.Platform.PlatformType.Normal)
                continue;

            // Enemy 요소가 있는 플랫폼만 사용
            if (!platform.elements.Contains(
                RoomMaker.Platform.PlatformElement.Enemy))
                continue;

            enemyPlatforms.Add(platform);
        }

        if (enemyPlatforms.Count == 0)
        {
            Debug.Log("적을 배치할 플랫폼이 없습니다.");
            return;
        }

        // 노드 종류에 해당하는 생성 규칙 적용
        foreach (EnemySpawnEntry rule in spawnRules)
        {
            if (rule.roomType != roomType)
                continue;

            if (rule.enemyPrefab == null)
                continue;

            for (int i = 0; i < rule.count; i++)
            {
                RoomMaker.Platform platform =
                    enemyPlatforms[
                        UnityEngine.Random.Range(
                            0,
                            enemyPlatforms.Count
                        )
                    ];

                SpawnEnemy(
                    rule.enemyPrefab,
                    platform,
                    tilemap,
                    player
                );
            }
        }
    }

    private void SpawnEnemy(
        EnemyController prefab,
        RoomMaker.Platform platform,
        Tilemap tilemap,
        Transform player)
    {
        // 플랫폼이 차지하는 타일 범위
        int startX =
            platform.position.x - platform.width / 2;

        int endX = startX + platform.width - 1;

        // 양쪽 끝에서 일정 거리 떨어진 곳에 배치
        int minX = startX + edgePadding;
        int maxX = endX - edgePadding;

        int spawnX;

        if (minX <= maxX)
        {
            spawnX = UnityEngine.Random.Range(
                minX,
                maxX + 1
            );
        }
        else
        {
            spawnX = platform.position.x;
        }

        Vector3 spawnPosition =
            tilemap.GetCellCenterWorld(
                new Vector3Int(
                    spawnX,
                    platform.position.y,
                    0
                )
            );

        spawnPosition.y += spawnYOffset;

        Transform parent = enemyContainer != null
            ? enemyContainer
            : transform;

        EnemyController enemy = Instantiate(
     prefab,
     spawnPosition,
     Quaternion.identity,
     parent
 );

        enemy.Initialize(player);

        // 적 사망 컴포넌트 초기화
        EnemyLife life = enemy.GetComponent<EnemyLife>();

        if (life == null)
            life = enemy.gameObject.AddComponent<EnemyLife>();

        life.Initialize(this, enemy);

        spawnedEnemies.Add(enemy);
    }

    // 이전 방의 적 제거
    public void ClearEnemies()
    {
        foreach (EnemyController enemy in spawnedEnemies)
        {
            if (enemy != null)
                Destroy(enemy.gameObject);
        }

        spawnedEnemies.Clear();
    }

    // 살아 있는 적 수
    public int GetAliveEnemyCount()
    {
        int count = 0;

        foreach (EnemyController enemy in spawnedEnemies)
        {
            if (enemy != null)
                count++;
        }

        return count;
    }

    // 적이 사망할 때 호출
    public void NotifyEnemyDied(EnemyController enemy)
    {
        // 현재 방에서 관리하는 적만 처리
        if (!spawnedEnemies.Remove(enemy))
            return;

        Debug.Log(
            $"적 사망! 남은 적: {spawnedEnemies.Count}"
        );

        if (spawnedEnemies.Count == 0)
        {
            Debug.Log("현재 방의 모든 적 처치!");
            AllEnemiesDefeated?.Invoke();
        }
    }

    // 테스트용: 현재 방의 모든 적 즉시 처치
    public void KillAllEnemies()
    {
        // 사망 과정에서 리스트가 바뀌므로 복사본 사용
        EnemyController[] enemies =
            spawnedEnemies.ToArray();

        foreach (EnemyController enemy in enemies)
        {
            if (enemy == null)
                continue;

            EnemyLife life =
                enemy.GetComponent<EnemyLife>();

            if (life != null)
                life.Die();
        }
    }
}
