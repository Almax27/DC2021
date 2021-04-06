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

    public int TileIndex(Vector2Int p)
    {
        return p.x + bounds.width * p.y;
    }

    public Tile TileAtPostion(Vector2Int p)
    {
        int index = TileIndex(p);
        if (index >= 0 && index < tiles.Length)
        {
            return tiles[TileIndex(p)];
        }
        return new Tile();
    }

    public bool IsTilePosValid(int x, int y)
    {
        return x >= 0 && y >= 0 && x < bounds.width && y < bounds.height;
    }

    public Tile TileAtOffset(int index, Vector2Int offset)
    {
        Vector2Int p = TilePos(index);        
        int offsetIndex = TileIndex(p + offset);
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
            count += TileAtOffset(index, new Vector2Int(x, 0)).type != WorldTileType.None ? 1 : 0;
        }
        for (int y = -1; y < 2; y += 2)
        {
            count += TileAtOffset(index, new Vector2Int(0, y)).type != WorldTileType.None ? 1 : 0;
        }
        return count;
    }

    public bool HasNeighborOfType(int index, WorldTileType type)
    {
        for (int x = -1; x < 2; x += 2)
        {
            if (TileAtOffset(index, new Vector2Int(x,0)).type == type) return true;
        }
        for (int y = -1; y < 2; y += 2)
        {
            if (TileAtOffset(index, new Vector2Int(0, y)).type == type) return true;
        }
        return false;
    }

    public List<int> GetNeighboursOfType(int index, WorldTileType type)
    {
        List<int> neighbours = new List<int>(4);
        for (int x = -1; x < 2; x += 2)
        {
            var p = new Vector2Int(x, 0);
            if (TileAtOffset(index, p).type == type) neighbours.Add(TileIndex(p));
        }
        for (int y = -1; y < 2; y += 2)
        {
            var p = new Vector2Int(0, y);
            if (TileAtOffset(index,p).type == type) neighbours.Add(TileIndex(p));
        }
        return neighbours;
    }

    public List<int> GetEmptyTilesInArea(RectInt area)
    {
        List<int> emptyTiles = new List<int>(area.width * area.height);
        var p = Vector2Int.zero;
        for (; p.x < area.width; p.x++)
        {
            for (; p.y < area.height; p.y++)
            {
                int index = TileIndex(area.min + p);
                if(tiles[index].IsEmpty)
                {
                    emptyTiles.Add(index);
                }
            }
        }
        return emptyTiles;
    }
}

