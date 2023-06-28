using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    private static Texture2D _whiteTexture;
    private static Camera _mainCamera;

    private static Ray _ray;
    private static RaycastHit _hit;

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

    public static Camera MainCamera
    {
        get
        {
            if (_mainCamera == null)
                _mainCamera = Camera.main;

            return _mainCamera;
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

    public static Vector3 MiddleOfScreenPointToWorld() => MiddleOfScreenPointToWorld(MainCamera);

    public static Vector3 MiddleOfScreenPointToWorld(Camera cam)
    {
        _ray = cam.ScreenPointToRay(0.5f * new Vector2(Screen.width, Screen.height));
        if (Physics.Raycast(
                _ray,
                out _hit,
                1000f,
                Globals.TerrainLayerMask
            )) return _hit.point;
        return Vector3.zero;
    }

    public static Vector3[] ScreenCornersToWorldPoints() => ScreenCornersToWorldPoints(MainCamera);

    public static Vector3[] ScreenCornersToWorldPoints(Camera cam)
    {
        Vector3[] corners = new Vector3[4];
        for (int i = 0; i < 4; i++)
        {
            _ray = cam.ScreenPointToRay(new Vector2((i % 2), (i / 2)));
            if (Physics.Raycast(
                    _ray,
                    out _hit,
                    1000f,
                    Globals.TerrainLayerMask
                )) corners[i] = _hit.point;
        }
        return corners;
    }

    public static Vector3 ProjectOnTerrain(Vector3 pos)
    {
        Vector3 initialPos = pos + Vector3.up * 1000f;
        if (Physics.Raycast(initialPos, Vector3.down, out _hit, 2000f, Globals.FlatTerrainLayerMask))
            pos = _hit.point;
        return pos;
    }

    public static (Vector3, Vector3) GetCameraWorldBounds()
    {
        Vector3 bottomLeftCorner = new Vector3(0f, 0f);
        Vector3 topRightCorner = new Vector3(1f, 1f);
        float dist = 1000f;

        _ray = MainCamera.ViewportPointToRay(bottomLeftCorner);
        Vector3 bottomLeft = GameManager.Instance.MapWrapperCollider.Raycast(_ray, out _hit, dist)
            ? _hit.point : Vector3.zero;
            
        _ray = MainCamera.ViewportPointToRay(topRightCorner);
        Vector3 topRight = GameManager.Instance.MapWrapperCollider.Raycast(_ray, out _hit, dist)
            ? _hit.point : Vector3.zero;

        return (bottomLeft, topRight);
    }

    public static Dictionary<InGameResource, int> ConvertResourceValueListToDictionary(List<ResourceValue> convertationList)
    {
        var resultDictionary = new Dictionary<InGameResource, int>();
        foreach (var value in convertationList)
        {
            if (resultDictionary.ContainsKey(value.Resource))
            {
                resultDictionary[value.Resource] += value.Amount;
            }
            else
            {
                resultDictionary.Add(value.Resource, value.Amount);
            }
        }
        return resultDictionary;
    }
}
