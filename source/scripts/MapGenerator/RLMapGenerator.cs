using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static Utils;

public class RLMapGenerator : Node2D
{
    PackedScene bspBuildingScene = null!;

    Node buildings = null!;
    bool generating = false;

    [Export] int tileSize = 8; // Rozmiar komórek mapy
    [Export] int num_rooms = 100; // Ilość pokoi
    [Export] int minSize = 20; // Minimalny rozmiar budynku
    [Export] int maxSize = 50; // Maksymalny rozmiar budynku
    [Export] float horizontalSpreed = 400; // Rozrzut horyzontalny budynków
    [Export] float verticalSpread = 400; // Rozrzut wertykalny budynków
    [Export] float deleteProcent = 0.3f; // Szansa na usunięcie budynków
    [Export] float solverBias = 1.5f; // Bias odpychania

    AStar2D? mstPath = null;

    public override void _EnterTree()
    {
        bspBuildingScene = (PackedScene)ResourceLoader.Load(Imports.BSP_BUILDING_PATH);
        buildings = GetNode("Buildings");
    }

    public override void _Process(float delta)
    {
        Update();
    }


    public async void GenerateMap(Map map)
    {
        if (generating is true) throw new Exception("Generating is true");
        generating = true;
        mstPath = null;

        var buildingsNodes = buildings.GetChildren();

        if (buildingsNodes.Count > 0)
            foreach (RigidBodyBuilding building in buildingsNodes)
                building.QueueFree();

        for (int i = 0; i < num_rooms; i++)
        {
            var pos = Vector2.Zero;
            pos = new Vector2(
                Utils.RandomFloat(-horizontalSpreed, horizontalSpreed),
                Utils.RandomFloat(-verticalSpread, verticalSpread));

            var room = (RigidBodyBuilding)bspBuildingScene.Instance();
            var width = minSize + Utils.rng.Next() % (maxSize - minSize);
            var height = minSize + Utils.rng.Next() % (maxSize - minSize);

            room.GenerateBuilding(
                pos: pos,
                buildingSize: (width, height),
                minRoomSize: (4, 4),
                solverBias: solverBias,
                tileSize: tileSize
                );

            buildings.AddChild(room);
        }
        await ToSignal(GetTree().CreateTimer(1.1f), "timeout");

        var roomPositions = new List<Vector2>();

        foreach (RigidBodyBuilding room in buildings.GetChildren())
            if (((float)Utils.rng.NextDouble()) < deleteProcent) room.QueueFree();
            else
            {
                room.Mode = RigidBody2D.ModeEnum.Static;
                roomPositions.Add(room.Position);
            }

        await ToSignal(GetTree(), "idle_frame");

        mstPath = FindMinimumSpanningTree(roomPositions);

        generating = false;

        await ToSignal(GetTree(), "idle_frame");
        MakeMap(map);

    }

    public async void MakeMap(Map map)
    {

        // Powierzchnia Mapy
        var mapRect = new Rect2();
        foreach (RigidBodyBuilding room in buildings.GetChildren())
        {
            var roomShape = room.GetNode<CollisionShape2D>("CollisionShape2D").Shape as RectangleShape2D ?? throw new Exception("invalid shape");
            var roomRect = new Rect2(room.Position - (room.BuildingSize.ToVec2() * tileSize) / 2, roomShape.Extents * 2);
            mapRect = mapRect.Merge(roomRect);
        }
        var topLeft = map.WorldToMap(mapRect.Position);
        var bottomRight = map.WorldToMap(mapRect.End);

        // Ziemia 
        for (int x = (int)topLeft.x; x <= (int)bottomRight.x; x++)
            for (int y = (int)topLeft.y; y <= (int)bottomRight.y; y++)
                map.SetCell(new Tile(x, y, TileType.Grass));

        await ToSignal(GetTree().CreateTimer(0.5f), "timeout");

        foreach (RigidBodyBuilding building in buildings.GetChildren())
        {
            // Podłoga budynku
            if (building.RoomTree is null) continue;
            for (int x = 0; x < building.RoomTree.size.x; x++)
                for (int y = 0; y < building.RoomTree.size.y; y++)
                {
                    var pos = map.WorldToMap(building.Position + new Vector2(x, y) * tileSize) - new Vector2(building.offset.x, building.offset.y);
                    map.SetCell(new Tile(x, y, TileType.Red_Carpet));
                }

            await ToSignal(GetTree(), "idle_frame");
        }

        Func<int, int, HealthSystem_Tile> createWallHealthSystem = (x, y) => new HealthSystem_Tile(5, 5, () => new Vector2(x, y) * Map.TILE_SIZE, map, new Tile(x, y, TileType.Rubble));

        foreach (RigidBodyBuilding building in buildings.GetChildren())
        {
            if (building.RoomTree is null) continue;
            var rooms = building.RoomTree.GetLowestNodes();
            await ToSignal(GetTree(), "idle_frame");

            // Ściany budynków 
            foreach (var room in rooms)
                for (int x = 0; x < room.size.x; x++)
                    for (int y = 0; y < room.size.y; y++)
                        if (x == 0 || y == 0)
                        {
                            var pos = map.WorldToMap(building.Position + (room.pos.x + x, room.pos.y + y).ToVec2() * tileSize) - new Vector2(building.offset.x, building.offset.y);
                            map.SetCell(new DestructableTile(x, y, TileType.Rubble, TileType.Wall, createWallHealthSystem(x, y)));
                        }
        }

        foreach (RigidBodyBuilding building in buildings.GetChildren())
        {
            if (building.RoomTree is null) continue;
            for (int x = 0; x < building.RoomTree.size.x; x++)
                for (int y = 0; y < building.RoomTree.size.y; y++)
                    if (x == 0 || y == 0 || x == building.RoomTree.size.x - 1 || y == building.RoomTree.size.y - 1)
                    {
                        var pos = map.WorldToMap(building.Position + new Vector2(x, y) * tileSize) - new Vector2(building.offset.x, building.offset.y);
                        map.SetCell(new DestructableTile(x, y, TileType.Rubble, TileType.Wall, createWallHealthSystem(x, y)));
                    }
        }

    }

    public AStar2D FindMinimumSpanningTree(List<Vector2> roomPositions)
    {
        // Algorytm Prima
        var path = new AStar2D();
        path.AddPoint(path.GetAvailablePointId(), roomPositions.Last());
        roomPositions.RemoveAt(roomPositions.Count - 1);

        // Powtarzanie dopóki są punkty 
        while (roomPositions.Any())
        {
            var minDistance = float.MaxValue;
            var minP = Vector2.Zero;
            var curP = Vector2.Zero;
            foreach (int p1Id in path.GetPoints())
            {
                var p1 = path.GetPointPosition(p1Id);
                foreach (var p2 in roomPositions)
                {
                    if (p1.DistanceTo(p2) < minDistance)
                    {
                        minDistance = p1.DistanceTo(p2);
                        minP = p2;
                        curP = p1;
                    }
                }
            }
            var n = path.GetAvailablePointId();
            path.AddPoint(n, minP);
            path.ConnectPoints(path.GetClosestPoint(curP), n);
            roomPositions.Remove(minP);
        }
        return path;
    }

    public override void _Draw()
    {
        if (mstPath is not null)
        {
            foreach (int p in mstPath.GetPoints())
            {
                foreach (var c in mstPath.GetPointConnections(p))
                {
                    var pp = mstPath.GetPointPosition(p);
                    var cp = mstPath.GetPointPosition(c);
                    DrawLine(pp, new Vector2(cp[0], cp[1]), new Color(1, 1, 0), 1, true);
                }
            }
        }
    }


}
