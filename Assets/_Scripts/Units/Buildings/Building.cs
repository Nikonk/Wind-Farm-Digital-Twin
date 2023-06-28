using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum BuildingPlacement
{
    VALID,
    INVALID,
    FIXED
}

public class Building : Unit
{
    private BuildingPlacement _placement;
    private List<Material> _materials;
    private BuildingManager _buildingManager;

    public Building(BuildingData data) : this(data, new List<ResourceValue>() { }, new List<ResourceValue>() { }) { }
    public Building(BuildingData data, List<ResourceValue> production, List<ResourceValue> consumption)
        : base(data)
    {
        _materials = new List<Material>();
        Renderer[] renderers = Position.GetComponentsInChildren<Renderer>();

        foreach (Renderer rend in renderers)
        {
            Material[] materials = rend.materials;

            foreach (Material material in materials)
                _materials.Add(new Material(material));
        }

        _buildingManager = Position.GetComponent<BuildingManager>();
        _placement = BuildingPlacement.VALID;

        SetMaterials();
    }

    public int DataIndex
    {
        get
        {
            for (int i = 0; i < Globals.BuildingData.Count; i++)
                if (Globals.BuildingData[i].Code == Data.Code)
                    return i;

            return -1;
        }
    }

    public bool IsFixed => _placement == BuildingPlacement.FIXED;
    public bool HasValidPlacement => _placement == BuildingPlacement.VALID;
    public GameObject GameObjectOfBuilding => _buildingManager.gameObject;

    public void SetMaterials() => SetMaterials(_placement);

    public void SetMaterials(BuildingPlacement placement)
    {
        List<Material> materials;

        if (placement == BuildingPlacement.VALID)
        {
            Material refMaterial = Resources.Load("Materials/Valid") as Material;
            materials = new List<Material>();

            for (int i = 0; i < _materials.Count; i++)
                materials.Add(refMaterial);
        }
        else if (placement == BuildingPlacement.INVALID)
        {
            Material refMaterial = Resources.Load("Materials/Invalid") as Material;
            materials = new List<Material>();

            for (int i = 0; i < _materials.Count; i++)
                materials.Add(refMaterial);
        }
        else if (placement == BuildingPlacement.FIXED)
        {
            materials = _materials;
        }
        else
        {
            return;
        }

        Renderer[] renderers = Position.GetComponentsInChildren<Renderer>();

        for (int i = 0; i < renderers.Length; i++)
            renderers[i].material = materials[i];
    }

    public override void Place()
    {
        _placement = BuildingPlacement.FIXED;
        SetMaterials();
        base.Place();

        if (Data.IsHasProduction)
            GameManager.Instance.AddProducingUnits(this);

        if (Data.IsHasConsumption)
            GameManager.Instance.AddConsumingUnits(this);

        if (Data.IsHasTransfer)
        {
            ConnectedToBuilding();
            GameManager.Instance.AddTransferUnits(this);
        }
    }

    public void CheckValidPlacement()
    {
        if (_placement == BuildingPlacement.FIXED)
            return;

        _placement = _buildingManager.CheckPlacement()
            ? BuildingPlacement.VALID
            : BuildingPlacement.INVALID;
    }

    public Dictionary<InGameResource, int> ComputeProduction()
    {
        var resultProduction = new Dictionary<InGameResource, int>();

        foreach (var productionModel in Data.ProductionModels)
            foreach (var production in productionModel.Productions)
                if (resultProduction.ContainsKey(production.Key))
                    resultProduction[production.Key] += production.Value;
                else
                    resultProduction.Add(production.Key, production.Value);

        return resultProduction;
    }

    public Dictionary<InGameResource, int> ComputeConsumption()
    {
        var resultConsumption = new Dictionary<InGameResource, int>();

        foreach (var consumptionModel in Data.ConsumptionModels)
            foreach (var consumption in consumptionModel.Consumptions)
                if (resultConsumption.ContainsKey(consumption.Key))
                    resultConsumption[consumption.Key] += consumption.Value;
                else
                    resultConsumption.Add(consumption.Key, consumption.Value);

        return resultConsumption;
    }

    public void ConnectedToBuilding()
    {
        Collider[] colliders = Physics.OverlapSphere(Transform.position, Globals.EnergyTransferArea, Globals.UnitLayerMask);
        var units = new List<Building>();

        BuildingManager buildingManager;

        foreach (Collider collider in colliders)
            if (collider.gameObject.TryGetComponent<BuildingManager>(out buildingManager))
            {
                var building = buildingManager.Unit as Building;
                units.Add(building);
            }

        if (units == null)
            return;

        foreach (var unit in units)
            foreach (var productionModel in (unit.Data as BuildingData).ProductionModels)
                foreach (var transferModel in (Data as BuildingData).TransferModels)
                    transferModel.AddTransferedProductions(productionModel.Produce);
    }
}
