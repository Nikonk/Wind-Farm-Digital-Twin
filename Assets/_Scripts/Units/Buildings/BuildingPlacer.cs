using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildingPlacer : MonoBehaviour
{
    private Building _placedBuilding = null;
    private Ray _ray;
    private RaycastHit _raycastHit;
    private Vector3 _lastPlacementPosition;

    private void Update()
    {
        if (GameManager.Instance.gameIsPaused) return;

        if (_placedBuilding != null)
        {
            if (Input.GetKeyUp(KeyCode.Escape) || Input.GetKeyUp(KeyCode.Mouse1))
            {
                _CancelPlacedBuilding();                
                EventManager.TriggerEvent("PlaceBuildingOff");
                return;
            }

            _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(
                _ray,
                out _raycastHit,
                1000f,
                Globals.TERRAIN_LAYER_MASK))
            {
                _placedBuilding.SetPosition(_raycastHit.point);
                if (_lastPlacementPosition != _raycastHit.point)
                {
                    _placedBuilding.CheckValidPlacement();
                    Dictionary<InGameResource, int> prod = _placedBuilding.ComputeProduction();
                    EventManager.TriggerEvent(
                        "UpdatePlacedBuildingProduction",
                        new object[] { prod, _raycastHit.point }
                    );
                }
                _lastPlacementPosition = _raycastHit.point;
            }

            if (_placedBuilding.HasValidPlacement &&
                Input.GetMouseButtonUp(0) &&
                !EventSystem.current.IsPointerOverGameObject())
            {
                _PlaceBuilding();
            }
        }
    }

    private void _PreparePlacedBuilding(int buildingDataIndex)
    {
        if (_placedBuilding != null && !_placedBuilding.IsFixed)
        {
            Destroy(_placedBuilding.Transform.gameObject);
        }
        Building building = new Building(
            Globals.BUILDING_DATA[buildingDataIndex]
        );
        _placedBuilding = building;
        _lastPlacementPosition = Vector3.zero;
        if (_placedBuilding.ComputeProduction() != null)
            EventManager.TriggerEvent("PlaceBuildingOn");
    }

    private void _PlaceBuilding()
    {
        _placedBuilding.ComputeProduction();
        _placedBuilding.ComputeConsumption();
        _placedBuilding.Place();
        if (_placedBuilding.CanBuy())
        {
            _PreparePlacedBuilding(_placedBuilding.DataIndex);
        }
        else
        {
            EventManager.TriggerEvent("PlaceBuildingOff");
            _placedBuilding = null;
        }
        EventManager.TriggerEvent("UpdateResourceTexts");
        EventManager.TriggerEvent("CheckBuildingButtons");

        Globals.UpdateNavMeshSurface();
    }

    private void _CancelPlacedBuilding()
    {
        Destroy(_placedBuilding.Transform.gameObject);
        _placedBuilding = null;
    }

    public void SelectPlacedBuilding(int buildingDataIndex)
    {
        _PreparePlacedBuilding(buildingDataIndex);
    }
}
