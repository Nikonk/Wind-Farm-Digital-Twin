using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Building", menuName = "Scriptable Objects/Building", order = 2)]
public class BuildingData : UnitData
{
    [SerializeField]
    private List<ProductionModel> _productionModels;

    [SerializeField]
    private List<ConsumptionModel> _consumptionModels;

    [SerializeField]
    private List<TransferModel> _transferModels;
    
    public override bool IsHasProduction => _productionModels.Count > 0;
    public override bool IsHasConsumption => _consumptionModels.Count > 0;
    public override bool IsHasTransfer => _transferModels != null && _transferModels.Count > 0;
    public override List<ProductionModel> ProductionModels => _productionModels;
    public override List<ConsumptionModel> ConsumptionModels => _consumptionModels;
    public override List<TransferModel> TransferModels => _transferModels;
}
