
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("게임 매니저")]
    public MapManager mapManager;
    public CardManager cardManager;
    public UIManager uiManager;

    // 플레이어의 유일한 참조
    public Playercontroller Player { get; private set; }

    // 플레이어의 물리 효과 컴포넌트
    public PlayerPhysicsEffects PlayerPhysics
    {
        get
        {
            if (Player == null)
                return null;

            return Player.GetComponent<PlayerPhysicsEffects>();
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        if (CardManager.Instance != null)
            cardManager = CardManager.Instance;
    }

    public void RegisterPlayer(Playercontroller player)
    {
        if (player == null)
        {
            Debug.LogError("플레이어 등록 실패");
            return;
        }

        Player = player;
    }
}
