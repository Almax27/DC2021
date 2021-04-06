using Delaunay.Geo;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class WorldGeneratorBehaviour : MonoBehaviour
{
    public bool debugWorldBounds = false;
    public bool debugTiles = false;
    public bool debugSpanningTree = false;
    public bool debugPathing = false;

    public int seed;

    public WorldConfig config;

    [SerializeField]
    GameObject worldRoot = null;

    public GameObject tilePrefab = null;

    public World World { get; private set; }

    List<GameObject> generatedObjects = new List<GameObject>();
    public List<GameObject> GeneratedObjects => generatedObjects;

    public readonly WorldGenerator worldGenerator = new WorldGenerator();

    public WorldPathFinder pathFinder = new WorldPathFinder();
    public List<Vector2Int> pathResults = new List<Vector2Int>();
    public Vector2Int pathStart = new Vector2Int();
    public Vector2Int pathEnd = new Vector2Int();

    private void Awake()
    {
        World = null;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnDrawGizmos()
    {
        if (World != null)
        {
            if (debugPathing)
            {
                if (pathFinder.IsProcessing)
                {
                    foreach (var p in pathFinder.OpenSet)
                    {
                        Gizmos.color = Color.magenta;
                        Gizmos.DrawCube(new Vector3(p.x + 0.5f, 0.5f, p.y + 0.5f), new Vector3(0.5f, 2.0f, 0.5f));
                    }
                }

                for(int i = 0; i < pathResults.Count; i++)
                {
                    float tval = (float)i / pathResults.Count;
                    Gizmos.color = new Color(tval, tval, tval);
                    Gizmos.DrawCube(new Vector3(pathResults[i].x + 0.5f, 0.5f, pathResults[i].y + 0.5f), new Vector3(0.5f, 3.0f, 0.5f));
                }
            }

            if (debugWorldBounds)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireCube(new Vector3(World.Bounds.center.x, 0, World.Bounds.center.y),
                                    new Vector3(World.Bounds.size.x, 0, World.Bounds.size.y));
            }

            if (debugTiles)
            {
                for (int i = 0; i < World.tiles.Length; i++)
                {
                    switch (World.tiles[i].type)
                    {
                        case WorldTileType.None:
                            continue;
                        case WorldTileType.Corridor:
                            Gizmos.color = Color.grey;
                            break;
                        case WorldTileType.Room:
                            Gizmos.color = Color.green;
                            break;
                        default:
                            Gizmos.color = Color.magenta;
                            break;
                    }
                    Vector2 pos = World.TilePos(i);
                    Gizmos.DrawCube(new Vector3(pos.x + 0.5f, 0.5f * World.tiles[i].height, pos.y + 0.5f),
                                    new Vector3(0.8f, World.tiles[i].height, 0.8f));
                }
            }

            if (debugSpanningTree && worldGenerator.SpanningTree != null)
            {
                Gizmos.color = Color.green;
                for (int i = 0; i < worldGenerator.SpanningTree.Count; i++)
                {
                    LineSegment seg = worldGenerator.SpanningTree[i];
                    Vector2 left = (Vector2)seg.p0;
                    Vector2 right = (Vector2)seg.p1;
                    Gizmos.DrawLine(new Vector3(left.x, 0, left.y), new Vector3(right.x, 0, right.y));
                }
            }


        }
    }

    public void Generate()
    {
        gameObject.isStatic = true;

        generatedObjects.Clear();

        worldRoot = null;
        while(transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }

        worldRoot = new GameObject("WorldRoot");
        worldRoot.transform.parent = gameObject.transform;
        worldRoot.isStatic = true;

        World = new World(seed, config);
        worldGenerator.Generate(World, config);

        if (tilePrefab == null)
            return;

        for (int i = 0; i < World.tiles.Length; i++)
        {
            if(World.tiles[i].type != WorldTileType.None)
            {
                Vector2Int p = World.TilePos(i);
                var newTile = Instantiate<GameObject>(tilePrefab, new Vector3(p.x, 0.0f, p.y), Quaternion.identity, gameObject.transform);
                generatedObjects.Add(newTile);

#if UNITY_EDITOR
                newTile.name = $"{p}";
#endif

                newTile.transform.parent = worldRoot.transform;
                worldRoot.isStatic = true;

                var tileBehaviour = newTile.GetComponent<WorldTileBehaviour>();
                if (tileBehaviour)
                {
                    World.tiles[i].behaviour = tileBehaviour;
                    tileBehaviour.PostGenerate(i, World, config);
                    SpawnPropsInTile(World, p, tileBehaviour);
                }
            }
        }

        //Build ambient lights
        foreach (var room in World.rooms)
        {
            Vector3 p = new Vector3(room.rect.center.x, 0, room.rect.center.y);
            float radius = (room.rect.max - room.rect.min).magnitude / 2 + 1.0f;
            Color color = room.config.biome.ambientColor;
            //color = new Color(Random.value, Random.value, Random.value);

            for (int i = -1; i < 2; i += 2)
            {
                var lightGO = Instantiate<GameObject>(config.ambientRoomLightPrefab.gameObject, worldRoot.transform);
                generatedObjects.Add(lightGO);
                var light = lightGO.GetComponent<Light>();
                lightGO.isStatic = true;
                light.transform.position = new Vector3(room.rect.center.x, 0.5f * room.height + (i * radius * 0.5f), room.rect.center.y);
                light.range = radius;
                light.color = color;
                if (i == -1) light.cullingMask = (1 << 3);
            }
        }

        SpawnPlayer();

        SpawnObjectives();

        SpawnEnemies();
    }

    void SpawnPropsInTile(World world, Vector2Int tile, WorldTileBehaviour tileBehaviour)
    {
        int tileIndex = world.TileIndex(tile);

        var biome = tileBehaviour.tile.biome;
        if (biome.grassSprites.Length > 0 && config.propPrefab.gameObject)
        {
            float density = 1.0f - ((float)world.NeighouringTileCount(tileIndex) / 8.0f);
            int grassCount = (int)(biome.grassPerTile.Random * density);
            for (int i = 0; i < grassCount; i++)
            {
                var go = Instantiate<GameObject>(config.propPrefab.gameObject, tileBehaviour.transform);
                go.isStatic = true;
                var spriteRenderer = go.GetComponent<SpriteRenderer>();
                spriteRenderer.sprite = biome.grassSprites[Random.Range(0, biome.grassSprites.Length)];
                spriteRenderer.material = biome.propsMaterial;
                go.transform.position = new Vector3(tile.x + 0.5f + Random.Range(-0.45f, 0.45f), 0.0f, tile.y + 0.5f + Random.Range(-0.45f, 0.45f));
            }
        }

        if(Random.value < biome.torchDensity)
        {
            if(Random.value < 0.5f && biome.ceilingTorchesPrefabs.Length > 0)
            {
                //Place randomly off center
                var prefab = biome.ceilingTorchesPrefabs[Random.Range(0, biome.ceilingTorchesPrefabs.Length)];
                Vector3 position = new Vector3(tile.x, 0, tile.y);
                position.x += 0.5f + (Random.value > 0 ? 1 : -1) * Random.Range(0.3f, 0.4f);
                position.y += world.TileAtPostion(tile).height;
                position.z += 0.5f + (Random.value > 0 ? 1 : -1) * Random.Range(0.3f, 0.4f);
                var go = Instantiate<GameObject>(prefab.gameObject, position, Quaternion.identity, tileBehaviour.transform);
                go.isStatic = true;
            }
            else if(biome.groundTorchesPrefabs.Length > 0)
            {
                var prefab = biome.groundTorchesPrefabs[Random.Range(0, biome.groundTorchesPrefabs.Length)];

                //Place along a wall
                List<int> wallTiles = world.GetNeighboursOfType(tileIndex, WorldTileType.None);
                if(wallTiles.Count > 0)
                {
                    Vector2Int wallTile = world.TilePos(wallTiles[Random.Range(0, wallTiles.Count)]);
                    Vector2Int dir = wallTile - tile;
                    Vector3 position = new Vector3(wallTile.x + 0.5f + (dir.x * 0.4f) + (dir.y * Random.Range(-0.4f, 0.4f)), 0, wallTile.y + 0.5f + (dir.y * 0.4f) + (dir.x * Random.Range(-0.4f, 0.4f)));
                    var go = Instantiate<GameObject>(prefab.gameObject, position, Quaternion.identity, tileBehaviour.transform);
                    go.isStatic = true;
                }
            }
        }
    }

    void SpawnPlayer()
    {
        var playerGO = Instantiate<GameObject>(config.playerPrefab.gameObject, transform);
        playerGO.SetActive(Application.isPlaying);

        var Player = playerGO.GetComponent<PlayerBehaviour>();
        Player.PostSpawn();

        //Find rooms furthest from each other
        int[] bestRoomPair = new int[2];
        float bestDistSq = 0.0f;
        for (int iRoomA = 0; iRoomA < World.rooms.Count; iRoomA++)
        {
            for (int iRoomB = iRoomA; iRoomB < World.rooms.Count; iRoomB++)
            {
                float distSq = (World.rooms[iRoomA].rect.center - World.rooms[iRoomB].rect.center).sqrMagnitude;
                if (distSq > bestDistSq)
                {
                    bestDistSq = distSq;
                    bestRoomPair[0] = iRoomA;
                    bestRoomPair[1] = iRoomB;
                }
            }
        }

        bestRoomPair.Shuffle();

        var spawnRoom = World.rooms[bestRoomPair[0]];

        Vector2Int spawnTile = new Vector2Int((int)spawnRoom.rect.center.x, (int)spawnRoom.rect.center.y);

        if(!Player.IsWalkable(spawnTile))
        {
            //find next best
            foreach(var backupSpawnTile in spawnRoom.rect.allPositionsWithin)
            {
                if(Player.IsWalkable(backupSpawnTile))
                {
                    spawnTile = backupSpawnTile;
                    break;
                }
            }
        }

        var spawnRoomExitTile = spawnRoom.exitTiles[0];
        var spawnHeading = WorldAgentBehaviour.BestHeadingForDirection(spawnRoomExitTile - spawnTile);

        if(!Player.WarpTo(spawnTile, spawnHeading))
        {
            UnityEngine.Debug.LogError($"Failed to spawn player at {spawnTile}");
        }
    }

    void SpawnObjectives()
    {
        //pick a random room and put a table in it
        var shuffledRooms = new List<World.Room>(World.rooms);
        shuffledRooms.Shuffle();

        for (int i = 0; i < config.objectiveCount && i < shuffledRooms.Count; i++)
        {
            var room = shuffledRooms[i];

            var emptyTiles = World.GetEmptyTilesInArea(room.rect);
            UnityEngine.Debug.Assert(emptyTiles.Count > 0);

            int tileIndex = emptyTiles[Random.Range(0, emptyTiles.Count)];
            var tilePos = World.TilePos(tileIndex);

            var objectiveGO = Instantiate<GameObject>(config.objectivePrefab.gameObject, new Vector3(tilePos.x + 0.5f, 0, tilePos.y + 0.5f), Quaternion.identity, World.tiles[tileIndex].behaviour.transform);
            World.tiles[tileIndex].interactable = objectiveGO.GetComponent<ObjectiveInteractableBehaviour>();
        }
    }

    void SpawnEnemies()
    {
        foreach(var room in World.rooms)
        {
            if(room.config.enemyTypes.Length == 0)
            {
                continue;
            }

            var emptyTiles = World.GetEmptyTilesInArea(room.rect);
            emptyTiles.Shuffle();

            int numberOfEnemies = room.config.enemyCountRange.Random;
            for(int i = 0; i < numberOfEnemies; i++)
            {
                int emptyTileIndex = emptyTiles.Count - 1;
                int tileIndex = emptyTiles[emptyTiles.Count - 1];
                emptyTiles.RemoveAt(emptyTileIndex);

                var tilePos = World.TilePos(tileIndex);

                var enemyPrefab = room.config.enemyTypes[Random.Range(0, room.config.enemyTypes.Length)];

                var enemyGO = Instantiate<GameObject>(enemyPrefab.gameObject, new Vector3(tilePos.x + 0.5f, 0, tilePos.y + 0.5f), Quaternion.identity, transform);
            }
        }
    }

    public void CancelGeneration()
    {
        worldGenerator.Cancel();
    }
}