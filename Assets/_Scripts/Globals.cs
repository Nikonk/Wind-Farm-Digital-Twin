using System.Collections.Generic;

public class Globals 
{
    public static int TERRAIN_LAYER_MASK = 1 << 8;
    
    public static BuildingData[] BUILDING_DATA = new BuildingData[]
    {
        new BuildingData("WindGenerator", 100, new Dictionary<string, int>()
        {
            { "money", 100 }
        }),
        new BuildingData("Inverter", 100, new Dictionary<string, int>()
        {
            { "money", 150 }
        })
    };

    public static Dictionary<string, GameResource> GAME_RESOURCES =
        new Dictionary<string, GameResource>()
    {
        {"money", new GameResource("Money", 1000)},
        {"wind", new GameResource("Wind", 0)},
        {"energy", new GameResource("Energy", 0)}
    };
}
