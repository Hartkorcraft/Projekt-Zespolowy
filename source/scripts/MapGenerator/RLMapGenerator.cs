using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static Utils;

public class RLMapGenerator : Node2D
{
    PackedScene bspBuildingScene = null!;
    PackedScene playerScene = null!;

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
        playerScene = (PackedScene)ResourceLoader.Load(Imports.PLAYER_SCENE);

        buildings = GetNode("Buildings");
    }

    public override void _Process(float delta)
    {
        Update();
    }


    public async void InitMap(Map map)
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
        Vector2 bottomLeft, topRight;

        InitArea(map, out bottomLeft, out topRight);
        InitGround(map, bottomLeft, topRight);

        await ToSignal(GetTree().CreateTimer(0.5f), "timeout");

        await CreateBuilding(map);

        Func<int, int, HealthSystem_Tile> createWallHealthSystem = (x, y) => new HealthSystem_Tile(5, 5, () => new Vector2(x, y) * Map.TILE_SIZE, map, new Tile(x, y, TileType.Rubble)); //TODO hit particles 

        await ToSignal(GetTree(), "idle_frame");

        CreateInsideWalls(map, createWallHealthSystem);
        await ToSignal(GetTree().CreateTimer(0.6f), "timeout");

        CreateOutSideWalls(map, createWallHealthSystem);
        await ToSignal(GetTree().CreateTimer(0.6f), "timeout");


        var player = playerScene.Instance<Player>();
        map.AddChild(player);

        QueueFree();

        void InitArea(Map map, out Vector2 bottomLeft, out Vector2 topRight)
        {
            var mapRect = new Rect2();
            foreach (RigidBodyBuilding building in buildings.GetChildren())
            {
                var buildingShape = building.collisionShape2D?.Shape as RectangleShape2D ?? throw new Exception("invalid shape");
                var buildingRect = new Rect2(building.Position - (building.BuildingSize.ToVec2() * tileSize) / 2, buildingShape.Extents * 2);
                mapRect = mapRect.Merge(buildingRect);
            }
            bottomLeft = map.WorldToMap(mapRect.Position);
            topRight = map.WorldToMap(mapRect.End);
        }

        static void InitGround(Map map, Vector2 bottomLeft, Vector2 topRight)
        {
            for (int x = (int)bottomLeft.x; x <= (int)topRight.x; x++)
                for (int y = (int)bottomLeft.y; y <= (int)topRight.y; y++)
                    map.SetCell(new Tile(x, y, TileType.Grass));
        }

        async System.Threading.Tasks.Task CreateBuilding(Map map)
        {
            foreach (RigidBodyBuilding building in buildings.GetChildren())
            {
                // Podłoga budynku
                if (building.RoomTree is null) continue;

                var floorTypes = new[] { TileType.Carpet_Red, TileType.Carpet_Green, TileType.Carpet_Blue, TileType.Carpet_Orange };
                TileType randomFloorType = floorTypes[Utils.rng.Next(floorTypes.Length)];

                for (int x = 0; x < building.RoomTree.size.x; x++)
                    for (int y = 0; y < building.RoomTree.size.y; y++)
                    {
                        var pos = map.WorldToMap(building.Position + new Vector2(x, y) * tileSize) - new Vector2(building.offset.x, building.offset.y);

                        map.SetCell(new Tile(((int)pos.x), ((int)pos.y), randomFloorType));
                    }

                await ToSignal(GetTree(), "idle_frame");
            }
        }

        void CreateInsideWalls(Map map, Func<int, int, HealthSystem_Tile> createWallHealthSystem)
        {
            foreach (RigidBodyBuilding building in buildings.GetChildren())
            {
                if (building.RoomTree is null) continue;
                var rooms = building.RoomTree.GetLowestNodes();

                // Ściany budynków 
                foreach (var room in rooms)
                    for (int x = 0; x < room.size.x; x++)
                        for (int y = 0; y < room.size.y; y++)
                            if (x == 0 || y == 0)
                            {
                                var pos = map.WorldToMap(building.Position + (room.pos.x + x, room.pos.y + y).ToVec2() * tileSize) - new Vector2(building.offset.x, building.offset.y);
                                map.SetCell(new DestructableTile(((int)pos.x), ((int)pos.y), TileType.Rubble, TileType.Wall, createWallHealthSystem(((int)pos.x), ((int)pos.y))));
                            }
            }
        }

        void CreateOutSideWalls(Map map, Func<int, int, HealthSystem_Tile> createWallHealthSystem)
        {
            foreach (RigidBodyBuilding building in buildings.GetChildren())
            {
                if (building.RoomTree is null) continue;

                for (int x = 0; x < building.RoomTree.size.x; x++)
                    for (int y = 0; y < building.RoomTree.size.y; y++)
                        if (x == 0 || y == 0 || x == building.RoomTree.size.x - 1 || y == building.RoomTree.size.y - 1)
                        {
                            var pos = map.WorldToMap(building.Position + new Vector2(x, y) * tileSize) - new Vector2(building.offset.x, building.offset.y);
                            map.SetCell(new DestructableTile(((int)pos.x), ((int)pos.y), TileType.Rubble, TileType.Wall, createWallHealthSystem(((int)pos.x), ((int)pos.y))));
                        }
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
        foreach (RigidBodyBuilding building in buildings.GetChildren())
        {
            var buildingShape = building.collisionShape2D?.Shape as RectangleShape2D ?? throw new Exception("invalid shape");
            var buildingRect = new Rect2(building.Position - (building.BuildingSize.ToVec2() * tileSize) / 2, buildingShape.Extents * 2);
            var buildingPreviewColor = new Color(0, 0.1f, 0.8f, 0.5f);

            DrawRect(buildingRect, buildingPreviewColor);
        }
    }


}
