using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class MiniMapDrag : MonoBehaviour,
    IPointerDownHandler, IDragHandler
{
    [Header("지도 연결")]
    public MiniMap miniMap;

    [Header("지도 UI")]
    public RectTransform miniMapPanel;
    public RawImage mapDisplay;
    public RectTransform gameCanvas;

    [Header("버튼")]
    public Button expandButton;
    public Button closeButton;

    [Header("RenderTexture")]
    public RenderTexture mapRenderTexture;

    [Header("해상도")]
    public Vector2Int miniResolution = new Vector2Int(512, 512);
    public Vector2Int fullResolution = new Vector2Int(1920, 1080);

    [Header("지도 드래그")]
    private Vector2 previousPointerPosition;
    private bool isFullMapOpen = false;

    [Header("확대 설정")]
    public float zoomSpeed = 0.5f;

    private float previousTimeScale;

    // MiniMapPanel 원래 상태
    private Vector2 panelOriginalSize;
    private Vector2 panelOriginalPosition;
    private Vector2 panelOriginalAnchorMin;
    private Vector2 panelOriginalAnchorMax;
    private Vector2 panelOriginalOffsetMin;
    private Vector2 panelOriginalOffsetMax;
    private Vector2 panelOriginalPivot;

    // RawImage 원래 상태
    private Vector2 originalSize;
    private Vector2 originalPosition;
    private Vector2 originalAnchorMin;
    private Vector2 originalAnchorMax;
    private Vector2 originalOffsetMin;
    private Vector2 originalOffsetMax;
    private Vector3 originalScale;

    private void Start()
    {
        // MiniMapPanel 원래 상태 저장
        panelOriginalSize = miniMapPanel.sizeDelta;
        panelOriginalPosition = miniMapPanel.anchoredPosition;
        panelOriginalAnchorMin = miniMapPanel.anchorMin;
        panelOriginalAnchorMax = miniMapPanel.anchorMax;
        panelOriginalOffsetMin = miniMapPanel.offsetMin;
        panelOriginalOffsetMax = miniMapPanel.offsetMax;
        panelOriginalPivot = miniMapPanel.pivot;

        // RawImage 원래 상태 저장
        RectTransform rect = mapDisplay.rectTransform;

        originalSize = rect.sizeDelta;
        originalPosition = rect.anchoredPosition;
        originalAnchorMin = rect.anchorMin;
        originalAnchorMax = rect.anchorMax;
        originalOffsetMin = rect.offsetMin;
        originalOffsetMax = rect.offsetMax;
        originalScale = rect.localScale;

        expandButton.onClick.AddListener(OpenFullMap);
        closeButton.onClick.AddListener(CloseFullMap);
    }

    private void Update()
    {
        if (!isFullMapOpen)
            return;

        if (Mouse.current == null)
            return;

        float scroll = Mouse.current.scroll.ReadValue().y;

        if (scroll != 0)
        {
            miniMap.ZoomCamera(scroll * zoomSpeed * 0.01f);
        }
    }

    // 전체 맵 열기
    public void OpenFullMap()
    {
        if (isFullMapOpen)
            return;

        isFullMapOpen = true;

        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        // RenderTexture 해상도 증가
        SetRenderResolution(fullResolution);

        // MiniMapPanel 확장
        miniMapPanel.anchorMin = new Vector2(0.5f, 0.5f);
        miniMapPanel.anchorMax = new Vector2(0.5f, 0.5f);
        miniMapPanel.pivot = new Vector2(0.5f, 0.5f);
        miniMapPanel.anchoredPosition = Vector2.zero;
        miniMapPanel.sizeDelta = gameCanvas.rect.size;

        // RawImage 확장
        RectTransform rect = mapDisplay.rectTransform;

        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = new Vector3(1f, 2.5f, 2.5f);

        // 카메라 전체 지도 모드 진입
        miniMap.EnterFullMapMode();

        closeButton.gameObject.SetActive(true);
        expandButton.gameObject.SetActive(false);
    }

    // 전체 맵 닫기
    public void CloseFullMap()
    {
        if (!isFullMapOpen)
            return;

        isFullMapOpen = false;

        // RenderTexture 해상도 복구
        SetRenderResolution(miniResolution);

        // MiniMapPanel 복구
        miniMapPanel.anchorMin = panelOriginalAnchorMin;
        miniMapPanel.anchorMax = panelOriginalAnchorMax;
        miniMapPanel.pivot = panelOriginalPivot;
        miniMapPanel.sizeDelta = panelOriginalSize;
        miniMapPanel.anchoredPosition = panelOriginalPosition;
        miniMapPanel.offsetMin = panelOriginalOffsetMin;
        miniMapPanel.offsetMax = panelOriginalOffsetMax;

        // RawImage 복구
        RectTransform rect = mapDisplay.rectTransform;

        rect.anchorMin = originalAnchorMin;
        rect.anchorMax = originalAnchorMax;
        rect.sizeDelta = originalSize;
        rect.anchoredPosition = originalPosition;
        rect.offsetMin = originalOffsetMin;
        rect.offsetMax = originalOffsetMax;
        rect.localScale = originalScale;

        // 카메라 미니맵 모드 복귀
        miniMap.ExitFullMapMode();

        Time.timeScale = previousTimeScale;

        closeButton.gameObject.SetActive(false);
        expandButton.gameObject.SetActive(true);
    }

    // RenderTexture 해상도 변경
    private void SetRenderResolution(Vector2Int resolution)
    {
        mapRenderTexture.Release();

        mapRenderTexture.width = resolution.x;
        mapRenderTexture.height = resolution.y;

        mapRenderTexture.Create();
    }

    // 마우스 클릭 시작 위치 저장
    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isFullMapOpen)
            return;

        RectTransform rect = mapDisplay.rectTransform;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rect,
            eventData.position,
            eventData.pressEventCamera,
            out previousPointerPosition
        );
    }

    // 마우스 드래그
    public void OnDrag(PointerEventData eventData)
    {
        if (!isFullMapOpen)
            return;

        RectTransform rect = mapDisplay.rectTransform;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rect,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 currentPointerPosition))
            return;

        Vector2 delta = currentPointerPosition - previousPointerPosition;

        // MiniMap에 카메라 이동 요청
        miniMap.MoveCameraByUIDelta(delta, rect.rect.size);

        previousPointerPosition = currentPointerPosition;
    }
}