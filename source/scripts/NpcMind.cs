using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class NpcMind
{
    float viewRange = 200;

    Map map;
    public List<PathCell> Path = new List<PathCell>();

    Vector2? playerLastSeenPos = null;

    public Vector2 GetDirToPlayer(Entity npc, Player player)
    {
        //raycast aby sprawdzić czy widzi
        var spaceState = npc.GetWorld2d().DirectSpaceState;
        var result = spaceState.IntersectRay(
            from: npc.GlobalPosition,
            to: player.GlobalPosition,
            exclude: new Godot.Collections.Array { npc },
            collisionLayer: 0b11);

        if (result.Count > 0)
        {
            var hit = result["collider"];
            var distanceToPlayer = npc.GlobalPosition.DistanceTo(player.GlobalPosition);

            if (hit is Player && distanceToPlayer <= viewRange)
            {
                playerLastSeenPos = player.GlobalPosition;

                var dir = player.GlobalPosition - npc.GlobalPosition;
                dir.Normalized();
                return dir;
            }
        }
        if (playerLastSeenPos is null) return Vector2.Zero;

        Path = FindPath(npc.GlobalPosition, playerLastSeenPos.Value);
        if (Path.Any())
        {
            var pathPos = (Map.TILE_SIZE / 2 + Path[0].GridPos.x * Map.TILE_SIZE, Map.TILE_SIZE / 2 + Path[0].GridPos.y * Map.TILE_SIZE).ToVec2();
            var pathDir = pathPos - npc.GlobalPosition;
            pathDir.Normalized();
            return pathDir;
        }

        return Vector2.Zero;
    }

    List<PathCell> FindPath(Vector2 npcPos, Vector2 destinationPos)
    {
        var npcGridPos = map.ToMapPos(npcPos);
        var playerGridPos = map.ToMapPos(destinationPos);

        return map.PathFinding.FindPath(
            startPos: npcGridPos,
            endPos: (playerGridPos.x, playerGridPos.y),
            checkBlocking: CheckForPathfindingBlocking,
            getNeigbours: map.PathFinding.GetNeigboursDiagonal,
            getTileCost: (pos) => 0,
            checkLast: true
            );
    }

    bool CheckForPathfindingBlocking((int x, int y) pos)
    {
        var tile = Map.GetTile(pos) ?? throw new Exception("no tile");

        var floorTileType = tile.TileType_Floor;
        var wallTileType = tile.TileType_Wall;

        var blocking = map.CheckIfTileHasCollision(floorTileType) ? true :
                       map.CheckIfTileHasCollision(wallTileType) ? true : false;

        return blocking;
    }

    public NpcMind(Map map)
    {
        this.map = map;
    }
}
