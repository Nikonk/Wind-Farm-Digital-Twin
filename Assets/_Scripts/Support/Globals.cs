using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

[System.Serializable]
public enum InGameResource
{
    Money,
    Wind,
    Energy
}

public static class Globals
{
    public static List<UnitManager> SelectedUnits = new List<UnitManager>();

    private static Dictionary<InGameResource, GameResource> _gameResources =
        new Dictionary<InGameResource, GameResource>()
    {
        {InGameResource.Money, new GameResource(InGameResource.Money, 1000)},
        {InGameResource.Wind, new GameResource(InGameResource.Wind, 11)},
        {InGameResource.Energy, new GameResource(InGameResource.Wind, 0)}
    };
    private static ReadOnlyDictionary<InGameResource, GameResource> _readonlyDictionary =
        new ReadOnlyDictionary<InGameResource, GameResource>(_gameResources);

    private static int _terrainLayerMask = 1 << 8;
    private static int _flatTerrainLayerMask = 1 << 10;
    private static int _unitLayerMask = 1 << 12;

    public static ReadOnlyCollection<BuildingData> BuildingData { get; private set; }
    public static ReadOnlyDictionary<InGameResource, GameResource> GameResources => _readonlyDictionary;
    public static int TerrainLayerMask => _terrainLayerMask;
    public static int FlatTerrainLayerMask => _flatTerrainLayerMask;
    public static int UnitLayerMask => _unitLayerMask;

    public static void LoadBuildingData()
    {
        var buildingData = Resources.LoadAll<BuildingData>("ScriptableObjects/Units/Buildings") as BuildingData[];

        ReadOnlyCollection<BuildingData> readonlyBuildingData = new ReadOnlyCollection<BuildingData>(buildingData.ToList());

        Globals.BuildingData = readonlyBuildingData;
    }
}
