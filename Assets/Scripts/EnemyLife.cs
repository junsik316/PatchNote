
using UnityEngine;

public class EnemyLife : MonoBehaviour
{
    private RoomEnemySpawner spawner;
    private EnemyController enemy;

    private bool isDead = false;

    public void Initialize(
        RoomEnemySpawner owner,
        EnemyController controller)
    {
        spawner = owner;
        enemy = controller;
        isDead = false;
    }

    public void Die()
    {
        if (isDead)
            return;

        isDead = true;

        // 스포너에 사망 알림
        if (spawner != null)
            spawner.NotifyEnemyDied(enemy);

        Destroy(gameObject);
    }
}
