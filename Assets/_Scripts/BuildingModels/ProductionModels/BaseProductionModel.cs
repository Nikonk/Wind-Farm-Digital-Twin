using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ProductionModel", menuName = "Scriptable Objects/Production Model/Base", order = 1)]
public class BaseProductionModel : ProductionModel
{
    [SerializeField]
    private List<ResourceValue> _production = new List<ResourceValue>();

    public override Dictionary<InGameResource, int> Productions => Utils.ConvertResourceValueListToDictionary(_production);

    public override void Produce()
    {
        foreach (var resource in _production)
            Globals.GameResources[resource.Resource].ChangeAmount(resource.Amount);
    }
}