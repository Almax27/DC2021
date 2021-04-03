using Delaunay.Geo;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGeneratorBehaviour : MonoBehaviour
{
    public bool debugWorldBounds = false;
    public bool debugTiles = false;
    public bool debugSpanningTree = false;

    public int seed;

    public WorldConfig config;

    [SerializeField]
    GameObject worldRoot = null;
    public GameObject tilePrefab = null;

    public World World { get; private set; }

    public readonly WorldGenerator worldGenerator = new WorldGenerator();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnDrawGizmos()
    {
        if (World != null)
        {
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
                    Vector2 pos = World.TilePosWS(i);
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

        World = new World(seed);
        yield return worldGenerator.Generate(World, config);

        if (worldRoot)
        {
            if (Application.isPlaying)
            {
                Destroy(worldRoot);
            }
            else
            {
                DestroyImmediate(worldRoot);
            }
        }
        worldRoot = new GameObject("WorldRoot");
        worldRoot.transform.parent = gameObject.transform;
        worldRoot.isStatic = true;

        if (tilePrefab == null)
            yield break;

        for(int i = 0; i < World.tiles.Length; i++)
        {
            if(World.tiles[i].type != WorldTileType.None)
            {
                Vector2 p = World.TilePosWS(i);
                var newTile = Instantiate<GameObject>(tilePrefab, new Vector3(p.x, 0.0f, p.y), Quaternion.identity, gameObject.transform);
                newTile.transform.parent = worldRoot.transform;
                worldRoot.isStatic = true;

                var tileBehaviour = newTile.GetComponent<WorldTileBehaviour>();
                if (tileBehaviour)
                {
                    tileBehaviour.PostGenerate(i, World, config);
                }
            }
        }
    }

    public void CancelGeneration()
    {
        worldGenerator.Cancel();
    }
}