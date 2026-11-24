using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;

public class Playercontroller : MonoBehaviour
{
    Rigidbody2D rb;
    Transform Transform;
    //움직임 속도 조절 변수
    public float  speed = 5f;
    public float jumpForce = 5f;
    Vector2 MoveVector;
    //점프 상태를 확인하기 위한 변수
    public bool isOnGround = true;
    public bool isJumping = false;
    public bool isDoubleJumped = false;

    [Header("카드 효과")]
    public Collider2D playerCollider;
    public PhysicsMaterial2D playerMaterial;

    public CardManager cardManager;

    private bool isRunning = false;

    private PlayerPhysicsEffects physicsEffects;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        physicsEffects = GetComponent<PlayerPhysicsEffects>();
        
    }

    void Start()
    {
        Transform = GetComponent<Transform>();
        rb = GetComponent<Rigidbody2D>();

        playerCollider = GetComponent<Collider2D>();

        playerMaterial = Instantiate(playerCollider.sharedMaterial);
        playerCollider.sharedMaterial = playerMaterial;
    }

    // Update is called once per frame
    void Update()
    {
        rb.transform.position += new Vector3(MoveVector.x, MoveVector.y, 0) * speed * Time.deltaTime;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        if (Keyboard.current != null &&
    Keyboard.current.eKey.wasPressedThisFrame)
        {
            GameManager.Instance.uiManager.ToggleInventory();
        }
    }
    //움직임 입력값을 받는 함수
    public void Move(InputAction.CallbackContext value)
    {
        UpdateSpeed();
        MoveVector = value.ReadValue<Vector2>();
        
    }
    //점프
    public void Jump(InputAction.CallbackContext callbackContext)
    {
        if ((isOnGround && callbackContext.performed && !isJumping )|| (!isDoubleJumped && callbackContext.performed))
        {
            rb.constraints = RigidbodyConstraints2D.FreezePosition;
            rb.AddForce(Vector2.up * physicsEffects.JumpForce, ForceMode2D.Impulse);
            rb.constraints = RigidbodyConstraints2D.None;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            if (isJumping) isDoubleJumped = true;
            isJumping = true;
            
        }

        
        
    }
    //달리기
    public void Run(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.performed && !isJumping)
        {
            isRunning = true;
            UpdateSpeed();
        }
        else if (callbackContext.canceled)
        {
            isRunning = false;
            UpdateSpeed();
        }
    }

    // 충돌 감지
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                // 충돌 면의 법선 벡터가 충분히 위쪽을 향하는지 확인
                if (contact.normal.y > 0.5f)
                {
                    isOnGround = true;
                    isJumping = false;
                    isDoubleJumped = false;
                    break;
                }
            }
        }
    }


   

    private void UpdateSpeed()
    {
        speed = (isRunning ? physicsEffects.MoveSpeed *2f : physicsEffects.MoveSpeed);
    }

}
