using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class Minimap : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Vector2 _terrainSize;
    private Vector2 _uiSize;
    
    private Vector2 _lastPointerPosition;
    private bool _dragging = false;

    private void Start()
    {
        Canvas.ForceUpdateCanvases();

        _uiSize = GetComponent<RectTransform>().sizeDelta;
        _lastPointerPosition = Input.mousePosition;
    }

    private void Update()
    {
        if (!_dragging) 
            return;

        Vector2 delta = (Vector2) Input.mousePosition - _lastPointerPosition;
        _lastPointerPosition = Input.mousePosition;

        if (delta.magnitude > Mathf.Epsilon)
        {
            Vector2 uiPosition =
                (new Vector2(Input.mousePosition.x, Input.mousePosition.y) /
                 GameManager.Instance.CanvasScaleFactor);
            Vector3 realPosition = new Vector3(
                uiPosition.x / _uiSize.x * _terrainSize.x,
                0f,
                uiPosition.y / _uiSize.y * _terrainSize.y
            );
            realPosition = Utils.ProjectOnTerrain(realPosition);
            EventManager.TriggerEvent("MoveCamera", realPosition);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _dragging = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _dragging = false;
    }
}
