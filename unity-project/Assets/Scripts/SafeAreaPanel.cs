using UnityEngine;

// Keep a 1280x720 UI layout inside the device safe area.
public class SafeAreaPanel : MonoBehaviour
{
    public bool simulateInsets;
    private RectTransform area, content;

    public RectTransform Initialize(RectTransform parent)
    {
        area = (RectTransform)transform;
        area.SetParent(parent, false);
        content = new GameObject("SafeContent", typeof(RectTransform))
            .GetComponent<RectTransform>();
        content.SetParent(area, false);
        content.anchorMin = content.anchorMax = new Vector2(0.5f, 0.5f);
        content.sizeDelta = new Vector2(1280, 720);
        Apply();
        return content;
    }

    public void Apply()
    {
        if (content == null || Screen.width == 0 || Screen.height == 0) return;
        Rect safe = Screen.safeArea;
        if (simulateInsets)
            safe = new Rect(Screen.width * 0.08f, Screen.height * 0.05f,
                Screen.width * 0.84f, Screen.height * 0.9f);
        ApplyRect(safe, new Vector2(Screen.width, Screen.height));
    }

    public void ApplyRect(Rect safe, Vector2 screen)
    {
        if (content == null || screen.x <= 0 || screen.y <= 0) return;
        area.anchorMin = new Vector2(safe.xMin / screen.x, safe.yMin / screen.y);
        area.anchorMax = new Vector2(safe.xMax / screen.x, safe.yMax / screen.y);
        area.offsetMin = area.offsetMax = Vector2.zero;
        var parent = (RectTransform)area.parent;
        float width = parent.rect.width * safe.width / screen.x;
        float height = parent.rect.height * safe.height / screen.y;
        float scale = Mathf.Min(width / 1280f, height / 720f);
        content.localScale = Vector3.one * scale;
    }

    private void LateUpdate() => Apply();
}
