using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class UnitManager : MonoBehaviour
{
    [SerializeField]
    private GameObject selectionCircle;

    protected BoxCollider _collider;
    public virtual Unit Unit { get; set; }

    private void OnMouseDown()
    {
        if (IsActive())
            Select(
                true,
                Input.GetKey(KeyCode.LeftShift) ||
                Input.GetKey(KeyCode.RightShift)
            );
    }

    public void Initialize(Unit unit)
    {
        _collider = GetComponent<BoxCollider>();
        Unit = unit;
    }

    private void _SelectUnit()
    {
        if (Globals.SELECTED_UNITS.Contains(this)) return;
        Globals.SELECTED_UNITS.Add(this);
        selectionCircle.SetActive(true);
        EventManager.TriggerTypedEvent("SelectUnit", new CustomEventData(Unit));
    }

    public void Select() { Select(false, false); }
    public void Select(bool singleClick, bool holdingShift)
    {
        if (!singleClick)
        {
            _SelectUnit();
            return;
        }

        if (!holdingShift)
        {
            List<UnitManager> selectedUnits = new List<UnitManager>(Globals.SELECTED_UNITS);
            foreach (UnitManager um in selectedUnits)
                um.Deselect();
            _SelectUnit();
        }
        else
        {
            if (!Globals.SELECTED_UNITS.Contains(this))
                _SelectUnit();
            else
                Deselect();
        }
    }

    public void Deselect()
    {
        if (!Globals.SELECTED_UNITS.Contains(this)) return;
        Globals.SELECTED_UNITS.Remove(this);
        selectionCircle.SetActive(false);
        EventManager.TriggerTypedEvent("DeselectUnit", new CustomEventData(Unit));
    }

    protected virtual bool IsActive()
    {
        return true;
    }
}
