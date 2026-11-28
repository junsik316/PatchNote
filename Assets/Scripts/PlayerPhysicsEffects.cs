using UnityEngine;

// Player keeps and applies its own physics values; cards are evaluated by CardManager.
public class PlayerPhysicsEffects : MonoBehaviour, ICardEffectReceiver
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
    private CardManager registeredManager;

    public EffectTarget Target => EffectTarget.Player;

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
        RegisterIfAvailable();
        if (registeredManager == null)
            RefreshCardEffects(null);
    }

    private void OnEnable()
    {
        if (rb != null)
            RegisterIfAvailable();
    }

    private void RegisterIfAvailable()
    {
        CardManager manager = CardManager.Instance;
        if (manager == null || manager == registeredManager) return;
        if (registeredManager != null)
            registeredManager.UnregisterReceiver(this);
        registeredManager = manager;
        manager.RegisterReceiver(this);
    }

    private void OnDisable()
    {
        if (registeredManager != null)
            registeredManager.UnregisterReceiver(this);
        registeredManager = null;
    }

    // Initial registration and later card changes share the same refresh path.
    // Base values remain on the player and are never overwritten by cards.
    public void RefreshCardEffects(CardManager manager)
    {
        if (rb == null) return;

        MoveSpeed = Mathf.Max(0f, Modified(manager, PhysicsType.MoveSpeed, baseMoveSpeed));
        JumpForce = Mathf.Max(0f, Modified(manager, PhysicsType.JumpForce, baseJumpForce));
        rb.gravityScale = Modified(manager, PhysicsType.GravityScale, baseGravityScale);

        if (runtimeMaterial != null)
        {
            runtimeMaterial.friction = Mathf.Clamp01(
                Modified(manager, PhysicsType.Friction, baseFriction));
            runtimeMaterial.bounciness = Mathf.Clamp01(
                Modified(manager, PhysicsType.Bounciness, baseBounciness));
        }
    }

    private float Modified(CardManager manager, PhysicsType property, float baseValue)
    {
        return manager != null
            ? manager.GetModifiedPhysicsValue(Target, property, baseValue)
            : baseValue;
    }

    private void OnDestroy()
    {
        if (runtimeMaterial != null)
            Destroy(runtimeMaterial);
    }
}
