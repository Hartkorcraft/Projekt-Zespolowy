
using Godot;

public class MapGenerator
{

    public void Generate(Map map, (int x, int y) size)
    {
        var mapInfo = InitLayer(map, new MapInfo(size));

    }

    MapInfo InitLayer(Map map, MapInfo mapInfo)
    {
        var hitParticleScene = map.GetScene<Prefabs>(ScenesPaths.PREFABS).HitWallParticle;
        var deathParticleScene = map.GetScene<Prefabs>(ScenesPaths.PREFABS).DeathWallParticle;

        for (int y = 0; y < mapInfo.Size.y; y++)
            for (int x = 0; x < mapInfo.Size.x; x++)
            {
                if (x == 2)//(int)size.x / 2)
                {
                    var newTile = new Tile(x, y, TileType.Rubble, TileType.Empty);
                    var (_x, _y) = (x, y);

                    map.SetCell(
                        newTile: new DestructableTile(
                            posX: x,
                            posY: y,
                            floorType: TileType.Path,
                            wallType: TileType.Wall,
                            healthSystem: new HealthSystem_Tile(
                                health: 10,
                                maxHealth: 10,
                                getOriginPos: () => new Vector2(_x, _y) * Map.TILE_SIZE,
                                map: map,
                                newTile: newTile,
                                hitParticleScene: hitParticleScene,
                                deathParticleScene: deathParticleScene)));
                }
                else map.SetCell(new Tile(x, y, TileType.Grass, TileType.Empty));
            }
        GD.Print("Generated");
        return mapInfo;
    }
}

public class MapInfo
{
    public readonly (int x, int y) Size;

    public MapInfo((int x, int y) size)
    {
        Size = size;
    }
}

