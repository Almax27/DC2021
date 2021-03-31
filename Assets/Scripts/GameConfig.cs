using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "Data/GameConfig", order = 1)]
public class GameConfig : ScriptableObject
{
    [System.Serializable]
    public struct MusicConfig
    {
        public AudioClip Clip;
        public float BPM;
    }

    public List<MusicConfig> musicConfigs = new List<MusicConfig>();    
}
