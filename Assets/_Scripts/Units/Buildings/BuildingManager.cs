using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class BuildingManager : UnitManager
{
    private Building _building;
    private int _nCollisions = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Terrain")
            return;

        _nCollisions++;
        CheckPlacement();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Terrain")
            return;

        _nCollisions--;
        CheckPlacement();
    }

    public override Unit Unit
    {
        get => _building;
        set 
        {
            _building = value is Building building ? building : null; 
        }
    }

    public void Initialize(Building building)
    {
        Collider = GetComponent<BoxCollider>();
        _building = building;
    }

    public bool CheckPlacement()
    {
        if (_building == null)
            return false;

        if (_building.IsFixed)
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

        Vector3 p = transform.position;
        Vector3 c = Collider.center;
        Vector3 e = Collider.size / 2f;
        float bottomHeight = c.y - e.y + 0.5f;
        Vector3[] bottomCorners = new Vector3[]
        {
            new Vector3(c.x - e.x, bottomHeight, c.z - e.z),
            new Vector3(c.x - e.x, bottomHeight, c.z + e.z),
            new Vector3(c.x + e.x, bottomHeight, c.z - e.z),
            new Vector3(c.x + e.x, bottomHeight, c.z + e.z)
        };

        int invalidCornersCount = 0;

        foreach (Vector3 corner in bottomCorners)
        {
            if (!Physics.Raycast(
                p + corner,
                Vector3.up * -1f,
                2f,
                Globals.TerrainLayerMask
            ))
                invalidCornersCount++;
        }

        return invalidCornersCount < 3;
    }

    protected override bool IsActive()
    {
        return _building.IsFixed;
    }
}
