using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SkillManager : MonoBehaviour
{
    [SerializeField] private SkillData _skill;

    private GameObject _source;
    private Button _button;
    private bool _ready;

    public SkillData Skill => _skill;

    public void Initialize(SkillData skill, GameObject source)
    {
        _skill = skill;
        _source = source;
    }

    public void Trigger(GameObject target = null)
    {
        if (!_ready)
            return;

        StartCoroutine(WrappedTrigger(target));
    }

    public void SetButton(Button button)
    {
        _button = button;
        SetReady(true);
    }

    private void SetReady(bool ready)
    {
        _ready = ready;
        if (_button != null) _button.interactable = ready;
    }

    private IEnumerator WrappedTrigger(GameObject target)
    {
        yield return new WaitForSeconds(_skill.CastTime);
        _skill.Trigger(_source, target);
        SetReady(false);

        yield return new WaitForSeconds(_skill.Cooldown);
        SetReady(true);
    }
}
