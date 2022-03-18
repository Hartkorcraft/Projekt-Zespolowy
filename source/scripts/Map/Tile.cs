using Godot;
public class Tile
{
    public readonly (int x, int y) Pos;
    public readonly TileType TileType_Floor;
    public readonly TileType TileType_Wall;

    public Tile(int posX, int posY, TileType floorType, TileType wallType = TileType.Empty)
    {
        Pos = (posX, posY);
        TileType_Floor = floorType;
        TileType_Wall = wallType;
    }
}


public class DestructableTile : Tile, IHealthSystem
{
    public HealthSystem HealthSystem { get; private set; }

    public DestructableTile(int posX, int posY, TileType floorType, TileType wallType, HealthSystem healthSystem) : base(posX, posY, floorType, wallType)
    {
        this.HealthSystem = healthSystem;
    }
}