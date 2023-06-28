using UnityEngine;

[RequireComponent(typeof(Camera))]
public class MinimapManager : MonoBehaviour
{
    [SerializeField] private Material _mapFOVIndicator;
    [SerializeField] private float _lineWidth = 0.01f;

    private static Material _indicatorMat;

    private Camera _minimapCam;


    private void Start()
    {
        if (_indicatorMat == null)
            _indicatorMat = _mapFOVIndicator;

        _minimapCam = GetComponent<Camera>();
    }

    public void OnPostRender()
    {
        (Vector3 minWorldPoint, Vector3 maxWorldPoint) = Utils.GetCameraWorldBounds();
        Vector3 minViewportPoint = _minimapCam.WorldToViewportPoint(minWorldPoint);
        Vector3 maxViewportPoint = _minimapCam.WorldToViewportPoint(maxWorldPoint);

        float minX = minViewportPoint.x;
        float minY = minViewportPoint.y;
        float maxX = maxViewportPoint.x;
        float maxY = maxViewportPoint.y;

        GL.PushMatrix();
        {
            _indicatorMat.SetPass(0);
            GL.LoadOrtho();

            GL.Begin(GL.QUADS);
            GL.Color(new Color(1f, 1f, 0.85f));
            {
                GL.Vertex(new Vector3(minX, minY + _lineWidth, 0));
                GL.Vertex(new Vector3(minX, minY - _lineWidth, 0));
                GL.Vertex(new Vector3(maxX, minY - _lineWidth, 0));
                GL.Vertex(new Vector3(maxX, minY + _lineWidth, 0));

                GL.Vertex(new Vector3(minX + _lineWidth, minY, 0));
                GL.Vertex(new Vector3(minX - _lineWidth, minY, 0));
                GL.Vertex(new Vector3(minX - _lineWidth, maxY, 0));
                GL.Vertex(new Vector3(minX + _lineWidth, maxY, 0));

                GL.Vertex(new Vector3(minX, maxY + _lineWidth, 0));
                GL.Vertex(new Vector3(minX, maxY - _lineWidth, 0));
                GL.Vertex(new Vector3(maxX, maxY - _lineWidth, 0));
                GL.Vertex(new Vector3(maxX, maxY + _lineWidth, 0));

                GL.Vertex(new Vector3(maxX + _lineWidth, minY, 0));
                GL.Vertex(new Vector3(maxX - _lineWidth, minY, 0));
                GL.Vertex(new Vector3(maxX - _lineWidth, maxY, 0));
                GL.Vertex(new Vector3(maxX + _lineWidth, maxY, 0));
            }
            GL.End();
        }
        GL.PopMatrix();
    }
}
