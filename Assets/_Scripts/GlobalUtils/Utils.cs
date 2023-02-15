using UnityEngine;

public static class Utils
{
    private static Texture2D _whiteTexture;
    public static Texture2D WhiteTexture
    {
        get
        {
            if (_whiteTexture == null)
            {
                _whiteTexture = new Texture2D(1, 1);
                _whiteTexture.SetPixel(0, 0, Color.white);
                _whiteTexture.Apply();
            }
            return _whiteTexture;
        }
    }

    public static void DrawScreenRect(Rect rect, Color color)
    {
        GUI.color = color;
        GUI.DrawTexture(rect, WhiteTexture);
        GUI.color = Color.white;
    }

    public static void DrawScreenRectBorder(Rect rect, float thickness, Color color)
    {
        Utils.DrawScreenRect(new Rect(rect.xMin, rect.yMin, rect.width, thickness), color);
        Utils.DrawScreenRect(new Rect(rect.xMin, rect.yMin, thickness, rect.height), color);
        Utils.DrawScreenRect(new Rect(rect.xMax - thickness, rect.yMin, thickness, rect.height), color);
        Utils.DrawScreenRect(new Rect(rect.xMin, rect.yMax - thickness, rect.width, thickness), color);
    }

    public static Rect GetScreenRect(Vector3 screenPosition1, Vector3 screenPosition2)
    {
        screenPosition1.y = Screen.height - screenPosition1.y;
        screenPosition2.y = Screen.height - screenPosition2.y;
        Vector3 topLeft = Vector3.Min(screenPosition1, screenPosition2);
        Vector3 bottomRight = Vector3.Max(screenPosition1, screenPosition2);
        return Rect.MinMaxRect(topLeft.x, topLeft.y, bottomRight.x, bottomRight.y);
    }

    public static Bounds GetViewportBounds(Camera camera, Vector3 screenPosition1, Vector3 screenPosition2)
    {
        Vector3 v1 = Camera.main.ScreenToViewportPoint(screenPosition1);
        Vector3 v2 = Camera.main.ScreenToViewportPoint(screenPosition2);
        Vector3 min = Vector3.Min(v1, v2);
        Vector3 max = Vector3.Max(v1, v2);
        min.z = camera.nearClipPlane;
        max.z = camera.farClipPlane;

        Bounds bounds = new Bounds();
        bounds.SetMinMax(min, max);
        return bounds;
    }

    public static Vector3 MiddleOfScreenPointToWorld()
        { return MiddleOfScreenPointToWorld(Camera.main); }
    public static Vector3 MiddleOfScreenPointToWorld(Camera cam)
    {
        RaycastHit hit;
        Ray ray = cam.ScreenPointToRay(0.5f * new Vector2(Screen.width, Screen.height));
        if (Physics.Raycast(
                ray,
                out hit,
                1000f,
                Globals.TERRAIN_LAYER_MASK
            )) return hit.point;
        return Vector3.zero;
    }

    public static Vector3[] ScreenCornersToWorldPoints()
        { return ScreenCornersToWorld(Camera.main); }
    public static Vector3[] ScreenCornersToWorld(Camera cam)
    {
        Vector3[] corners = new Vector3[4];
        RaycastHit hit;
        for (int i = 0; i < 4; i++)
        {
            Ray ray = cam.ScreenPointToRay(new Vector2((i % 2) * Screen.width, (int)(i / 2) * Screen.height));
            if (Physics.Raycast(
                    ray,
                    out hit,
                    1000f,
                    Globals.FLAT_TERRAIN_LAYER_MASK
                )) corners[i] = hit.point;
        }
        return corners;
    }
}
