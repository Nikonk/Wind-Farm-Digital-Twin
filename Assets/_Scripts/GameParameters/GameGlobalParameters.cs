using UnityEngine;

[CreateAssetMenu(fileName = "Global Parameters", menuName = "Scriptable Objects/Game Global Parameters", order = 10)]
public class GameGlobalParameters : GameParameters
{
    [Header("Day and Night")]
    public bool enableDayAndNightCycle;
    public float dayLengthInSeconds;
    public float dayInitialRatio;

    [Header("Units production")]
    public int baseEnergyProduction;

    [Header("Units consumption")]
    public int baseEnergyConsumption;

    public override string GetParametersName() => "Global";
}
