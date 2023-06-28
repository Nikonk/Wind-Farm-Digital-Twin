using System.Collections.Generic;
using UnityEngine;

public abstract class ConsumptionModel : ScriptableObject
{
    public abstract Dictionary<InGameResource, int> Consumptions { get; }

    public abstract void Consume();
}