using System.Collections.Generic;
using UnityEngine;

public enum WorldTileType
{
    None,
    Corridor,
    Room
}

public class World
{
    public struct Tile
    {
        public WorldTileType type;
        public int height;
        public WorldBiomeConfig biome;
    }

    public struct Room
    {
        public RectInt rect;
        public List<Vector2Int> connectionPoints;
        public int height;
        public WorldBiomeConfig biome;

        public Room(RectInt rect, WorldBiomeConfig roomBiome, int height = 1)
        {
            this.rect = rect;
            this.height = height;
            connectionPoints = new List<Vector2Int>();
            for (int x = 0; x < rect.width; x++)
            {
                connectionPoints.Add(new Vector2Int(rect.xMin + x, rect.yMin - 1));
                connectionPoints.Add(new Vector2Int(rect.xMin + x, rect.yMax));
            }
            for (int y = 0; y < rect.height; y++)
            {
                connectionPoints.Add(new Vector2Int(rect.xMin - 1, rect.yMin + y));
                connectionPoints.Add(new Vector2Int(rect.xMax, rect.yMin + y));
            }
            biome = roomBiome;
        }
    }

    public struct Corridor
    {
        public List<Vector2Int> tiles;
        public int height;
        public WorldBiomeConfig biome;

        public Corridor(int length, WorldBiomeConfig corridorBiome)
        {
            tiles = new List<Vector2Int>(length);
            height = 0;
            biome = corridorBiome;
        }
    }

    public readonly int seed;

    RectInt bounds = new RectInt();
    public RectInt Bounds => bounds;

    public Tile[] tiles;
    public List<Room> rooms;
    public List<Corridor> corridors;

    public void InitTilesFromBounds(RectInt _bounds)
    {
        bounds = _bounds;
        tiles = new Tile[_bounds.width * _bounds.height];
    }

    public Vector2Int TilePos(int index)
    {
        return new Vector2Int(index % bounds.width, index / bounds.width);
    }

    public Vector2 TilePosWS(int index)
    {
        return new Vector2(index % bounds.width, index / bounds.width) - bounds.size / 2;
    }

    public int TileIndex(int x, int y)
    {
        return x + bounds.width * y;
    }

    public int TileIndexWS(int x, int y)
    {
        return (x + bounds.width / 2) + bounds.width * (y + bounds.height / 2);
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

    public World(int _seed) : base()
    {
        seed = _seed;
        tiles = new Tile[0];
        rooms = new List<Room>();
        corridors = new List<Corridor>();
    }
}

