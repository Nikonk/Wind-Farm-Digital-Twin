using UnityEngine;

public static class DataHandler
{
    public static void LoadGameData()
    {
        Globals.LoadBuildingData();

        GameParameters[] gameParametersList = Resources.LoadAll<GameParameters>("ScriptableObjects/Parameters");

        foreach (GameParameters parameters in gameParametersList)
            parameters.LoadFromFile("GameData");
    }

    public static void SaveGameData()
    {
        GameParameters[] gameParametersList = Resources.LoadAll<GameParameters>("ScriptableObjects/Parameters");

        foreach (GameParameters parameters in gameParametersList)
            parameters.SaveToFile("GameData");
    }
}
