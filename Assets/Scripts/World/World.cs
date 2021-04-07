using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public partial class World
{
    public delegate bool PlotFunction(int x, int y);

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
            return tiles[index];
        }
        Debug.Assert(false);
        return new Tile();
    }

    public bool IsTilePosValid(Vector2Int p)
    {
        return p.x >= 0 && p.y >= 0 && p.x < bounds.width && p.y < bounds.height;
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

    public delegate void TileCallback(Vector2Int tile);
    public void ForEachNeighbour(int index, TileCallback callback)
    {
        var tilePos = TilePos(index);
        for (int x = -1; x < 2; x += 2)
        {
            var p = tilePos + new Vector2Int(x, 0);
            if (IsTilePosValid(p)) callback?.Invoke(p);
        }
        for (int y = -1; y < 2; y += 2)
        {
            var p = tilePos + new Vector2Int(0, y);
            if (IsTilePosValid(p)) callback?.Invoke(p);
        }
    }

    public int NeighouringTileCount(int index)
    {
        int count = 0;
        ForEachNeighbour(index, t =>
        {
            if (TileAtPostion(t).type != WorldTileType.None)
            {
                count++;
            }
        });
        return count;
    }

    public bool HasNeighborOfType(int index, WorldTileType type)
    {
        bool hasNeighbour = false;
        ForEachNeighbour(index, t =>
        {
            if (TileAtPostion(t).type == type)
            {
                hasNeighbour = true;
            }
        });
        return hasNeighbour;
    }

    public List<int> GetNeighboursOfType(int index, WorldTileType type)
    {
        List<int> neighbours = new List<int>(4);
        ForEachNeighbour(index, t =>
        {
            if (TileAtPostion(t).type == type)
            {
                neighbours.Add(TileIndex(t));
            }
        });
        return neighbours;
    }

    public bool HasUnexporedNeighbors(int index)
    {
        bool hasUnexplored = false;
        ForEachNeighbour(index, t =>
        {
            var tile = TileAtPostion(t);
            if (tile.type != WorldTileType.None && tile.explored == false)
            {
                hasUnexplored = true;
            }
        });
        return hasUnexplored;
    }

    public List<int> GetEmptyTilesInArea(RectInt area)
    {
        List<int> emptyTiles = new List<int>(area.width * area.height);
        var p = Vector2Int.zero;
        for (p.x = 0; p.x < area.width; p.x++)
        {
            for (p.y = 0; p.y < area.height; p.y++)
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

    static void Swap<T>(ref T lhs, ref T rhs)
    {
        T temp = lhs;
        lhs = rhs;
        rhs = temp;
    }

    public void PlotLine(int x0, int y0, int x1, int y1, PlotFunction plot)
    {
        bool steep = Mathf.Abs(y1 - y0) > Mathf.Abs(x1 - x0);
        if (steep) { Swap<int>(ref x0, ref y0); Swap<int>(ref x1, ref y1); }
        if (x0 > x1) { Swap<int>(ref x0, ref x1); Swap<int>(ref y0, ref y1); }
        int dX = (x1 - x0), dY = Mathf.Abs(y1 - y0), err = (dX / 2), ystep = (y0 < y1 ? 1 : -1), y = y0;

        for (int x = x0; x <= x1; ++x)
        {
            if (!(steep ? plot(y, x) : plot(x, y))) return;
            err = err - dY;
            if (err < 0) { y += ystep; err += dX; }
        }
    }

    public bool LineTrace(Vector2Int start, Vector2Int end)
    {
        bool hit = false;
        PlotLine(start.x, start.y, end.x, end.y,
            (x, y) =>
            {
                int tileIndex = x + bounds.width * y;
                if (tiles[tileIndex].type == WorldTileType.None)
                {
                    hit = true;
                    return false;
                }
                return true;
            });
        return hit;
    }

    public void RevealTiles(Vector2Int center, float radius)
    {
        //for tiles around start
        //is tile already visible? - skip
        //is tile in range? - skip
        //is tile visible to start?

        int ceilRadius = Mathf.CeilToInt(radius);
        float radiusSq = radius * radius;
        Vector2Int halfSize = new Vector2Int(ceilRadius, ceilRadius);

        var searchArea = new RectInt(center.x - halfSize.x, center.y - halfSize.x, halfSize.x * 2, halfSize.y * 2);

        tiles[TileIndex(center)].explored = true;

        var p = Vector2Int.zero;
        for (p.x = 0; p.x < searchArea.width; p.x++)
        {
            for (p.y = 0; p.y < searchArea.height; p.y++)
            {
                Vector2Int realPos = searchArea.min + p;
                int index = TileIndex(realPos);
                if (tiles[index].type == WorldTileType.None || tiles[index].explored || (p - halfSize).sqrMagnitude > radiusSq) continue;
                if(!LineTrace(center, realPos))
                {
                    tiles[index].explored = true;
                }
            }
        }
    }
}

