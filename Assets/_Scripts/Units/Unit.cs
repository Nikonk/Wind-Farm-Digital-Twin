using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public class Unit
{
    protected UnitData _data;
    protected Transform _transform;
    protected int _currentHealth;
    protected string _uid;
    protected int _level;
    protected Dictionary<InGameResource, int> _production;
    protected List<SkillManager> _skillManagers;

    public Unit(UnitData data) : this(data, new List<ResourceValue>() { }) { }
    public Unit(UnitData data, List<ResourceValue> production)
    {
        _data = data;
        _currentHealth = data.HP;

        GameObject g = GameObject.Instantiate(data.Prefab) as GameObject;
        _transform = g.transform;

        _uid = System.Guid.NewGuid().ToString();
        _level = 1;
        _production = production.ToDictionary(rv => rv.code, rv => rv.amount);

        _skillManagers = new List<SkillManager>();
        SkillManager sm;
        foreach (SkillData skill in _data.Skills)
        {
            sm = g.AddComponent<SkillManager>();
            sm.Initialize(skill, g);
            _skillManagers.Add(sm);
        }

        _transform.GetComponent<UnitManager>().Initialize(this);
    }

    public void SetPosition(Vector3 position)
    {
        _transform.position = position;
    }

    public virtual void Place()
    {
        _transform.GetComponent<BoxCollider>().isTrigger = false;
        foreach (ResourceValue resource in _data.Cost)
        {
            Globals.GAME_RESOURCES[resource.code].AddAmount(-resource.amount);
        }

        if (_production.Count > 0)
            GameManager.Instance.producingUnits.Add(this);
    }

    public bool CanBuy()
    {
        return _data.CanBuy();
    }

    public void LevelUp()
    {
        _level += 1;
    }

    public void ProduceResources()
    {
        foreach (KeyValuePair<InGameResource, int> resource in _production)
            Globals.GAME_RESOURCES[resource.Key].AddAmount(resource.Value);
    }

    public void TriggerSkill(int index, GameObject target = null)
    {
        _skillManagers[index].Trigger(target);
    }

    public Dictionary<InGameResource, int> ComputeProduction()
    {
        if (_data.CanProduce.Length == 0) return null;

        GameGlobalParameters globalParams = GameManager.Instance.gameGlobalParameters;
        Vector3 pos = _transform.position;

        if (_data.CanProduce.Contains(InGameResource.Energy))
        {
            _production[InGameResource.Energy] = globalParams.baseEnergyProduction;
        }

        return _production;
    }

    public UnitData Data { get => _data; }
    public string Code { get => _data.Code; }
    public Transform Transform { get => _transform; }
    public int HP { get => _currentHealth; set => _currentHealth = value; }
    public int MaxHP { get => _data.HP; }
    public string Uid { get => _uid; }
    public int Level { get => _level; }
    public Dictionary<InGameResource, int> Production { get => _production; }
    public List<SkillManager> SkillManagers { get => _skillManagers; }
}
