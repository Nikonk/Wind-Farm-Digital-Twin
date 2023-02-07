using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Unit", menuName = "Scriptable Objects/Unit", order = 1)]
public class UnitData : ScriptableObject
{
    [SerializeField]
    private string _code;
    [SerializeField]
    private string _unitName;
    [SerializeField]
    private string _description;
    [SerializeField]
    private int _healthpoints;
    [SerializeField]
    private GameObject _prefab;
    [SerializeField]
    private List<ResourceValue> _cost;
    [SerializeField]
    private List<SkillData> _skills = new List<SkillData>();

    public bool CanBuy()
    {
        foreach (ResourceValue resource in _cost)
        {
            if (Globals.GAME_RESOURCES[resource.code].Amount < resource.amount)
            {
                return false;
            }
        }
        return true;
    }

    public string Code { get => _code; }
    public string UnitName { get => _unitName; }
    public string Description { get => _description; }
    public int HP { get => _healthpoints; }
    public GameObject Prefab { get => _prefab; }
    public List<ResourceValue> Cost { get => _cost; }
    public List<SkillData> Skills { get => _skills; }
}
