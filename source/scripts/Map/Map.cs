using Godot;
using System;

public class Map : TileMap
{
    public const int TILE_SIZE = 8;

    [Export] bool initMap = true;
    [Export] int sizeX = 50;
    [Export] int sizeY = 50;

    Tile[,] mapTiles = null!;

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
                mapTiles[x, y] = new Tile(this, x, y, TileType.Grass);
            }
    }

    public static (int x, int y) ToMapPos(Vector2 pos)
        => ((int)pos.x / TILE_SIZE, (int)pos.y / TILE_SIZE);

}

public class Tile
{
    public readonly (int x, int y) Pos;


    public Tile(TileMap map, int posX, int posY, TileType tileType)
    {
        map.SetCell(posX, posY, ((int)tileType));
    }
}
