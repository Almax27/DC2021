using System.Collections.Generic;
using System.Threading.Tasks;
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

    public bool IsTilePosValid(int x, int y)
    {
        return x >= 0 && y >= 0 && x < bounds.width && y < bounds.height;
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

    public int NeighouringTileCount(int index)
    {
        int count = 0;
        for(int x = -1; x < 2; x += 2)
        {
            for(int y = -1; y < 2; y += 2)
            {
                count += TileAtOffset(index, x, y).type != WorldTileType.None ? 1 : 0;
            }
        }
        return count;
    }
}

