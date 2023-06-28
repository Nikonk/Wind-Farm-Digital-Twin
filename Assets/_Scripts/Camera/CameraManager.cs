using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraManager : MonoBehaviour
{
    [SerializeField] private float _translationSpeed = 180;
    [SerializeField] private float _altitude = 40;
    [SerializeField] private float _zoomSpeed = 400;
    [SerializeField] private bool _autoAdaptAltitude = false;

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
    private Vector3 _offset;
    private Vector3 _halfViewZone;
    private float _minimapBuffer = 10f;


    private void Start() 
    {        
        _minX = 0;
        _maxX = 1000;
        _minZ = 0;
        _maxZ = 1000;

        (Vector3 minWorldPoint, Vector3 maxWorldPoint) = Utils.GetCameraWorldBounds();

        _offset = transform.position - (maxWorldPoint + minWorldPoint) / 2f;
        _halfViewZone = (maxWorldPoint - minWorldPoint) / 2f + Vector3.one * _minimapBuffer;
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
        if (GameManager.Instance.IsGamePaused) 
            return;

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
            Zoom(Input.mouseScrollDelta.y > 0f ? -1 : 1);
    }

    private void OnEnable() 
    {
        EventManager.AddListener("MoveCamera", OnMoveCamera);
    }

    private void OnDisable() 
    {
        EventManager.RemoveListener("MoveCamera", OnMoveCamera);
    }

    public void SetPosition(Vector3 pos)
    {
        transform.position = pos - _distance * transform.forward;
    }

    private void _TranslateCamera(int dir)
    {
        if (dir == 0 && transform.position.z - _offset.z + _halfViewZone.z <= _maxZ)
            transform.Translate(_forwardDir * Time.deltaTime * _translationSpeed, Space.World);
        else if (dir == 1 && transform.position.x + _halfViewZone.x <= _maxX)
            transform.Translate(transform.right * Time.deltaTime * _translationSpeed);
        else if (dir == 2 && transform.position.z - _offset.z - _halfViewZone.z >= _minZ)
            transform.Translate(-_forwardDir * Time.deltaTime * _translationSpeed, Space.World);
        else if (dir == 3 && transform.position.x - _halfViewZone.x >= _minX)
            transform.Translate(-transform.right * Time.deltaTime * _translationSpeed);

        if (_autoAdaptAltitude)
            FixAltitude();
    }

    public void OnMouseEnterScreenBorder(int borderIndex)
    {
        _mouseOnScreenCoroutine = StartCoroutine(SetMouseOnScreenBorder(borderIndex));
    }

    public void OnMouseExitScreenBorder()
    {
        StopCoroutine(_mouseOnScreenCoroutine);
        _mouseOnScreenBorder = -1;
    }

    private IEnumerator SetMouseOnScreenBorder(int borderIndex)
    {
        yield return new WaitForSeconds(0.3f);
        _mouseOnScreenBorder = borderIndex;
    }

    private void Zoom(int zoomDir)
    {
        _camera.orthographicSize += zoomDir * Time.deltaTime * _zoomSpeed;
        _camera.orthographicSize = Mathf.Clamp(_camera.orthographicSize, 8f, 100f);

        (Vector3 minWorldPoint, Vector3 maxWorldPoint) = Utils.GetCameraWorldBounds();

        _offset = transform.position - (maxWorldPoint + minWorldPoint) / 2f;
        _halfViewZone = (maxWorldPoint - minWorldPoint) / 2f + Vector3.one * _minimapBuffer;

        Vector3 centerPosition = Utils.MiddleOfScreenPointToWorld();
        centerPosition = FixBounds(centerPosition);
        SetPosition(centerPosition);
    }

    private Vector3 FixBounds(Vector3 pos)
    {
        if (pos.x - _halfViewZone.x < _minX)
            pos.x = _minX + _halfViewZone.x;

        if (pos.x + _halfViewZone.x > _maxX)
            pos.x = _maxX - _halfViewZone.x;

        if (pos.z - _halfViewZone.z < _minZ)
            pos.z = _minZ + _halfViewZone.z;

        if (pos.z + _halfViewZone.z > _maxZ)
            pos.z = _maxZ - _halfViewZone.z;

        return pos;
    }


    private void FixAltitude()
    {
        _ray = new Ray(transform.position, Vector3.up * -1000f);

        if (Physics.Raycast(
                _ray,
                out _hit,
                1000f,
                Globals.TerrainLayerMask
            )) 
            transform.position = _hit.point + Vector3.up * _altitude;
    }

    private void OnMoveCamera(object data)
    {
        Vector3 position = FixBounds( (Vector3)data );
        SetPosition(position);

        if (_autoAdaptAltitude)
            FixAltitude();
    }
}
