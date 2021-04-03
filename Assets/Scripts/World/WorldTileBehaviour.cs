using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldTileBehaviour : MonoBehaviour
{
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
            renderer.sprite = sprites[Random.Range(0, sprites.Length - 1)];
        }
    }

    void RefreshVisbility(SpriteRenderer renderer, ref World.Tile adjacentTile)
    {
        if(renderer)
        {
            renderer.enabled = adjacentTile.type == WorldTileType.None;
        }
    }

    public void PostGenerate(int tileIndex, World world, WorldConfig config)
    {
        tile = world.tiles[tileIndex];

        if (tile.biome == null)
        {
            Debug.Assert(false, $"Invalid biome for tile{name}");
            return;
        }

        ApplyMaterial(ceilingFace, ref tile.biome.ceilingSprites);
        ApplyMaterial(groundFace, ref tile.biome.groundSprites);
        ApplyMaterial(forwardFace, ref tile.biome.wallSprites);
        ApplyMaterial(backFace, ref tile.biome.wallSprites);
        ApplyMaterial(leftFace, ref tile.biome.wallSprites);
        ApplyMaterial(rightFace, ref tile.biome.wallSprites);

        World.Tile forwardTile = world.TileAtOffset(tileIndex, 0, 1);
        World.Tile backTile = world.TileAtOffset(tileIndex, 0, -1);
        World.Tile leftTile = world.TileAtOffset(tileIndex, -1, 0);
        World.Tile rightTile = world.TileAtOffset(tileIndex, 1, 0);

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
