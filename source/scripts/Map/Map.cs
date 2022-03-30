using Godot;
using System;

public class Map : Node
{
    public const int TILE_SIZE = 8;
    public static readonly Vector2 HALF_TILE = new Vector2(4, 4);

    public PathFinding PathFinding { get; private set; } = null!;
    public RLMapGenerator MapGenerator { get; private set; } = null!;

    bool initMap = true;

    static int sizeX = 50;
    static int sizeY = 50;

    // komórki mapy
    static Tile[,] mapTiles = null!;
    // tilemapy 
    Tiles tilesFloor = null!;
    Tiles tilesWalls = null!;
    TileSet tileSet = null!;



    // cordy świata na mampy
    public Vector2 WorldToMap(Vector2 pos)
        => tilesFloor.WorldToMap(pos);

    public (int x, int y) ToMapPos(Vector2 pos)
        => ((int)WorldToMap(pos).x, ((int)WorldToMap(pos).y));


    public override void _Input(InputEvent e)
    {
        if (e.IsActionPressed(InputActions.SPACE_BAR_INPUT))
        {
            InitMap((sizeX, sizeY));
        }
    }

    public override void _EnterTree()
    {
        tilesFloor = GetNode<Tiles>("Tiles_Floor");
        tilesWalls = GetNode<Tiles>("Tiles_Walls");
        MapGenerator = GetNode<RLMapGenerator>("MapGenerator");
        
        tilesFloor.Map = this;
        tilesWalls.Map = this;

        tileSet = (TileSet)ResourceLoader.Load(Imports.TILESET_PATH);

        PathFinding = new PathFinding((sizeX, sizeY));

        if (initMap) InitMap((sizeX, sizeY));
    }

    public override void _Ready()
    {
        base._Ready();
    }

    // Generacja mapy
    public void InitMap((int x, int y) size)
    {
        tilesFloor.Clear();
        tilesWalls.Clear();

        mapTiles = new Tile[size.x, size.y];
        MapGenerator.GenerateMap(this);
    }

    public static bool OnMap((int x, int y) pos)
        => pos.x >= 0 && pos.x < sizeX && pos.y >= 0 && pos.y < sizeY;

    public bool CheckIfTileHasCollision(TileType tileType)
        => tileSet.TileGetShapeCount((int)tileType) > 0;

    public void SetCell(Tile newTile)
    {
        if (OnMap(newTile.Pos) is false) throw new Exception("Out of bounds " + newTile.Pos);
        mapTiles[newTile.Pos.x, newTile.Pos.y] = newTile;

        tilesFloor.SetCell(newTile.Pos.x, newTile.Pos.y, (int)newTile.TileType_Floor);
        tilesWalls.SetCell(newTile.Pos.x, newTile.Pos.y, (int)newTile.TileType_Wall);
    }

    public static Tile? GetTile((int x, int y) pos)
    {
        if (OnMap(pos) is false) return null; // throw new Exception("Out of bounds " + pos);
        return mapTiles[pos.x, pos.y];
    }
}
