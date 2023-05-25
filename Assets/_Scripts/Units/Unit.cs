using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public class Unit
{
    protected UnitData _data;
    protected Transform _transform;
    protected string _uid;
    protected List<SkillManager> _skillManagers;

    public Unit(UnitData data)
    {
        _data = data;

        GameObject g = GameObject.Instantiate(data.Prefab) as GameObject;
        _transform = g.transform;

        _uid = System.Guid.NewGuid().ToString();

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
            Globals.GAME_RESOURCES[resource.code].ChangeAmount(-resource.amount);
        }
    }

    public bool CanBuy()
    {
        return _data.CanBuy();
    }

    public void TriggerSkill(int index, GameObject target = null)
    {
        _skillManagers[index].Trigger(target);
    }

    public UnitData Data { get => _data; }
    public string Code { get => _data.Code; }
    public Transform Transform { get => _transform; }
    public string Uid { get => _uid; }
    public List<SkillManager> SkillManagers { get => _skillManagers; }
}
