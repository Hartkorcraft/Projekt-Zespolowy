using Godot;
using System;
using System.Collections.Generic;

public class Map : Node
{
    public const int TILE_SIZE = 8;
    public static readonly Vector2 HALF_TILE = new Vector2(4, 4);

    public PathFinding PathFinding { get; private set; } = null!;
    public RLMapGenerator MapGenerator { get; private set; } = null!;

    bool initMap = true;

    // komórki mapy
    Dictionary<(int x, int y), Tile> mapTiles = new Dictionary<(int x, int y), Tile>();

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
            InitMap();
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
    }

    public override void _Ready()
    {
        if (initMap) InitMap();
    }

    // Generacja mapy
    public void InitMap()
    {
        tilesFloor.Clear();
        tilesWalls.Clear();
        Bullet.Map = this;

        MapGenerator.InitMap(this);
        PathFinding = new PathFinding(mapTiles);
    }

    public bool OnMap((int x, int y) pos)
        => mapTiles.ContainsKey(pos);

    public bool CheckIfTileHasCollision(TileType tileType)
        => tileSet.TileGetShapeCount((int)tileType) > 0;

    public void SetCell(Tile newTile)
    {
        //if (OnMap(newTile.Pos) is false) throw new Exception("Out of bounds " + newTile.Pos);

        mapTiles[newTile.Pos] = newTile;
        tilesFloor.SetCell(newTile.Pos.x, newTile.Pos.y, (int)newTile.TileType_Floor);
        tilesWalls.SetCell(newTile.Pos.x, newTile.Pos.y, (int)newTile.TileType_Wall);
    }

    public Tile? GetTile((int x, int y) pos)
    {
        if (OnMap(pos) is false) throw new Exception("Out of bounds " + pos);
        return mapTiles[pos];
    }
}
