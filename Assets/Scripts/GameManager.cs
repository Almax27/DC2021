using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum GameState
{
    Uninitialised,
    GeneratingWorld,
    SpawningPlayer,
    Playing,
    CleaningUp
}


public class GameManager : MonoBehaviour
{
    public GameConfig gameConfig = null;

    public AudioSource musicSource = null;
    GameConfig.MusicConfig musicConfig;

    public float GameSpeed = 1.0f;
    public float TimeOffsetOverride = -1.0f;

    public GameState GameState { get; protected set; }

    //Normalised music time relative to BPM. I.e. 0-1 per beat
    public float MusicTime { get; private set; }

    public float BeatsPerSecond => musicConfig.BPM / 60.0f;

    WorldGeneratorBehaviour worldGeneratorBehaviour;
    public World World 
    { 
        get
        {
            if (worldGeneratorBehaviour) return worldGeneratorBehaviour.World;
            else return FindObjectOfType<WorldGeneratorBehaviour>()?.World;
        }
    }

    public WorldGeneratorBehaviour WorldGen => worldGeneratorBehaviour ? worldGeneratorBehaviour : FindObjectOfType<WorldGeneratorBehaviour>();

    float previousMusicTime = 0.0f;
    public UnityEvent onBeatEvent;

    PlayerBehaviour _player = null;
    public PlayerBehaviour Player { get { if (_player) return _player; else { _player = FindObjectOfType<PlayerBehaviour>(); return _player; } } }

    public int ObjectiveCompleteCount { get; set; }

    private void Awake()
    {
        if (!musicSource)
        {
            musicSource = GetComponent<AudioSource>();
        }

        GameState = GameState.Uninitialised;

        worldGeneratorBehaviour = FindObjectOfType<WorldGeneratorBehaviour>();
        Debug.Assert(worldGeneratorBehaviour, "Failed to find a World Generator");
    }

    // Start is called before the first frame update
    void Start()
    {
        PlayMusic(0);
        LoadNewWorld();
    }

    void OnGUI()
    {
        GUI.color = Color.white;
        GUI.Label(new Rect(0, 0, 100, 20), $"Music = {MusicTime:F2}");
        GUI.color = (MusicTime + 0.1f) % 2.0f < 1.0f ? Color.magenta : Color.white;
        GUI.Label(new Rect(0, 30, 100, 20), $"Beat = {MusicTime%1.0f:F2}");
    }

    void LoadNewWorld()
    {
        Debug.Assert(GameState == GameState.Uninitialised, $"Failed to load new world... GameState={GameState}");

        GameState = GameState.GeneratingWorld;

        worldGeneratorBehaviour.Generate();

        GameState = GameState.Playing;
    }

    public float SecondsToMusicTime(float seconds)
    {
        return seconds * BeatsPerSecond;
    }

    void Update()
    {
        if (musicSource && musicSource.isPlaying)
        {
            float offset = TimeOffsetOverride >= 0 ? TimeOffsetOverride : musicConfig.TimeOffset;
            MusicTime = (offset + musicSource.time) * BeatsPerSecond;
            musicSource.pitch = GameSpeed;

            if(MusicTime % 1.0f < previousMusicTime % 1.0f)
            {
                //We've looped a beat
                onBeatEvent.Invoke();
            }
            previousMusicTime = MusicTime;
        }

        if(World != null && ObjectiveCompleteCount >= World.config.objectiveCount)
        {
            LoadNewWorld();
        }
    }

    public void PlayMusic(int index)
    {
        if(gameConfig && index >= 0 && index < gameConfig.musicConfigs.Count)
        {
            musicConfig = gameConfig.musicConfigs[index];
            musicSource.clip = musicConfig.Clip;
            musicSource.Play();
        }
        else
        {
            Debug.LogAssertion($"Invalid index {index}");
        }
    }
}
