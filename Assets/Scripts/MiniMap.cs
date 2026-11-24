using UnityEngine;

public class MiniMap : MonoBehaviour
{
    [Header("지도")]
    public RectTransform mapContent;

    [Header("카메라")]
    public Camera miniMapCamera;
    public float miniCameraSize = 5f;
    public float mapPadding = 1.1f;

    [Header("플레이어 표시")]
    public RectTransform playerNodeUI;
    public MapManager mapManager;
    public MapMaker mapMaker;

    private bool isFullMapOpen = false;

    private void Start()
    {
        miniMapCamera.orthographicSize = miniCameraSize;
    }

    private void LateUpdate()
    {
        if (isFullMapOpen)
            return;

        UpdatePlayerNodeUI();
        FollowPlayer();
    }

    // 현재 방에 맞게 플레이어 표시 이동
    private void UpdatePlayerNodeUI()
    {
        if (mapManager.currentNode == null)
            return;

        RectTransform currentRoomUI =
            mapMaker.GetNodeUI(mapManager.currentNode);

        if (currentRoomUI == null)
            return;

        playerNodeUI.position = currentRoomUI.position;
    }

    // 플레이어 위치 추적
    private void FollowPlayer()
    {
        if (playerNodeUI == null)
            return;

        Vector3 targetPosition = playerNodeUI.position;

        miniMapCamera.transform.position = new Vector3(
            targetPosition.x,
            targetPosition.y,
            miniMapCamera.transform.position.z
        );
    }

    // 전체 맵 모드 진입
    public void EnterFullMapMode()
    {
        isFullMapOpen = true;

        CenterOnMap();
        SetFullMapCameraSize();
    }

    // 전체 맵 모드 종료
    public void ExitFullMapMode()
    {
        isFullMapOpen = false;

        miniMapCamera.orthographicSize = miniCameraSize;

        UpdatePlayerNodeUI();
        FollowPlayer();
    }

    // 맵의 중심으로 카메라 이동
    public void CenterOnMap()
    {
        Vector3[] corners = new Vector3[4];
        mapContent.GetWorldCorners(corners);

        Vector3 center = (corners[0] + corners[2]) / 2f;

        miniMapCamera.transform.position = new Vector3(
            center.x,
            center.y,
            miniMapCamera.transform.position.z
        );
    }

    // 전체 지도가 카메라에 들어오도록 시야 크기 계산
    public void SetFullMapCameraSize()
    {
        Vector3[] corners = new Vector3[4];
        mapContent.GetWorldCorners(corners);

        float width = Vector3.Distance(corners[0], corners[3]);
        float height = Vector3.Distance(corners[0], corners[1]);

        float aspect = miniMapCamera.aspect;

        float sizeByHeight = height / 2f;
        float sizeByWidth = width / (2f * aspect);

        miniMapCamera.orthographicSize =
            Mathf.Max(sizeByHeight, sizeByWidth) * mapPadding;
    }

    // UI 드래그 거리를 카메라 이동으로 변환
    public void MoveCameraByUIDelta(Vector2 delta, Vector2 displaySize)
    {
        if (!isFullMapOpen || displaySize.x <= 0 || displaySize.y <= 0)
            return;

        float worldPerUnitY =
            (miniMapCamera.orthographicSize * 2f) / displaySize.y;

        float worldPerUnitX =
            (miniMapCamera.orthographicSize * 2f * miniMapCamera.aspect)
            / displaySize.x;

        Vector3 movement =
            -miniMapCamera.transform.right * delta.x * worldPerUnitX
            - miniMapCamera.transform.up * delta.y * worldPerUnitY;

        miniMapCamera.transform.position += movement;
    }

    //확대 함수
    public void ZoomCamera(float amount)
    {
        if (!isFullMapOpen)
            return;

        miniMapCamera.orthographicSize = Mathf.Clamp(
            (miniMapCamera.orthographicSize - amount) * 3f,
            3f,
            90f
        );
    }
}