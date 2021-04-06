using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Delaunay;
using Delaunay.Geo;

public class WorldGenerator
{
    enum Direction { NONE, UP, RIGHT, DOWN, LEFT };

    Direction[] GetNextDirections(Direction dir)
    {
        switch (dir)
        {
            case Direction.UP:
                return new Direction[] { Direction.UP, Direction.LEFT, Direction.RIGHT };
            case Direction.DOWN:
                return new Direction[] { Direction.DOWN, Direction.LEFT, Direction.RIGHT };
            case Direction.LEFT:
                return new Direction[] { Direction.UP, Direction.DOWN, Direction.LEFT };
            case Direction.RIGHT:
                return new Direction[] { Direction.UP, Direction.DOWN, Direction.RIGHT };
            case Direction.NONE:
                return new Direction[] { Direction.UP, Direction.DOWN, Direction.LEFT, Direction.RIGHT };
        }
        return new Direction[0];
    }

    Vector2Int GetDirectionOffset(Direction dir)
    {
        switch (dir)
        {
            case Direction.UP:
                return new Vector2Int(0, 1);
            case Direction.DOWN:
                return new Vector2Int(0, -1);
            case Direction.LEFT:
                return new Vector2Int(-1, 0);
            case Direction.RIGHT:
                return new Vector2Int(1, 0);
        }
        return new Vector2Int(0, 0);
    }

    struct PendingRoom
    {
        public PendingRoom(int _index, Direction _direction) { sourceRoomIndex = _index; direction = _direction; }
        public int sourceRoomIndex;
        public Direction direction;
    }

    public WorldGenerator()
    {
        Speed = 1.0f;
        SpanningTree = new List<LineSegment>();
    }

    public float Speed { get; set; }
    public float WaitScale => Mathf.Clamp01(1.0f - (0.1f + Speed * 0.9f));

#if UNITY_EDITOR
    public string Status { get; set; }
#endif

    public bool IsRunning { get; private set; }
    public bool IsCanceled { get; private set; }

    public List<LineSegment> SpanningTree { get; private set; }

    public void Cancel()
    {
        IsCanceled = true;
    }

    public bool ShouldWait => Speed < 1;

    public IEnumerator Wait(float duration)
    {
        var randState = Random.state;
        yield return new WaitForSecondsRealtime(duration * WaitScale);
        Random.state = randState;
        if (IsCanceled) yield break;
    }

    public void Generate(World world, WorldConfig config)
    {
        Debug.Assert(world != null);

        Debug.Log($"Generating world... config={config.name}, seed={world.seed}");

        Random.InitState(world.seed);

        IsCanceled = false;
        IsRunning = true;
        Status = "Starting";

        SpanningTree.Clear();

        world.InitTilesFromBounds(new RectInt(Vector2Int.zero, config.worldSize));

        CreateRooms(world, config);

        CreateCorridors(world, config);

        CreateTiles(world, config);

        IsRunning = false;

        Status = IsCanceled ? "Canceled" : "Success";
    }

    void CreateRooms(World world, WorldConfig config)
    {
        int roomCount = Random.Range(config.roomCountRange.min, config.roomCountRange.max);

        Debug.Log($"Creating {roomCount} rooms...");

        world.rooms.Clear();
        world.rooms.Capacity = roomCount;

        List<WorldConfig.Room> roomConfigs = new List<WorldConfig.Room>(config.roomConfigs);
        roomConfigs.Sort((a, b) => a.weight < b.weight ? -1 : 1);

        float totalRoomWeight = 0.0f;
        foreach (var roomConfig in roomConfigs)
        {
            totalRoomWeight += roomConfig.weight;
        }

        //Build list of configs we want to use
        List<int> roomConfigIndices = new List<int>(roomCount);
        for (int i = 0; i < roomConfigs.Count; i++)
        {
            var roomConfig = roomConfigs[i];
            int roomsToCreate = Mathf.CeilToInt((roomConfig.weight / totalRoomWeight) * roomCount);

            roomsToCreate = Mathf.Min(roomsToCreate, roomCount - roomConfigIndices.Count);
            if (roomsToCreate <= 0)
            {
                Debug.LogWarning($"Fulfilled room count without using all configs");
                break;
            }

            Debug.Log($"Creating {roomsToCreate} rooms of type {i}");
            for (int j = 0; j < roomsToCreate; j++)
            {
                roomConfigIndices.Add(i);
            }
        }

        //Shuffle rooms
        roomConfigIndices.Shuffle();

        //Layout rooms
        Queue<PendingRoom> workingSet = new Queue<PendingRoom>();
        RectInt roomRect = new RectInt((int)world.Bounds.center.x, (int)world.Bounds.center.y, 0, 0);
        Direction lastDirection = Direction.NONE;
        foreach (var iRoomConfig in roomConfigIndices)
        {
            roomRect.size = new Vector2Int(roomConfigs[iRoomConfig].sizeRange.Random, roomConfigs[iRoomConfig].sizeRange.Random);

            //Select location of next room
            PendingRoom pendingRoom = new PendingRoom();
            while (workingSet.Count > 0)
            {
                pendingRoom = workingSet.Dequeue();

                if (pendingRoom.direction != Direction.NONE)
                {
                    var halfSizeOffset = (roomRect.size + world.rooms[pendingRoom.sourceRoomIndex].rect.size) / 2;
                    halfSizeOffset += Vector2Int.one * Mathf.Max(config.roomPadding.Random, 2);

                    Vector2 newPosition = world.rooms[pendingRoom.sourceRoomIndex].rect.center + GetDirectionOffset(pendingRoom.direction) * halfSizeOffset;
                    newPosition -= roomRect.size / 2;

                    Vector2 NudgeDirection = GetDirectionOffset(pendingRoom.direction + 1);
                    NudgeDirection *= Random.Range(-1.0f, 1.0f);
                    newPosition += NudgeDirection * roomRect.size;

                    roomRect.x = Mathf.RoundToInt(newPosition.x);
                    roomRect.y = Mathf.RoundToInt(newPosition.y);
                }

                if (CanRoomBePlaced(world, ref roomRect))
                {
                    break;
                }
                else if (workingSet.Count <= 0)
                {
                    Debug.LogWarning($"Failed to generate all rooms: {world.rooms.Count}/{roomCount}\nPlease decrease Room Count or increase World Size");
                    return;
                }
            }

            //Add room to world
            int height = Random.value > 0.5f ? 1 : 2;
            world.rooms.Add(new World.Room(roomRect, roomConfigs[iRoomConfig], height));

            foreach (Direction nextDirs in GetNextDirections(lastDirection))
            {
                workingSet.Enqueue(new PendingRoom(world.rooms.Count - 1, nextDirs));
            }
        }

        int numberOfRoomsToCull = Mathf.CeilToInt(config.roomReduction * world.rooms.Count);
        if (numberOfRoomsToCull > 0)
        {
            world.rooms.Shuffle();

            for(int i = 0; i < numberOfRoomsToCull; i++)
            {
                world.rooms.RemoveAt(world.rooms.Count - 1);
            }
        }
    }

    bool CanRoomBePlaced(World world, ref RectInt rect)
    {
        int padding = 1;
        RectInt inflatedRect = rect;
        inflatedRect.x -= padding;
        inflatedRect.y -= padding;
        inflatedRect.width += padding * 2;
        inflatedRect.height += padding * 2;

        //Check world bounds
        Vector2Int[] corners = new Vector2Int[] 
        {
            new Vector2Int(rect.xMin, rect.yMin),
            new Vector2Int(rect.xMin, rect.yMax),
            new Vector2Int(rect.xMax, rect.yMin),
            new Vector2Int(rect.xMax, rect.yMax)
        };
        foreach(var corner in corners)
        {
            if(!world.Bounds.Contains(corner))
            {
                return false;
            }
        }

        //Check overlaps with other rooms
        foreach (var other in world.rooms)
        {
            if(inflatedRect.Overlaps(other.rect))
            {
                return false;
            }
        }
        return true;
    }

    void CreateCorridors(World world, WorldConfig config)
    {
        Status = "Creating Corridors...";

        List<Vector2> points = new List<Vector2>(world.rooms.Count);
        foreach(var room in world.rooms)
        {
            points.Add(room.rect.center);
        }

        Voronoi v = new Voronoi(points, new Rect(world.Bounds.position, world.Bounds.size));

        SpanningTree = v.SpanningTree(KruskalType.MINIMUM);

        world.corridors.Clear();
        world.corridors.Capacity = SpanningTree.Count;

        //Build room edge geo
        List<List<LineSegment>> roomEdges = new List<List<LineSegment>>();
        foreach (var room in world.rooms)
        {
            Vector2[] corners = new Vector2[4]
            {
                new Vector2(room.rect.xMin, room.rect.yMin),
                new Vector2(room.rect.xMax, room.rect.yMin),
                new Vector2(room.rect.xMax, room.rect.yMax),
                new Vector2(room.rect.xMin, room.rect.yMax)
            };
            List<LineSegment> edges = new List<LineSegment>()
            {
                new LineSegment(corners[0], corners[1]),
                new LineSegment(corners[1], corners[2]),
                new LineSegment(corners[2], corners[3]),
                new LineSegment(corners[3], corners[0])
            };
            roomEdges.Add(edges);
        }

        Vector2 intersection = new Vector2();
        List<int> roomsIntersected = new List<int>(2);
        List<Vector2Int> roomIntersections = new List<Vector2Int>(2);
        foreach (var connection in SpanningTree)
        {
            roomsIntersected.Clear();
            roomIntersections.Clear();
            for (int iRoom = 0; iRoom < roomEdges.Count; iRoom++)
            {
                foreach(var edge in roomEdges[iRoom])
                {
                    if(LineSegment.Intersect(edge, connection, ref intersection))
                    {
                        var intersectionTile = new Vector2Int(Mathf.FloorToInt(intersection.x), Mathf.FloorToInt(intersection.y));

                        var bestTile = intersectionTile;
                        float bestDistSq = float.MaxValue;
                        var connectionPoints = world.rooms[iRoom].connectionPoints;
                        for (int iConnectionPoint = 0; iConnectionPoint < connectionPoints.Count; iConnectionPoint++)
                        {
                            float distSq = (intersectionTile - connectionPoints[iConnectionPoint]).sqrMagnitude;
                            if(distSq < bestDistSq)
                            {
                                bestDistSq = distSq;
                                bestTile = connectionPoints[iConnectionPoint];
                            }
                        }

                        roomsIntersected.Add(iRoom);
                        roomIntersections.Add(bestTile);

                        break;
                    }
                }
                if (roomIntersections.Count == 2) break;
            }

            Debug.Assert(roomIntersections.Count == 2, $"{roomIntersections.Count} intersections found!");

            //Walk shortest path
            Vector2Int distance = roomIntersections[1] - roomIntersections[0];
            Vector2Int distanceAbs = new Vector2Int(Mathf.Abs(distance.x), Mathf.Abs(distance.y));
            Vector2Int tile = roomIntersections[0]; //start

            World.Corridor newCorridor = new World.Corridor(distanceAbs.x + distanceAbs.y, config.corridorBiome, roomsIntersected.ToArray());
            newCorridor.tiles.Add(tile);

            if (distanceAbs.x > distanceAbs.y)
            {
                int xSign = distance.x > 0 ? 1 : -1;
                for (int xStep = 0; xStep < distanceAbs.x; xStep++)
                {
                    tile.x += xSign;
                    newCorridor.tiles.Add(tile);
                }
                int ySign = distance.y > 0 ? 1 : -1;
                for (int yStep = 0; yStep < distanceAbs.y; yStep++)
                {
                    tile.y += ySign;
                    newCorridor.tiles.Add(tile);
                }
            }
            else
            {
                int ySign = distance.y > 0 ? 1 : -1;
                for (int yStep = 0; yStep < distanceAbs.y; yStep++)
                {
                    tile.y += ySign;
                    newCorridor.tiles.Add(tile);
                }
                int xSign = distance.x > 0 ? 1 : -1;
                for (int xStep = 0; xStep < distanceAbs.x; xStep++)
                {
                    tile.x += xSign;
                    newCorridor.tiles.Add(tile);
                }
            }

            world.corridors.Add(newCorridor);


            world.rooms[roomsIntersected[0]].connectedRooms.Add(world.rooms[roomsIntersected[1]]);
            world.rooms[roomsIntersected[1]].connectedRooms.Add(world.rooms[roomsIntersected[0]]);

            world.rooms[roomsIntersected[0]].exitTiles.Add(roomIntersections[0]);
            world.rooms[roomsIntersected[1]].exitTiles.Add(roomIntersections[1]);
        }
    }

    void CreateTiles(World world, WorldConfig config)
    {
        foreach (var room in world.rooms)
        {
            foreach(var p in room.rect.allPositionsWithin)
            {
                int iTile = world.TileIndex(p);
                world.tiles[iTile].type = WorldTileType.Room;
                world.tiles[iTile].height = room.height;
                world.tiles[iTile].biome = room.config.biome;
            }
        }

        foreach (var corridor in world.corridors)
        {
            foreach(var p in corridor.tiles)
            {
                int iTile = world.TileIndex(p);
                world.tiles[iTile].type = WorldTileType.Corridor;
                world.tiles[iTile].height = corridor.height;
                world.tiles[iTile].biome = corridor.biome;
            }
        }

        //Fill in some random things for fun
        foreach (var room in world.rooms)
        {
            //Fill in random tiles
            int maxNumTiles = Mathf.Min(room.rect.size.x, room.rect.size.y) - 1; //prevent blocking paths
            int numTiles = Mathf.Clamp(room.config.fillCountRange.Random, 0, maxNumTiles);
            for (int i = 0; i < numTiles; i++)
            {
                Vector2Int p = new Vector2Int(Random.Range(room.rect.xMin, room.rect.xMax), Random.Range(room.rect.yMin, room.rect.yMax));
                int index = world.TileIndex(p);
                if (world.HasNeighborOfType(index, WorldTileType.Corridor) == false)
                {
                    world.tiles[index].type = WorldTileType.None;
                    world.tiles[index].height = 0;
                }
            }
        }
    }
}
