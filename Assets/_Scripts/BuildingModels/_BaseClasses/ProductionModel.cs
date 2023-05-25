using System.Collections.Generic;
using UnityEngine;

public abstract class ProductionModel : ScriptableObject
{
    public abstract void Produce();
    
    public abstract float ProducingRate { get; }
    public abstract Dictionary<InGameResource, int> Production { get; }
}