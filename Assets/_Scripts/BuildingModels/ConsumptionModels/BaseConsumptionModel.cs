using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ConsumptionModel", menuName = "Scriptable Objects/Consumption Model/Base", order = 1)]
public class BaseConsumptionModel : ConsumptionModel
{
    [SerializeField]
    private List<ResourceValue> _consumption = new List<ResourceValue>();

    public override Dictionary<InGameResource, int> Consumptions => Utils.ConvertResourceValueListToDictionary(_consumption);

    public override void Consume()
    {
        foreach (var resource in _consumption)
            Globals.GameResources[resource.Resource].ChangeAmount(-resource.Amount);
    }
}