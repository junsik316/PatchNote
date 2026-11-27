
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public enum EnemyState
    {
        Idle,
        Chase,
        Attack
    }

    [Header("컴포넌트")]
    public Rigidbody2D rb;

    [Header("이동")]
    public float moveSpeed = 3f;

    [Header("감지 범위")]
    public float detectionRange = 7f;
    public float loseRange = 10f;
    public float attackRange = 1.5f;

    [Header("공격")]
    public int attackDamage = 10;
    public float attackInterval = 1.5f;

    private Transform player;
    private float nextAttackTime;

    public EnemyState CurrentState { get; private set; }
        = EnemyState.Idle;

    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        // 생성 시 플레이어 참조를 받지 못한 경우
        if (player == null)
        {
            GameObject playerObject =
                GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
                player = playerObject.transform;
        }
    }

    public void Initialize(Transform target)
    {
        player = target;
        CurrentState = EnemyState.Idle;
    }

    private void FixedUpdate()
    {
        if (rb == null)
            return;

        if (player == null)
        {
            CurrentState = EnemyState.Idle;
            StopMoving();
            return;
        }

        float distance = Vector2.Distance(
            transform.position,
            player.position
        );

        UpdateState(distance);

        switch (CurrentState)
        {
            case EnemyState.Idle:
                StopMoving();
                break;

            case EnemyState.Chase:
                ChasePlayer();
                break;

            case EnemyState.Attack:
                StopMoving();
                TryAttack();
                break;
        }
    }

    private void UpdateState(float distance)
    {
        // 추적 중이 아닐 때는 감지 범위로 판단
        if (CurrentState == EnemyState.Idle)
        {
            if (distance <= detectionRange)
                CurrentState = EnemyState.Chase;
            else
                return;
        }

        // 추적 중에는 더 넓은 해제 범위 사용
        if (distance > loseRange)
        {
            CurrentState = EnemyState.Idle;
            return;
        }

        // 공격 범위 안에 들어오면 공격
        if (distance <= attackRange)
            CurrentState = EnemyState.Attack;
        else
            CurrentState = EnemyState.Chase;
    }

    private void ChasePlayer()
    {
        float direction = Mathf.Sign(
            player.position.x - transform.position.x
        );

        rb.linearVelocity = new Vector2(
            direction * moveSpeed,
            rb.linearVelocity.y
        );

        // 플레이어 방향 바라보기
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * direction;
        transform.localScale = scale;
    }

    private void StopMoving()
    {
        rb.linearVelocity = new Vector2(
            0f,
            rb.linearVelocity.y
        );
    }

    private void TryAttack()
    {
        if (Time.time < nextAttackTime)
            return;

        nextAttackTime = Time.time + attackInterval;

        // 공격 순간에도 거리 확인
        float distance = Vector2.Distance(
            transform.position,
            player.position
        );

        if (distance > attackRange)
            return;

        Debug.Log($"{gameObject.name} 공격!");

        // 피해를 받을 수 있는 컴포넌트 검색
        IDamagable target =
            player.GetComponentInParent<IDamagable>();

        if (target != null)
            target.TakeDamage(attackDamage);
    }

    private void OnValidate()
    {
        detectionRange = Mathf.Max(0.1f, detectionRange);

        loseRange = Mathf.Max(
            detectionRange,
            loseRange
        );

        attackRange = Mathf.Clamp(
            attackRange,
            0.1f,
            detectionRange
        );

        attackInterval = Mathf.Max(0.1f, attackInterval);
    }

    private void OnDrawGizmosSelected()
    {
        // 감지 거리
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(
            transform.position,
            detectionRange
        );

        // 추적 해제 거리
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(
            transform.position,
            loseRange
        );

        // 공격 거리
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(
            transform.position,
            attackRange
        );
    }
}
