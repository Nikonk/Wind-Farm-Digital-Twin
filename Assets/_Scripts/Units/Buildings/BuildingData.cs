using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Building", menuName = "Scriptable Objects/Building", order = 2)]
public class BuildingData : UnitData
{
    [SerializeField]
    private List<ProductionModel> _productionModels;

    [SerializeField]
    private List<ConsumptionModel> _consumptionModels;
    
    public override bool IsHasProduction { get => _productionModels.Count > 0; }
    public override bool IsHasConsumption { get => _consumptionModels.Count > 0; }
    public override List<ProductionModel> ProductionModels { get => _productionModels; }
    public override List<ConsumptionModel> ConsumptionModels { get => _consumptionModels; }
}
