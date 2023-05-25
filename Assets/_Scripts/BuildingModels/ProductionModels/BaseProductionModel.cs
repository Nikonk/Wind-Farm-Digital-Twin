using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ProductionModel", menuName = "Scriptable Objects/Production Model/Base", order = 1)]
public class BaseProductionModel : ProductionModel
{
    [SerializeField]
    private List<ResourceValue> _production = new List<ResourceValue>();
    [SerializeField]
    private float _consumingRate = 3f;

    public override void Produce()
    {
        foreach (var resource in _production)
            Globals.GAME_RESOURCES[resource.code].ChangeAmount(resource.amount);
    }

    public override float ProducingRate
    {
        get => _consumingRate;
    }
    public override Dictionary<InGameResource, int> Production
    {
        get => Utils.ConvertResourceValueListToDictionary(_production);
    }
}