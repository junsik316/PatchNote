using System.Collections.Generic;
using UnityEngine;

// 이번 단계에서는 기존 플레이어 물리 효과를 유지하기 위해
// CardInstance 접근 부분만 파생 타입 기반으로 변경한다.
// 계산기/적용기 분리는 다음 리팩터링 단계에서 수행한다.
public class PlayerPhysicsEffects : MonoBehaviour
{
    [Header("플레이어 물리 컴포넌트")]
    public Rigidbody2D rb;
    public Collider2D bodyCollider;

    [Header("기본 이동 계수")]
    public float baseMoveSpeed = 5f;
    public float baseJumpForce = 10f;

    public float MoveSpeed { get; private set; }
    public float JumpForce { get; private set; }

    private float baseGravityScale;
    private float baseFriction;
    private float baseBounciness;
    private PhysicsMaterial2D runtimeMaterial;

    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();
        if (bodyCollider == null)
            bodyCollider = GetComponent<Collider2D>();

        if (rb == null)
        {
            Debug.LogError("Rigidbody2D가 없습니다.", this);
            enabled = false;
            return;
        }

        MoveSpeed = baseMoveSpeed;
        JumpForce = baseJumpForce;
        baseGravityScale = rb.gravityScale;

        if (bodyCollider != null)
        {
            PhysicsMaterial2D original = bodyCollider.sharedMaterial;
            if (original != null)
            {
                runtimeMaterial = Instantiate(original);
            }
            else
            {
                runtimeMaterial = new PhysicsMaterial2D("PlayerRuntimeMaterial");
                runtimeMaterial.friction = 0.4f;
                runtimeMaterial.bounciness = 0f;
            }

            baseFriction = runtimeMaterial.friction;
            baseBounciness = runtimeMaterial.bounciness;
            bodyCollider.sharedMaterial = runtimeMaterial;
        }
    }

    private void Start()
    {
        if (GameManager.Instance != null &&
            GameManager.Instance.uiManager != null)
        {
            Recalculate(GameManager.Instance.uiManager.inventory);
        }
    }

    public void Recalculate(List<CardInstance> inventory)
    {
        Dictionary<PhysicsType, float> values =
            new Dictionary<PhysicsType, float>()
            {
                { PhysicsType.MoveSpeed, baseMoveSpeed },
                { PhysicsType.JumpForce, baseJumpForce },
                { PhysicsType.GravityScale, baseGravityScale },
                { PhysicsType.Friction, baseFriction },
                { PhysicsType.Bounciness, baseBounciness }
            };

        if (inventory != null)
        {
            foreach (CardInstance root in inventory)
            {
                if (root == null || root.parentCard != null)
                    continue;

                // 이전의 문자열 physicsType과 CardType 필드 대신
                // 구체 타입에 저장된 enum 값을 직접 사용한다.
                if (!(root is PhysicsCardInstance physicsCard))
                    continue;

                PhysicsType target = physicsCard.physicsType;
                if (!values.ContainsKey(target))
                    continue;

                values[target] = CalculateChain(root, values[target]);
            }
        }

        MoveSpeed = Mathf.Max(0f, values[PhysicsType.MoveSpeed]);
        JumpForce = Mathf.Max(0f, values[PhysicsType.JumpForce]);
        rb.gravityScale = values[PhysicsType.GravityScale];

        if (runtimeMaterial != null)
        {
            runtimeMaterial.friction = Mathf.Clamp01(values[PhysicsType.Friction]);
            runtimeMaterial.bounciness = Mathf.Clamp01(values[PhysicsType.Bounciness]);
        }

        Debug.Log(
            $"물리 계수 갱신: Speed={MoveSpeed}, " +
            $"Jump={JumpForce}, Gravity={rb.gravityScale}"
        );
    }

    private float CalculateChain(CardInstance root, float initialValue)
    {
        float result = initialValue;
        HashSet<CardInstance> visited = new HashSet<CardInstance>();
        visited.Add(root);
        CardInstance current = root.childCard;

        while (current != null)
        {
            if (!visited.Add(current))
            {
                Debug.LogWarning("카드 연결에 순환이 있습니다.");
                break;
            }

            if (current is NumberCardInstance numberCard)
                result = ApplyNumberCard(result, numberCard);

            current = current.childCard;
        }
        return result;
    }

    private float ApplyNumberCard(float value, NumberCardInstance numberCard)
    {
        float number = numberCard.numberValue;
        float result;

        switch (numberCard.cardOperator)
        {
            case CardOperator.Multiply:
                result = value * number;
                break;
            case CardOperator.Divide:
                if (Mathf.Approximately(number, 0f))
                {
                    Debug.LogWarning("0으로 나눌 수 없습니다.");
                    return value;
                }
                result = value / number;
                break;
            case CardOperator.Add:
                result = value + number;
                break;
            case CardOperator.Subtract:
                result = value - number;
                break;
            default:
                Debug.LogWarning("지원하지 않는 연산자: " + numberCard.cardOperator);
                return value;
        }

        if (float.IsNaN(result) || float.IsInfinity(result))
        {
            Debug.LogWarning("유효하지 않은 물리 계산 결과입니다.");
            return value;
        }
        return result;
    }

    private void OnDestroy()
    {
        if (runtimeMaterial != null)
            Destroy(runtimeMaterial);
    }
}
