using System;
using UnityEngine;

[Serializable]
public class ResourceValue
{
    [SerializeField] private InGameResource _resource;
    [SerializeField] private int _amount;

    public ResourceValue(InGameResource resource, int amount)
    {
        _resource = resource;
        _amount = amount;
    }

    public InGameResource Resource => _resource;
    public int Amount => _amount;
}
