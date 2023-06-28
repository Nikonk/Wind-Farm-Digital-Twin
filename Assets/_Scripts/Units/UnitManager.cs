using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class UnitManager : MonoBehaviour
{
    [SerializeField] private GameObject _selectionCircle;

    protected BoxCollider Collider;

    private void OnMouseDown()
    {
        if (IsActive())
            Select(true,
                   Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
    }

    public virtual Unit Unit { get; set; }

    public void Initialize(Unit unit)
    {
        Collider = GetComponent<BoxCollider>();
        Unit = unit;
    }

    private void _SelectUtil()
    {
        if (Globals.SelectedUnits.Contains(this)) 
            return;

        Globals.SelectedUnits.Add(this);
        _selectionCircle.SetActive(true);
        EventManager.TriggerEvent("SelectUnit", Unit);
    }

    public void Select() => Select(false, false);

    public void Select(bool singleClick, bool holdingShift)
    {
        if (!singleClick)
        {
            _SelectUtil();
            return;
        }

        if (!holdingShift)
        {
            List<UnitManager> selectedUnits = new List<UnitManager>(Globals.SelectedUnits);

            foreach (UnitManager um in selectedUnits)
                um.Deselect();

            _SelectUtil();
        }
        else
        {
            if (!Globals.SelectedUnits.Contains(this))
                _SelectUtil();
            else
                Deselect();
        }
    }

    public void Deselect()
    {
        if (!Globals.SelectedUnits.Contains(this)) 
            return;

        Globals.SelectedUnits.Remove(this);
        _selectionCircle.SetActive(false);
        EventManager.TriggerEvent("DeselectUnit", Unit);
    }

    protected virtual bool IsActive() => true;
}
