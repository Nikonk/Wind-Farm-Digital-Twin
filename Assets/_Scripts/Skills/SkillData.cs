using UnityEngine;
using UnityEngine.AI;

public enum SkillType
{
    InstantiateCharacter
}

[CreateAssetMenu(fileName = "Skill", menuName = "Scriptable Objects/Skill", order = 4)]
public class SkillData : ScriptableObject
{
    [SerializeField] private string _code;
    [SerializeField] private string _skillName;
    [SerializeField] private string _description;
    [SerializeField] private SkillType _type;
    [SerializeField] private UnitData _unitReference;
    [SerializeField] private float _castTime;
    [SerializeField] private float _cooldown;
    [SerializeField] private Sprite _sprite;

    public float Cooldown => _cooldown;
    public float CastTime => _castTime;
    public string SkillName => _skillName;

    public void Trigger(GameObject source, GameObject target = null)
    {
        switch (_type)
        {
            case SkillType.InstantiateCharacter:
                InstantiateCharacter(source);
                break;

            default:
                break;
        }
    }

    private void InstantiateCharacter(GameObject source)
    {
        BoxCollider collider = source.GetComponent<BoxCollider>();
        Vector3 instantiationPosition = new Vector3(
            source.transform.position.x - collider.size.x * 0.7f,
            source.transform.position.y,
            source.transform.position.z - collider.size.z * 0.7f
        );
        Character c = new Character((CharacterData)_unitReference);
        c.Transform.GetComponent<NavMeshAgent>().Warp(instantiationPosition);
    }
}
