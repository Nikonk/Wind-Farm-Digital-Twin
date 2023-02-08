using System.Collections.Generic;
using UnityEngine.AI;

public class Globals 
{
    public static int TERRAIN_LAYER_MASK = 1 << 8;

    public static BuildingData[] BUILDING_DATA;

    public static NavMeshSurface NAV_MESH_SURFACE;

    public static Dictionary<string, GameResource> GAME_RESOURCES =
        new Dictionary<string, GameResource>()
    {
        {"money", new GameResource("Money", 1000)},
        {"wind", new GameResource("Wind", 0)},
        {"energy", new GameResource("Energy", 0)}
    };

    public static List<UnitManager> SELECTED_UNITS = new List<UnitManager>();

    public static void UpdateNavMeshSurface()
    {
        NAV_MESH_SURFACE.UpdateNavMesh(NAV_MESH_SURFACE.navMeshData);
    }
}
