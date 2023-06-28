using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UnitsSelection : MonoBehaviour
{
    public UIManager uiManager;

    private bool _isDraggingMouseBox = false;
    private Vector3 _dragStartPosition;
    private Ray _ray;
    private RaycastHit _raycastHit;
    private Dictionary<int, List<UnitManager>> _selectionGroups = new Dictionary<int, List<UnitManager>>();

    private void Update()
    {
        if (GameManager.Instance.IsGamePaused)
            return;

        if (EventSystem.current.IsPointerOverGameObject())
            return;

        if (Input.GetMouseButtonDown(0))    
        {
            _isDraggingMouseBox = true;
            _dragStartPosition = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0))
            _isDraggingMouseBox = false;
        
        if (_isDraggingMouseBox && _dragStartPosition != Input.mousePosition)
            SelectUnitsInDraggingBox();

        if (Globals.SelectedUnits.Count > 0)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                DeselectAllUnits();

            if (Input.GetMouseButtonDown(0))
            {
                _ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(
                    _ray,
                    out _raycastHit,
                    1000f
                ))
                    if (_raycastHit.transform.tag == "Terrain")
                        DeselectAllUnits();
            }
        }

        if (int.TryParse(Input.inputString, out int alphaKey))
        {
            if (
                Input.GetKey(KeyCode.LeftControl) ||
                Input.GetKey(KeyCode.RightControl) ||
                Input.GetKey(KeyCode.LeftApple) ||
                Input.GetKey(KeyCode.RightApple)
            )
                _CreateSelectionGroup(alphaKey);
            else
                ReselectGroup(alphaKey);
        }
    }

    private void OnGUI() {
        if (_isDraggingMouseBox)    
        {
            Rect rect = Utils.GetScreenRect(_dragStartPosition, Input.mousePosition);
            Utils.DrawScreenRect(rect, new Color(.5f, 1f, .4f, .2f));
            Utils.DrawScreenRectBorder(rect, 1, new Color(.5f, 1f, .4f));
        }
    }

    public void SelectUnitsGroup(int groupIndex)
    {
        ReselectGroup(groupIndex);
    }

    private void SelectUnitsInDraggingBox()
    {
        Bounds selectionBounds = Utils.GetViewportBounds(
            Camera.main,
            _dragStartPosition,
            Input.mousePosition
        );
        GameObject[] selectableUnits = GameObject.FindGameObjectsWithTag("Unit");
        bool inBounds;

        foreach (GameObject unit in selectableUnits)
        {
            inBounds = selectionBounds.Contains(
                Camera.main.WorldToViewportPoint(unit.transform.position)
            );

            if (inBounds)
                unit.GetComponent<UnitManager>().Select();
            else
                unit.GetComponent<UnitManager>().Deselect();
        }
    }

    private void DeselectAllUnits()
    {
        List<UnitManager> selectedUnits = new List<UnitManager>(Globals.SelectedUnits);

        foreach (UnitManager um in selectedUnits)
            um.Deselect();
    }

    private void _CreateSelectionGroup(int groupIndex)
    {
        if (Globals.SelectedUnits.Count == 0)
        {
            if (_selectionGroups.ContainsKey(groupIndex))
                RemoveSelectionGroup(groupIndex);

            return;
        }

        List<UnitManager> groupUnits = new List<UnitManager>(Globals.SelectedUnits);
        _selectionGroups[groupIndex] = groupUnits;

        uiManager.ToggleSelectionGroupButton(groupIndex, true);
    }

    private void RemoveSelectionGroup(int groupIndex)
    {
        _selectionGroups.Remove(groupIndex);
        uiManager.ToggleSelectionGroupButton(groupIndex, false);
    }

    private void ReselectGroup(int groupIndex)
    {
        if (!_selectionGroups.ContainsKey(groupIndex))
            return;

        DeselectAllUnits();

        foreach (UnitManager um in _selectionGroups[groupIndex])
            um.Select();
    }
}
