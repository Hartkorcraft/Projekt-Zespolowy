using System.Collections.Generic;
using Godot;

public class NpcPathfinding
{
    Map map;
    public List<PathCell> Path = new List<PathCell>();

    int delayTime = 0; // Aby nie wyszukiwać ścieżki co klatkę
    int delayRate = 5;
    (int x, int y)? playerLastSeenPos = null;

    public void Update((int x, int y) gridPos, Player player)
    {
        if (delayTime % delayRate != 0) return;
        if (playerLastSeenPos == null) return;

        var playerGridPos = map.WorldToMap(player.Position);

        //Path = map.PathFinding.FindPath(gridPos, (playerGridPos.x, playerGridPos.y),)

    }

    // bool CheckForPathfindingBlocking((int x, int y) pos)
    // {
    //     var blocking = Map.GetTile(pos);
    // }

    // public NpcPathfinding(Map map)
    // {
    //     this.map = map;
    // }

}
