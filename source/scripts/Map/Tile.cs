using Godot;
public class Tile
{
    public readonly (int x, int y) Pos;
    public readonly TileType TileType;

    public Tile(int posX, int posY, TileType tileType)
    {
        Pos = (posX, posY);
        TileType = tileType;
    }
}


public class DestructableTile : Tile, IHealthSystem
{
    public HealthSystem HealthSystem { get; private set; }

    public DestructableTile(int posX, int posY, TileType tileType, HealthSystem healthSystem) : base(posX, posY, tileType)
    {
        this.HealthSystem = healthSystem;
    }
}