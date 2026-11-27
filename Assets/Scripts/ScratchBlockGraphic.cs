using UnityEngine;
using UnityEngine.UI;

// Each card draws its own top socket and bottom tab using a UGUI mesh.
// The next card overlaps by ConnectorDepth to make a fitted joint.
[RequireComponent(typeof(RectTransform))]
public sealed class ScratchBlockGraphic : MaskableGraphic
{
    public const float BlockHeight = 72f;
    public const float ConnectorDepth = 10f;

    [SerializeField] private bool hasTopSocket;
    [SerializeField] private bool hasBottomTab;

    public void SetConnectors(bool topSocket, bool bottomTab)
    {
        hasTopSocket = topSocket;
        hasBottomTab = bottomTab;
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        Rect bounds = GetPixelAdjustedRect();

        if (bounds.width <= 0f || bounds.height <= 0f)
            return;

        float depth = Mathf.Min(ConnectorDepth, bounds.height * 0.25f);
        float notchWidth = Mathf.Min(50f, bounds.width * 0.38f);
        float notchLeft = bounds.xMin + Mathf.Min(28f,
            (bounds.width - notchWidth) * 0.5f);
        float notchRight = notchLeft + notchWidth;
        float bodyBottom = bounds.yMin + depth;
        float bodyTop = bounds.yMax - depth;

        AddQuad(vh, bounds.xMin, bodyBottom, bounds.xMax, bodyTop);

        if (hasTopSocket)
        {
            AddQuad(vh, bounds.xMin, bodyTop, notchLeft, bounds.yMax);
            AddQuad(vh, notchRight, bodyTop, bounds.xMax, bounds.yMax);
        }
        else
            AddQuad(vh, bounds.xMin, bodyTop, bounds.xMax, bounds.yMax);

        if (hasBottomTab)
            AddQuad(vh, notchLeft, bounds.yMin, notchRight, bodyBottom);
        else
            AddQuad(vh, bounds.xMin, bounds.yMin, bounds.xMax, bodyBottom);
    }

    private void AddQuad(VertexHelper vh, float left, float bottom,
        float right, float top)
    {
        if (right <= left || top <= bottom)
            return;

        int start = vh.currentVertCount;
        Color32 tint = color;

        vh.AddVert(new Vector3(left, bottom), tint, Vector2.zero);
        vh.AddVert(new Vector3(left, top), tint, Vector2.up);
        vh.AddVert(new Vector3(right, top), tint, Vector2.one);
        vh.AddVert(new Vector3(right, bottom), tint, Vector2.right);

        vh.AddTriangle(start, start + 1, start + 2);
        vh.AddTriangle(start, start + 2, start + 3);
    }
}
