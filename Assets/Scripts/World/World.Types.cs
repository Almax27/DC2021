using System.Collections.Generic;
using UnityEngine;

public enum WorldTileType
{
    None,
    Corridor,
    Room
}

public partial class World
{
    public struct Tile
    {
        public WorldTileType type;
        public int height;
        public WorldBiomeConfig biome;
        public WorldTileBehaviour behaviour;
        public InteractableBehaviour interactable;
        public WorldAgentBehaviour agent;
        public bool explored;

        public bool IsEmpty => (type != WorldTileType.None) && !((interactable && interactable.IsBlocking) || agent);
    }

    public class Room
    {
        public RectInt rect;
        public WorldConfig.Room config;
        public List<Vector2Int> connectionPoints;
        public List<Vector2Int> exitTiles;
        public List<Room> connectedRooms;
        public int height;

        public Room(RectInt rect, WorldConfig.Room roomConfig, int height = 1)
        {
            this.rect = rect;
            config = roomConfig;
            this.height = height;
            connectionPoints = new List<Vector2Int>();
            exitTiles = new List<Vector2Int>();
            connectedRooms = new List<Room>();
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
        }
    }

    public class Corridor
    {
        public List<Vector2Int> tiles;
        public int height;
        public WorldBiomeConfig biome;
        public int[] connectedRooms;

        public Corridor(int length, WorldBiomeConfig corridorBiome, int[] rooms)
        {
            tiles = new List<Vector2Int>(length);
            height = 0;
            biome = corridorBiome;
            connectedRooms = rooms;
        }
    }

    
}