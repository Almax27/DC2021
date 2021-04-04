using System.Collections.Generic;
using UnityEngine;

public partial class World
{
    public readonly WorldConfig config;
    public readonly int seed;

    RectInt bounds = new RectInt();
    public RectInt Bounds => bounds;

    public Tile[] tiles;
    public List<Room> rooms;
    public List<Corridor> corridors;

    public World(int _seed, WorldConfig _config) : base()
    {
        seed = _seed;
        config = _config;
        tiles = new Tile[0];
        rooms = new List<Room>();
        corridors = new List<Corridor>();
    }

    public void InitTilesFromBounds(RectInt _bounds)
    {
        bounds = _bounds;
        tiles = new Tile[_bounds.width * _bounds.height];
    }

    public Vector2Int TilePos(int index)
    {
        return new Vector2Int(index % bounds.width, index / bounds.width);
    }

    public int TileIndex(int x, int y)
    {
        return x + bounds.width * y;
    }

    public Tile TileAtPostion(int x, int y)
    {
        int index = TileIndex(x, y);
        if (index >= 0 && index < tiles.Length)
        {
            return tiles[TileIndex(x, y)];
        }
        return new Tile();
    }

    public Tile TileAtOffset(int index, int xOffset, int yOffset)
    {
        Vector2Int p = TilePos(index);        
        int offsetIndex = TileIndex(p.x + xOffset, p.y + yOffset);
        if (offsetIndex >= 0 && offsetIndex < tiles.Length)
        {
            return tiles[offsetIndex];
        }
        return new Tile();
    }    
}

