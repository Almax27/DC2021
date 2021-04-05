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

    public IEnumerator Generate()
    {
        gameObject.isStatic = true;

        World newWorld = new World(seed, config);
        worldGenerator.Generate(newWorld, config);

        if (worldRoot)
        {
            DestroyImmediate(worldRoot);
        }
        worldRoot = new GameObject("WorldRoot");
        worldRoot.transform.parent = gameObject.transform;
        worldRoot.isStatic = true;

        if (tilePrefab == null)
            yield break;


        //Build tiles
#if UNITY_EDITOR
        worldGenerator.Status = "Building Tiles...";
        yield return null;
#endif

        for (int i = 0; i < newWorld.tiles.Length; i++)
        {
            if(newWorld.tiles[i].type != WorldTileType.None)
            {
                Vector2Int p = newWorld.TilePos(i);
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
                    tileBehaviour.PostGenerate(i, newWorld, config);
                    SpawnPropsInTile(newWorld, p, tileBehaviour);
                }
            }
        }

        worldGenerator.Status = "Building Ambient lights...";
        yield return null;

        //Build ambient lights
        foreach (var room in newWorld.rooms)
        {
            Vector3 p = new Vector3(room.rect.center.x, 0, room.rect.center.y);
            float radius = (room.rect.max - room.rect.min).magnitude / 2 + 1.0f;
            Color color = room.biome.ambientColor;
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

        worldGenerator.Status = "Building props...";
        yield return null;

        World = newWorld;
    }

    void SpawnPropsInTile(World world, Vector2Int tile, WorldTileBehaviour tileBehaviour)
    {
        var biome = tileBehaviour.tile.biome;
        if (biome.grassSprites.Length > 0 && config.propPrefab.gameObject)
        {
            float density = 1.0f - ((float)world.NeighouringTileCount(world.TileIndex(tile.x, tile.y)) / 8.0f);
            int grassCount = (int)(biome.grassPerTile.Random * density);
            for (int i = 0; i < grassCount; i++)
            {
                var go = Instantiate<GameObject>(config.propPrefab.gameObject, tileBehaviour.transform);
                go.isStatic = true;
                var spriteRenderer = go.GetComponent<SpriteRenderer>();
                spriteRenderer.sprite = biome.grassSprites[Random.Range(0, biome.grassSprites.Length - 1)];
                spriteRenderer.material = biome.propsMaterial;
                go.transform.position = new Vector3(tile.x + 0.5f + Random.Range(-0.45f, 0.45f), 0.0f, tile.y + 0.5f + Random.Range(-0.45f, 0.45f));
            }
        }
    }

    public void CancelGeneration()
    {
        worldGenerator.Cancel();
    }
}