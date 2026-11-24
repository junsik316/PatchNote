
using UnityEngine;
using UnityEngine.UI;

public class MapLine : MonoBehaviour
{
    private RectTransform rect;
    private Image image;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        image = GetComponent<Image>();
    }

    public void SetLine(
        Vector2 start,
        Vector2 end,
        float thickness)
    {
        Vector2 direction = end - start;

        float distance = direction.magnitude;
        float angle = Mathf.Atan2(
            direction.y,
            direction.x
        ) * Mathf.Rad2Deg;

        rect.anchoredPosition = (start + end) / 2f;

        rect.sizeDelta = new Vector2(
            distance,
            thickness
        );

        rect.localRotation =
            Quaternion.Euler(0f, 0f, angle);
    }
}
