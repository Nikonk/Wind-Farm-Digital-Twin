using UnityEngine;
using UnityEngine.AI;

public class CharacterManager : UnitManager
{
    [SerializeField] private NavMeshAgent _agent;

    private Character _character;

    public override Unit Unit
    {
        get => _character;
        set 
        { 
            _character = value is Character character ? character : null; 
        }
    }

    public void MoveTo(Vector3 targetPosition)
    {
        _agent.destination = targetPosition;
    }
}
