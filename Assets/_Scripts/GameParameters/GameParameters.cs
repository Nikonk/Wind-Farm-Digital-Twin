using System.Collections.Generic;
using UnityEngine;

public abstract class GameParameters : JSONSerializableScriptableObject
{
    [SerializeField] protected List<string> _fieldsToShowInGame;

    public List<string> FieldsToShowInGame => _fieldsToShowInGame;

    public abstract string GetParametersName();

    public bool IsShowsField(string fieldName)
    {
        if (_fieldsToShowInGame == null)
            return false;

        return _fieldsToShowInGame.Contains(fieldName);
    }

    public void ToggleShowField(string fieldName)
    {
        if (_fieldsToShowInGame == null)
            _fieldsToShowInGame = new List<string>();

        if (IsShowsField(fieldName))
            _fieldsToShowInGame.Remove(fieldName);
        else
            _fieldsToShowInGame.Add(fieldName);
    }
}
