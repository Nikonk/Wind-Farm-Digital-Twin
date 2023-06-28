using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TransferModel", menuName = "Scriptable Objects/Transfer Model/Base", order = 1)]
public class BaseTransferModel : TransferModel
{
    [SerializeField]
    private List<ResourceValue> _transferResources = new List<ResourceValue>();
    
    public override Dictionary<InGameResource, int> TransferResources => Utils.ConvertResourceValueListToDictionary(_transferResources);

    public override void Transfer()
    {
        foreach (Action transferedProduction in TransferedProductions)
            transferedProduction();
    }
}