using System.Collections.Generic;
using UnityEngine;

public abstract class ConsumptionModel : ScriptableObject
{
    public abstract void Consume();

    public abstract float ConsumingRate { get; }
    public abstract Dictionary<InGameResource, int> Consumption { get; }
}