using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Unit", menuName = "Scriptable Objects/Unit", order = 1)]
public class UnitData : ScriptableObject
{
    [SerializeField] private string _code;
    [SerializeField] private string _unitName;
    [SerializeField] private string _description;
    [SerializeField] private GameObject _prefab;
    [SerializeField] private List<ResourceValue> _cost;
    [SerializeField] private List<SkillData> _skills = new List<SkillData>();

    public string Code => _code;
    public string UnitName => _unitName;
    public string Description => _description;
    public GameObject Prefab => _prefab;
    public List<ResourceValue> Cost => _cost;
    public List<SkillData> Skills => _skills;
    public virtual bool IsHasProduction => false;
    public virtual bool IsHasConsumption => false;
    public virtual bool IsHasTransfer => false;
    public virtual List<ProductionModel> ProductionModels => new List<ProductionModel>();
    public virtual List<ConsumptionModel> ConsumptionModels => new List<ConsumptionModel>();
    public virtual List<TransferModel> TransferModels => new List<TransferModel>();

    public bool CanBuy()
    {
        foreach (ResourceValue resource in _cost)
            if (Globals.GameResources[resource.Resource].Amount < resource.Amount)
                return false;

        return true;
    }
}
