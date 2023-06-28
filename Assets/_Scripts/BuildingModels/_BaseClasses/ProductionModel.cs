using System.Collections.Generic;
using UnityEngine;

public abstract class ProductionModel : ScriptableObject
{
    public abstract Dictionary<InGameResource, int> Productions { get; }

    public abstract void Produce();
}