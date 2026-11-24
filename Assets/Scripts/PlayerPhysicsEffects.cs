
using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPhysicsEffects : MonoBehaviour
{
    [Header("플레이어 물리 컴포넌트")]
    public Rigidbody2D rb;
    public Collider2D bodyCollider;

    [Header("기본 이동 계수")]
    public float baseMoveSpeed = 5f;
    public float baseJumpForce = 10f;

    // PlayerController가 실제로 사용할 값
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

        // 다른 오브젝트의 물리 재질이 바뀌지 않도록 복제
        if (bodyCollider != null)
        {
            PhysicsMaterial2D original =
                bodyCollider.sharedMaterial;

            if (original != null)
            {
                runtimeMaterial = Instantiate(original);
            }
            else
            {
                runtimeMaterial = new PhysicsMaterial2D(
                    "PlayerRuntimeMaterial"
                );

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
        // 플레이어가 새로 생성되어도 카드 효과 적용
        if (GameManager.Instance != null &&
            GameManager.Instance.uiManager != null)
        {
            Recalculate(
                GameManager.Instance.uiManager.inventory
            );
        }
    }

    public void Recalculate(List<CardInstance> inventory)
    {
        // 매번 기본값부터 다시 계산
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
                if (root == null)
                    continue;

                // 연결된 자식 카드는 따로 계산하지 않음
                if (root.parentCard != null)
                    continue;

                // 물리 카드만 처리
                if (root.cardType != CardType.Physics)
                    continue;

                // 문자열을 물리 대상 열거형으로 변환
                if (!Enum.TryParse(
                    root.physicsType,
                    true,
                    out PhysicsType target))
                {
                    Debug.LogWarning(
                        "알 수 없는 물리 대상: " + root.physicsType
                    );
                    continue;
                }

                if (!values.ContainsKey(target))
                    continue;

                float currentValue = values[target];

                // 연결된 숫자 카드들을 차례대로 계산
                currentValue = CalculateChain(
                    root,
                    currentValue
                );

                values[target] = currentValue;
            }
        }

        // 최종 물리 계수 적용
        MoveSpeed = Mathf.Max(
            0f,
            values[PhysicsType.MoveSpeed]
        );

        JumpForce = Mathf.Max(
            0f,
            values[PhysicsType.JumpForce]
        );

        rb.gravityScale = values[
            PhysicsType.GravityScale
        ];

        if (runtimeMaterial != null)
        {
            runtimeMaterial.friction = Mathf.Clamp01(
                values[PhysicsType.Friction]
            );

            runtimeMaterial.bounciness = Mathf.Clamp01(
                values[PhysicsType.Bounciness]
            );
        }

        Debug.Log(
            $"물리 계수 갱신: " +
            $"Speed={MoveSpeed}, " +
            $"Jump={JumpForce}, " +
            $"Gravity={rb.gravityScale}"
        );
    }

    private float CalculateChain(
        CardInstance root,
        float initialValue)
    {
        float result = initialValue;

        HashSet<CardInstance> visited =
            new HashSet<CardInstance>();

        visited.Add(root);

        CardInstance current = root.childCard;

        while (current != null)
        {
            // 잘못 연결된 순환 구조 방지
            if (!visited.Add(current))
            {
                Debug.LogWarning("카드 연결에 순환이 있습니다.");
                break;
            }

            if (current.cardType == CardType.Number)
            {
                result = ApplyNumberCard(
                    result,
                    current
                );
            }

            current = current.childCard;
        }

        return result;
    }

    private float ApplyNumberCard(
        float value,
        CardInstance numberCard)
    {
        float number = (float)numberCard.numberValue;
        float result = value;

        // 기존 CardOperator의 이름 사용
        string op = numberCard.cardOperator.ToString();

        switch (op)
        {
            case "Multiply":
            case "Multipy":
                result = value * number;
                break;

            case "Divide":
                if (Mathf.Approximately(number, 0f))
                {
                    Debug.LogWarning("0으로 나눌 수 없습니다.");
                    return value;
                }

                result = value / number;
                break;

            case "Add":
                result = value + number;
                break;

            case "Subtract":
                result = value - number;
                break;

            default:
                Debug.LogWarning(
                    "지원하지 않는 연산자: " + op
                );
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
