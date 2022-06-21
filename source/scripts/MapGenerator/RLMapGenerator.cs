using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static Utils;

public class RLMapGenerator : Node2D
{
    PackedScene bspBuildingScene = null!;
    PackedScene playerScene = null!;
    PackedScene testEnemyScene = null!;

    Node buildings = null!;
    public bool IsGenerating { get; private set; } = false;
    public bool GeneratedMap { get; private set; } = false;
    [Export] int tileSize = 8; // Rozmiar komórek mapy
    [Export] int num_rooms = 100; // Ilość pokoi
    [Export] int minSize = 20; // Minimalny rozmiar budynku
    [Export] int maxSize = 50; // Maksymalny rozmiar budynku
    [Export] float horizontalSpreed = 400; // Rozrzut horyzontalny budynków
    [Export] float verticalSpread = 400; // Rozrzut wertykalny budynków
    [Export] float deleteProcent = 0.3f; // Szansa na usunięcie budynków
    [Export] float solverBias = 1.5f; // Bias odpychania
    [Export] float ruinWalls = 0.7f;
    [Export] float ruinWallsTime = 0.6f;

    [Export] float dirtPatch = 0.001f;
    [Export] float trashAmmount = 0.01f;
    AStar2D? mstPath = null;

    public static event Action? OnMapGenerationFinished;
    public PackedScene WallParticleScene = null!;

    public override void _EnterTree()
    {
        bspBuildingScene = (PackedScene)ResourceLoader.Load(Imports.BSP_BUILDING_PATH);
        playerScene = (PackedScene)ResourceLoader.Load(Imports.PLAYER_SCENE);
        testEnemyScene = (PackedScene)ResourceLoader.Load(Imports.ENEMY_TEST_SCENE);

        buildings = GetNode("Buildings");
    }

    public override void _Process(float delta)
    {
        Update();
    }


    public async void InitMap(Map map)
    {
        if (IsGenerating is true) throw new Exception("Generating is true");
        IsGenerating = true;
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
                minRoomSize: (5, 5),
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

        IsGenerating = false;

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
        await ToSignal(GetTree(), "idle_frame");

        WallParticleScene = (PackedScene)ResourceLoader.Load(Imports.WALL_PARTICLE_PATH);

        Func<int, int, HealthSystem_Tile> createWallHealthSystem =
            (x, y) => new HealthSystem_Tile(5, 5, () => new Vector2(x, y) * Map.TILE_SIZE, map, new Tile(x, y, TileType.Rubble), WallParticleScene, WallParticleScene); //TODO hit particles 

        CreateInsideWalls(map, createWallHealthSystem);
        await ToSignal(GetTree().CreateTimer(0.1f), "timeout");

        CreateOutsideWalls(map, createWallHealthSystem);
        await ToSignal(GetTree().CreateTimer(0.1f), "timeout");

        CreateOpenings();
        await ToSignal(GetTree().CreateTimer(0.1f), "timeout");

        var player = playerScene.Instance<Player>();
        map.AddChild(player);

        GeneratedMap = true;
        OnMapGenerationFinished?.Invoke();

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

        void InitGround(Map map, Vector2 bottomLeft, Vector2 topRight)
        {
            for (int x = (int)bottomLeft.x - 1; x <= (int)topRight.x + 1; x++)
                for (int y = (int)bottomLeft.y - 1; y <= (int)topRight.y + 1; y++)
                {
                    map.SetCell(new Tile(x, y, TileType.Grass));

                    if ((float)rng.NextDouble() < dirtPatch)
                    {
                        map.SetCell(new Tile(x, y, TileType.Ground_Dark));
                    }
                    else if ((float)rng.NextDouble() < trashAmmount)
                    {
                        map.SetCell(new Tile(x, y, TileType.Trash));
                    }
                }
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

        void CreateOutsideWalls(Map map, Func<int, int, HealthSystem_Tile> createWallHealthSystem)
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

                            if (((float)rng.NextDouble()) < ruinWalls)
                            {
                                map.SetCell(new DestructableTile(((int)pos.x), ((int)pos.y), TileType.Rubble, TileType.Empty, createWallHealthSystem(((int)pos.x), ((int)pos.y))));
                            }
                        }
            }
        }

        void CreateOpenings()
        {
            foreach (RigidBodyBuilding building in buildings.GetChildren())
            {
                if (building.RoomTree is null) continue;

                var nodes = building.RoomTree.GetLowestNodes();

                foreach (var node in nodes)
                {
                    if (node.Child1 is null || node.Child2 is null) continue;
                    var pos1 = map.WorldToMap(building.Position + new Vector2(node.Child1.pos.x, node.Child1.pos.y) * tileSize) - new Vector2(building.offset.x, building.offset.y);
                    var pos2 = map.WorldToMap(building.Position + new Vector2(node.Child2.pos.x, node.Child2.pos.y) * tileSize) - new Vector2(building.offset.x, building.offset.y);

                    map.SetCell(new Tile(((int)pos1.x), ((int)pos1.y), TileType.Path));

                    // GD.Print(pos1, pos2);

                    // var path = map.PathFinding.FindPath(pos1.ToTuple(), pos2.ToTuple(), (pos) => false, (cell) => map.PathFinding.GetNeigbours(cell), (pos) => 0);

                    // foreach (var cell in path)
                    // {
                    //     var tile = map.GetTile(cell.GridPos);
                    //     if (tile is null) return;
                    //     if (map.CheckIfTileHasCollision(tile.TileType_Wall))
                    //     {
                    //         map.SetCell(new Tile(cell.GridPos.x, cell.GridPos.y, TileType.Path));
                    //     }
                    // }
                }


            }
        }

        void CreateEnemies(Map map, Vector2 bottomLeft, Vector2 topRight)
        {
            var enemyPool = ScenesPaths.GetScene<Node>(map, ScenesPaths.ENEMIE_POOL);

            for (int x = (int)bottomLeft.x; x <= (int)topRight.x; x++)
                for (int y = (int)bottomLeft.y; y <= (int)topRight.y; y++)
                {
                    if (x % 8 != 0 || y % 8 != 0) continue;
                    var enemy = testEnemyScene.Instance<Enemy>();
                    enemy.Position = new Vector2(x * Map.TILE_SIZE, y * Map.TILE_SIZE);
                    enemyPool.AddChild(enemy);
                }
        }

    }

    public void CreateInsideWalls(Map map, Func<int, int, HealthSystem_Tile> createWallHealthSystem)
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
                            if (((float)rng.NextDouble()) < ruinWalls)
                            {
                                map.SetCell(new DestructableTile(((int)pos.x), ((int)pos.y), TileType.Rubble, TileType.Empty, createWallHealthSystem(((int)pos.x), ((int)pos.y))));
                            }
                            // await ToSignal(GetTree().CreateTimer(0.001f), "timeout");

                        }
        }
    }

    public async void CreateInsideWallsTime(Map map, Func<int, int, HealthSystem_Tile> createWallHealthSystem)
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

                            if (((float)rng.NextDouble()) > ruinWallsTime && map.GetTile((x, y))?.TileType_Wall == TileType.Empty)
                            {
                                map.SetCell(new DestructableTile(((int)pos.x), ((int)pos.y), TileType.Rubble, TileType.Wall, createWallHealthSystem(((int)pos.x), ((int)pos.y))));
                                //map.SetCell(new DestructableTile(((int)pos.x), ((int)pos.y), TileType.Rubble, TileType.Empty, createWallHealthSystem(((int)pos.x), ((int)pos.y))));
                            }
                            await ToSignal(GetTree().CreateTimer(0.001f), "timeout");

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
            // foreach (int p in mstPath.GetPoints())
            // {
            //     foreach (var c in mstPath.GetPointConnections(p))
            //     {
            //         var pp = mstPath.GetPointPosition(p);
            //         var cp = mstPath.GetPointPosition(c);
            //         DrawLine(pp, new Vector2(cp[0], cp[1]), new Color(1, 1, 0), 1, true);
            //     }
            // }
        }

        // foreach (RigidBodyBuilding building in buildings.GetChildren())
        // {
        //     var buildingShape = building.collisionShape2D?.Shape as RectangleShape2D ?? throw new Exception("invalid shape");
        //     var buildingRect = new Rect2(building.Position - (building.BuildingSize.ToVec2() * tileSize) / 2, buildingShape.Extents * 2);
        //     var buildingPreviewColor = new Color(0, 0.1f, 0.8f, 0.5f);

        //     DrawRect(buildingRect, buildingPreviewColor);
        // }
    }


}
