using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldTileBehaviour : MonoBehaviour
{
    public int tileIndex;
    public World.Tile tile;

    public SpriteRenderer ceilingFace;
    public SpriteRenderer groundFace;
    public SpriteRenderer forwardFace;
    public SpriteRenderer backFace;
    public SpriteRenderer leftFace;
    public SpriteRenderer rightFace;

    void ApplyMaterial(SpriteRenderer renderer, ref Sprite[] sprites)
    {
        if(renderer && sprites.Length > 0)
        {
            renderer.sharedMaterial = tile.biome.spriteMaterial;
            renderer.sprite = sprites[Random.Range(0, sprites.Length)];
        }
    }

    void RefreshVisbility(SpriteRenderer renderer, ref World.Tile adjacentTile)
    {
        if(renderer)
        {
            renderer.enabled = adjacentTile.type == WorldTileType.None;
        }
    }

    public void PostGenerate(int index, World world, WorldConfig config)
    {
        tileIndex = index;
        tile = world.tiles[tileIndex];

        gameObject.isStatic = true;

        ceilingFace.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        groundFace.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        forwardFace.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;
        backFace.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;
        leftFace.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;
        rightFace.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;

        foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
        {
            renderer.gameObject.isStatic = true;
            renderer.receiveShadows = true;
        }

        if (tile.biome == null)
        {
            Debug.Assert(false, $"Invalid biome for tile{name}");
            return;
        }

        ceilingFace.gameObject.layer = 3;

        ApplyMaterial(ceilingFace, ref tile.biome.ceilingSprites);
        ApplyMaterial(groundFace, ref tile.biome.groundSprites);
        ApplyMaterial(forwardFace, ref tile.biome.wallSprites);
        ApplyMaterial(backFace, ref tile.biome.wallSprites);
        ApplyMaterial(leftFace, ref tile.biome.wallSprites);
        ApplyMaterial(rightFace, ref tile.biome.wallSprites);

        World.Tile forwardTile = world.TileAtOffset(tileIndex, Vector2Int.up);
        World.Tile backTile = world.TileAtOffset(tileIndex, Vector2Int.down);
        World.Tile leftTile = world.TileAtOffset(tileIndex, Vector2Int.left);
        World.Tile rightTile = world.TileAtOffset(tileIndex, Vector2Int.right);

        if(tile.biome.randomiseCeilingRotation)
        {
            ceilingFace.transform.rotation *= Quaternion.Euler(0, 0, 90 * Random.Range(0, 3));
        }

        if (tile.biome.randomiseGroundRotation)
        {
            groundFace.transform.rotation *= Quaternion.Euler(0, 0, 90 * Random.Range(0, 3));
        }

        for(int i = 1; i < world.tiles[tileIndex].height; i++)
        {
            ceilingFace.transform.position += Vector3.up;

            if (forwardTile.height < i + 1)
            {
                var newForward = Instantiate<GameObject>(forwardFace.gameObject, gameObject.transform);
                newForward.transform.position += Vector3.up;
            }

            if (backTile.height < i + 1)
            {
                var newBack = Instantiate<GameObject>(backFace.gameObject, gameObject.transform);
                newBack.transform.position += Vector3.up;
            }

            if (leftTile.height < i + 1)
            {
                var newLeft = Instantiate<GameObject>(leftFace.gameObject, gameObject.transform);
                newLeft.transform.position += Vector3.up;
            }

            if (rightTile.height < i + 1)
            {
                var newRight = Instantiate<GameObject>(rightFace.gameObject, gameObject.transform);
                newRight.transform.position += Vector3.up;
            }
        }

        if (forwardTile.type != WorldTileType.None) DestroyImmediate(forwardFace.gameObject);
        if (backTile.type != WorldTileType.None) DestroyImmediate(backFace.gameObject);
        if (leftTile.type != WorldTileType.None) DestroyImmediate(leftFace.gameObject);
        if (rightTile.type != WorldTileType.None) DestroyImmediate(rightFace.gameObject);
    }
}
