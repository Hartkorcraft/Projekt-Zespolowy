
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class WaveSystem : Node2D
{
    public int CurWaveNum { get; private set; } = 2;
    PackedScene spawnerScene = null!;
    Map map = null!;
    Node enemyPool = null!;

    bool waveStarting = false;

    public override void _EnterTree()
    {
        map = GetParent().GetNode<Map>("Map");
        enemyPool = ScenesPaths.GetScene<Node>(map, ScenesPaths.ENEMIE_POOL);
        spawnerScene = (PackedScene)ResourceLoader.Load(Imports.SPAWNER_SCENE);
    }

    public override void _Process(float delta)
    {
        if (map.MapGenerator.GeneratedMap is false) return;

        if (enemyPool.GetChildCount() > 0 || waveStarting is true) return;
        else StartWave();
    }


    public void StartWave()
    {
        waveStarting = true;
        CurWaveNum++;

        var tiles = map.GetRandomTiles(10 * CurWaveNum);

        foreach (var tile in tiles)
        {
            SpawnSpawnPoint(tile);
        }

        async void SpawnSpawnPoint(Tile tile)
        {
            var spawner = spawnerScene.Instance<SpawnPoint>();
            spawner.Position = new Vector2(tile.Pos.x, tile.Pos.y) * Map.TILE_SIZE;
            enemyPool.AddChild(spawner);

            await ToSignal(GetTree().CreateTimer(((float)Utils.rng.NextDouble() * 4)), "timeout");
            spawner.Spawn();
        }

        void RepairWalls()
        {
            Func<int, int, HealthSystem_Tile> createWallHealthSystem =
                (x, y) => new HealthSystem_Tile(15, 15, () => new Vector2(x, y) * Map.TILE_SIZE, map, new Tile(x, y, TileType.Rubble), map.MapGenerator.WallParticleScene, map.MapGenerator.WallParticleScene); //TODO hit particles 

            map.MapGenerator.CreateInsideWallsTime(map, createWallHealthSystem);
        }

        RepairWalls();
        GD.Print("Started wave ", CurWaveNum);
        waveStarting = false;
    }

}

