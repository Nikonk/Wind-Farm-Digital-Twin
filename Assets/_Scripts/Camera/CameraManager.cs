using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraManager : MonoBehaviour
{
    public float translationSpeed = 60f;
    public float altitude = 40f;

    private Camera _camera;
    private RaycastHit _hit;
    private Ray _ray;

    private Vector3 _forwardDir;

    private int _mouseOnScreenBorder;

    private Coroutine _mouseOnScreenCoroutine;

    public float zoomSpeed = 30f;

    private void Awake() 
    {
        _camera = GetComponent<Camera>();
        _forwardDir = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        _mouseOnScreenBorder = -1;
        _mouseOnScreenCoroutine = null;
    }

    private void Update() 
    {
        if (_mouseOnScreenBorder >= 0)
        {
            _TranslateCamera(_mouseOnScreenBorder);
        }
        else
        {
            if (Input.GetKey(KeyCode.UpArrow))
                _TranslateCamera(0);
            if (Input.GetKey(KeyCode.RightArrow))
                _TranslateCamera(1);
            if (Input.GetKey(KeyCode.DownArrow))
                _TranslateCamera(2);
            if (Input.GetKey(KeyCode.LeftArrow))
                _TranslateCamera(3);
        }

        if (Math.Abs(Input.mouseScrollDelta.y) > 0)
            _Zoom(Input.mouseScrollDelta.y > 0f ? 1 : -1);
    }

    private void _TranslateCamera(int dir)
    {
        if (dir == 0)
            transform.Translate(_forwardDir * Time.deltaTime * translationSpeed, Space.World);
        else if (dir == 1)
            transform.Translate(transform.right * Time.deltaTime * translationSpeed);
        else if (dir == 2)
            transform.Translate(-_forwardDir * Time.deltaTime * translationSpeed, Space.World);
        else if (dir == 3)
            transform.Translate(-transform.right * Time.deltaTime * translationSpeed);
            
        _ray = new Ray(transform.position, Vector3.up * -1000f);
        if (Physics.Raycast(
                _ray,
                out _hit,
                1000f,
                Globals.TERRAIN_LAYER_MASK
            ))
            transform.position = _hit.point + Vector3.up * altitude;
    }

    public void OnMouseEnterScreenBorder(int borderIndex)
    {
        _mouseOnScreenCoroutine = StartCoroutine(_SetMouseOnScreenBorder(borderIndex));
    }

    public void OnMouseExitScreenBorder()
    {
        StopCoroutine(_mouseOnScreenCoroutine);
        _mouseOnScreenBorder = -1;
    }

    private IEnumerator _SetMouseOnScreenBorder(int borderIndex)
    {
        yield return new WaitForSeconds(0.3f);
        _mouseOnScreenBorder = borderIndex;
    }

    private void _Zoom(int zoomDir)
    {
        _camera.orthographicSize += zoomDir * Time.deltaTime * zoomSpeed;
        _camera.orthographicSize = Mathf.Clamp(_camera.orthographicSize, 8f, 26f);
    }
}
