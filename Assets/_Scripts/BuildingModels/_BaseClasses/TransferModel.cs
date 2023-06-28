using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class TransferModel : ScriptableObject
{
    protected List<Action> TransferedProductions = new List<Action>();
    protected List<Action> TransferedConsumptions = new List<Action>();

    public abstract Dictionary<InGameResource, int> TransferResources { get; }

    public abstract void Transfer();

    public void AddTransferedProductions(Action production)
    {
        TransferedProductions.Add(production);
    }

    public void AddTransferedProductions(List<ProductionModel> productions)
    {
        foreach (var production in productions)
            TransferedProductions.Add(production.Produce);
    }

    public void AddTransferedConsumptions(Action consumption)
    {
        TransferedConsumptions.Add(consumption);
    }

    public void AddTransferedConsumptions(List<ConsumptionModel> consumptions)
    {
        foreach (var consumption in consumptions)
            TransferedProductions.Add(consumption.Consume);
    }
}