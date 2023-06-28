using UnityEngine;

[CreateAssetMenu(fileName = "Global Parameters", menuName = "Scriptable Objects/Game Global Parameters", order = 10)]
public class GameGlobalParameters : GameParameters
{
    [Header("Units production")]
    [SerializeField] private int _baseEnergyProduction;

    [Header("Units consumption")]
    [SerializeField] private int _baseEnergyConsumption;

    public override string GetParametersName() => "Global";
}
