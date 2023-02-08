using UnityEngine;
using UnityEngine.AI;

public enum SkillType
{
    INSTANTIATE_CHARACTER
}

[CreateAssetMenu(fileName = "Skill", menuName = "Scriptable Objects/Skill", order = 4)]
public class SkillData : ScriptableObject
{
    [SerializeField]
    private string _code;
    [SerializeField]
    private string _skillName;
    [SerializeField]
    private string _description;
    [SerializeField]
    private SkillType _type;
    [SerializeField]
    private UnitData _unitReference;
    [SerializeField]
    private float _castTime;
    [SerializeField]
    private float _cooldown;
    [SerializeField]
    private Sprite _sprite;

    public void Trigger(GameObject source, GameObject target = null)
    {
        switch (_type)
        {
            case SkillType.INSTANTIATE_CHARACTER:
                BoxCollider coll = source.GetComponent<BoxCollider>();
                Vector3 instantiationPosition = new Vector3(
                    source.transform.position.x - coll.size.x * 0.7f,
                    source.transform.position.y,
                    source.transform.position.z - coll.size.z * 0.7f
                );
                Character c = new Character( (CharacterData)_unitReference );
                // c.Transform.position = instantiationPosition;
                c.Transform.GetComponent<NavMeshAgent>().Warp(instantiationPosition);
                c.Transform.GetComponent<CharacterManager>().Initialize(c);

                break;
            
            default:
                break;
        }
    }

    public float Cooldown { get => _cooldown; }
    public float CastTime { get => _castTime; }
    public string SkillName { get => _skillName; }
}
