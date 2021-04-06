using System.Collections.Generic;
using UnityEngine;
using Utils;

[CreateAssetMenu(fileName = "WorldConfig", menuName = "Data/WorldConfig", order = 1)]
public class WorldConfig : ScriptableObject
{
    [System.Serializable]
    public struct Room
    {
        [Tooltip("Chance of this room to appear in the map")]
        public float weight;

        public MinMaxInt sizeRange;

        [Tooltip("Minimum number of tiles to fill")]
        public MinMaxInt fillCountRange;

        public WorldBiomeConfig biome;

        public EnemyBehaviour[] enemyTypes;
        public MinMaxInt enemyCountRange;
    }

    public Vector2Int worldSize;
    public MinMaxInt roomCountRange;

    [Range(0,1)] 
    public float roomReduction = 0.5f;

    public List<Room> roomConfigs;

    public MinMaxInt roomPadding = new MinMaxInt(2,2);

    public WorldBiomeConfig corridorBiome;

    public PlayerBehaviour playerPrefab;

    public Light ambientRoomLightPrefab;

    public SpriteRenderer propPrefab;

    public int objectiveCount = 3;
    public ObjectiveInteractableBehaviour objectivePrefab;
    
}