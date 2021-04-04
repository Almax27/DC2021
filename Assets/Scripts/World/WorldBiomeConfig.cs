using System.Collections.Generic;
using UnityEngine;
using Utils;

[CreateAssetMenu(fileName = "WorldBiomeConfig", menuName = "Data/WorldBiomeConfig", order = 1)]
public class WorldBiomeConfig : ScriptableObject
{
    public Material spriteMaterial = null;
    public Sprite[] ceilingSprites = new Sprite[0];
    public Sprite[] groundSprites = new Sprite[0];
    public Sprite[] wallSprites = new Sprite[0];

    public bool randomiseCeilingRotation = true;
    public bool randomiseGroundRotation = true;

    public Color ambientColor = Color.white;
}