using Godot;
using System;

public class Map : TileMap
{
    public const int TILE_SIZE = 8;

    [Export] bool initMap = true;
    [Export] static int sizeX = 50;
    [Export] static int sizeY = 50;

    static Tile[,] mapTiles = null!;

    public override void _EnterTree()
    {
        if (initMap) InitMap((sizeX, sizeY));
    }

    public void InitMap((int x, int y) size)
    {
        this.Clear();

        mapTiles = new Tile[size.x, size.y];

        for (int y = 0; y < size.y; y++)
            for (int x = 0; x < size.x; x++)
            {
                if (x == (int)size.x / 2)
                {
                    var newTile = new Tile(x, y, TileType.Path);
                    SetCell(this, new DestructableTile(x, y, TileType.Wall, new HealthSystem(10, 10, () => SetCell(this, newTile))));
                    GD.Print(newTile.Pos);
                }
                else SetCell(this, new Tile(x, y, TileType.Grass));

            }
    }

    public static bool OnMap((int x, int y) pos)
        => pos.x >= 0 && pos.x < sizeX && pos.y >= 0 && pos.y < sizeY;

    public static void SetCell(Map map, Tile newTile)
    {
        if (OnMap(newTile.Pos) is false) throw new Exception("Out of bounds");
        mapTiles[newTile.Pos.x, newTile.Pos.y] = newTile;
        map.SetCell(newTile.Pos.x, newTile.Pos.y, (int)newTile.TileType);
    }

    public static Tile GetTile((int x, int y) pos)
    {
        if (OnMap(pos) is false) throw new Exception("Out of bounds");
        return mapTiles[pos.x, pos.y];
    }

    public static (int x, int y) ToMapPos(Vector2 pos)
        => ((int)pos.x / TILE_SIZE, (int)pos.y / TILE_SIZE);

}
