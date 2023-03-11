using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraManager : MonoBehaviour
{
    public float translationSpeed = 60f;
    public float altitude = 40f;
    public float zoomSpeed = 30f;
    public bool autoAdaptAltitude = false;

    private Camera _camera;
    private RaycastHit _hit;
    private Ray _ray;

    private Vector3 _forwardDir;
    private int _mouseOnScreenBorder;
    private Coroutine _mouseOnScreenCoroutine;
    private float _distance = 500f;
    
    private float _minX;
    private float _maxX;
    private float _minZ;
    private float _maxZ;
    private Vector3 _camOffset;
    private Vector3 _camHalfViewZone;
    private float _camMinimapBuffer = 10f;


    private void Start() 
    {        
        _minX = 0;
        _maxX = 1000;
        _minZ = 0;
        _maxZ = 1000;
        (Vector3 minWorldPoint, Vector3 maxWorldPoint) = Utils.GetCameraWorldBounds();
        _camOffset = transform.position - (maxWorldPoint + minWorldPoint) / 2f;
        _camHalfViewZone = (maxWorldPoint - minWorldPoint) / 2f + Vector3.one * _camMinimapBuffer;
    }

    private void Awake() 
    {
        _camera = GetComponent<Camera>();

        _forwardDir = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        _mouseOnScreenBorder = -1;
        _mouseOnScreenCoroutine = null;
    }

    private void Update() 
    {
        if (GameManager.Instance.gameIsPaused) return;

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
            _Zoom(Input.mouseScrollDelta.y > 0f ? -1 : 1);
    }

    private void OnEnable() 
    {
        EventManager.AddListener("MoveCamera", _OnMoveCamera);
    }

    private void OnDisable() 
    {
        EventManager.RemoveListener("MoveCamera", _OnMoveCamera);
    }

    public void SetPosition(Vector3 pos)
    {
        transform.position = pos - _distance * transform.forward;
    }

    private void _TranslateCamera(int dir)
    {
        if (dir == 0 && transform.position.z - _camOffset.z + _camHalfViewZone.z <= _maxZ)
            transform.Translate(_forwardDir * Time.deltaTime * translationSpeed, Space.World);
        else if (dir == 1 && transform.position.x + _camHalfViewZone.x <= _maxX)
            transform.Translate(transform.right * Time.deltaTime * translationSpeed);
        else if (dir == 2 && transform.position.z - _camOffset.z - _camHalfViewZone.z >= _minZ)
            transform.Translate(-_forwardDir * Time.deltaTime * translationSpeed, Space.World);
        else if (dir == 3 && transform.position.x - _camHalfViewZone.x >= _minX)
            transform.Translate(-transform.right * Time.deltaTime * translationSpeed);

        if (autoAdaptAltitude)
            _FixAltitude();
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
        _camera.orthographicSize = Mathf.Clamp(_camera.orthographicSize, 8f, 100f);

        (Vector3 minWorldPoint, Vector3 maxWorldPoint) = Utils.GetCameraWorldBounds();
        _camOffset = transform.position - (maxWorldPoint + minWorldPoint) / 2f;
        _camHalfViewZone = (maxWorldPoint - minWorldPoint) / 2f + Vector3.one * _camMinimapBuffer;

        Vector3 pos = Utils.MiddleOfScreenPointToWorld();
        pos = _FixBounds(pos);
        SetPosition(pos);
    }

    private Vector3 _FixBounds(Vector3 pos)
    {
        if (pos.x - _camHalfViewZone.x < _minX)
        pos.x = _minX + _camHalfViewZone.x;
        if (pos.x + _camHalfViewZone.x > _maxX)
        pos.x = _maxX - _camHalfViewZone.x;
        if (pos.z - _camHalfViewZone.z < _minZ)
        pos.z = _minZ + _camHalfViewZone.z;
        if (pos.z + _camHalfViewZone.z > _maxZ)
        pos.z = _maxZ - _camHalfViewZone.z;
        return pos;
    }


    private void _FixAltitude()
    {
        _ray = new Ray(transform.position, Vector3.up * -1000f);
        if (Physics.Raycast(
                _ray,
                out _hit,
                1000f,
                Globals.TERRAIN_LAYER_MASK
            )) transform.position = _hit.point + Vector3.up * altitude;
    }

    private void _OnMoveCamera(object data)
    {
        Vector3 pos = _FixBounds( (Vector3)data );
        SetPosition(pos);

        if (autoAdaptAltitude)
            _FixAltitude();
    }
}
