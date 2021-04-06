
using UnityEngine;

class RandomUtils
{
    public static Vector2Int RandomPointInBounds(RectInt rectInt)
    {
        return new Vector2Int(Random.Range(rectInt.xMin, rectInt.xMax - 1), Random.Range(rectInt.yMin, rectInt.yMax - 1));
    }
}