using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class BuildingManager : UnitManager
{
    private Building _building;
    private int _nCollisions = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Terrain"))
            return;

        _nCollisions++;
        CheckPlacement();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Terrain"))
            return;

        _nCollisions--;
        CheckPlacement();
    }

    public override Unit Unit
    {
        get => _building;
        set => _building = value is Building building ? building : null;
    }

    public void Initialize(Building building)
    {
        Collider = GetComponent<BoxCollider>();
        _building = building;
    }

    public bool CheckPlacement()
    {
        if (_building == null || _building.IsFixed)
            return false;

        bool validPlacement = HasValidPlacement();

        if (!validPlacement)
            _building.SetMaterials(BuildingPlacement.INVALID);
        else
            _building.SetMaterials(BuildingPlacement.VALID);

        return validPlacement;
    }

    public bool HasValidPlacement()
    {
        if (_nCollisions > 0)
            return false;

        Vector3 position = transform.position;
        Vector3 colliderCenter = Collider.center;
        Vector3 halfColliderSize = Collider.size / 2f;
        float bottomHeight = colliderCenter.y - halfColliderSize.y + 0.5f;
        Vector3[] bottomCorners = new Vector3[]
        {
            new Vector3(colliderCenter.x - halfColliderSize.x, bottomHeight, colliderCenter.z - halfColliderSize.z),
            new Vector3(colliderCenter.x - halfColliderSize.x, bottomHeight, colliderCenter.z + halfColliderSize.z),
            new Vector3(colliderCenter.x + halfColliderSize.x, bottomHeight, colliderCenter.z - halfColliderSize.z),
            new Vector3(colliderCenter.x + halfColliderSize.x, bottomHeight, colliderCenter.z + halfColliderSize.z)
        };

        int invalidCornersCount = 0;

        foreach (Vector3 corner in bottomCorners)
            if (!Physics.Raycast(position + corner,
                                 Vector3.up * -1f,
                                 2f,
                                 Globals.TerrainLayerMask))
                invalidCornersCount++;

        return invalidCornersCount < 3;
    }

    protected override bool IsActive() => _building.IsFixed;
}
