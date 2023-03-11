using System.Collections.Generic;
using UnityEngine.AI;

public enum InGameResource
{
    Money,
    Wind,
    Energy
}

public class Globals 
{
    public static int TERRAIN_LAYER_MASK = 1 << 8;
    public static int FLAT_TERRAIN_LAYER_MASK = 1 << 10;

    public static BuildingData[] BUILDING_DATA;

    public static NavMeshSurface NAV_MESH_SURFACE;

    public static Dictionary<InGameResource, GameResource> GAME_RESOURCES =
        new Dictionary<InGameResource, GameResource>()
    {
        {InGameResource.Money, new GameResource("Money", 1000)},
        {InGameResource.Wind, new GameResource("Wind", 0)},
        {InGameResource.Energy, new GameResource("Energy", 0)}
    };

    public static List<UnitManager> SELECTED_UNITS = new List<UnitManager>();

    public static void UpdateNavMeshSurface()
    {
        NAV_MESH_SURFACE.UpdateNavMesh(NAV_MESH_SURFACE.navMeshData);
    }
}
