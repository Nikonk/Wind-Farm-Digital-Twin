using System.Collections.Generic;

using UnityEngine;

public class Unit
{
    protected Transform Position;

    public Unit(UnitData data)
    {
        Data = data;

        GameObject unit = GameObject.Instantiate(data.Prefab) as GameObject;
        Position = unit.transform;

        Uid = System.Guid.NewGuid().ToString();

        SkillManagers = new List<SkillManager>();
        SkillManager sm;

        foreach (SkillData skill in Data.Skills)
        {
            sm = unit.AddComponent<SkillManager>();
            sm.Initialize(skill, unit);
            SkillManagers.Add(sm);
        }

        Position.GetComponent<UnitManager>().Initialize(this);
    }

    public UnitData Data { get; private set; }
    public string Uid { get; private set; }
    public List<SkillManager> SkillManagers { get; private set; }
    public string Code { get => Data.Code; }
    public Transform Transform { get => Position; }

    public void SetPosition(Vector3 position)
    {
        Position.position = position;
    }

    public virtual void Place()
    {
        Position.GetComponent<BoxCollider>().isTrigger = false;

        foreach (ResourceValue resource in Data.Cost)
            Globals.GameResources[resource.Resource].ChangeAmount(-resource.Amount);
    }

    public bool CanBuy() => Data.CanBuy();

    public void TriggerSkill(int index, GameObject target = null)
    {
        SkillManagers[index].Trigger(target);
    }
}
