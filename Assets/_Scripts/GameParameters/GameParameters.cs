using System.Collections.Generic;
using UnityEngine;

public abstract class GameParameters : JSONSerializableScriptableObject
{
    [SerializeField] protected List<string> FieldsToShowInGame;

    public List<string> FieldsToShow => FieldsToShowInGame;

    public abstract string GetParametersName();

    public bool IsShowsField(string fieldName)
    {
        if (FieldsToShowInGame == null)
            return false;

        return FieldsToShowInGame.Contains(fieldName);
    }

    public void ToggleShowField(string fieldName)
    {
        if (FieldsToShowInGame == null)
            FieldsToShowInGame = new List<string>();

        if (IsShowsField(fieldName))
            FieldsToShowInGame.Remove(fieldName);
        else
            FieldsToShowInGame.Add(fieldName);
    }
}
