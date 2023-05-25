using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ConsumptionModel", menuName = "Scriptable Objects/Consumption Model/Base", order = 1)]
public class BaseConsumptionModel : ConsumptionModel
{
    [SerializeField]
    private List<ResourceValue> _consumption = new List<ResourceValue>();
    [SerializeField]
    private float _consumingRate = 3f;

    public override void Consume()
    {
        foreach (var resource in _consumption)
            Globals.GAME_RESOURCES[resource.code].ChangeAmount(-resource.amount);
    }

    public override float ConsumingRate
    {
        get => _consumingRate;
    }
    public override Dictionary<InGameResource, int> Consumption
    { 
        get => Utils.ConvertResourceValueListToDictionary(_consumption);
    }
}