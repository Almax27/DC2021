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

    public HUDBehaviour HUD;

    public AudioSource musicSource = null;
    GameConfig.MusicConfig musicConfig;

    public float GameSpeed = 1.0f;
    public float TimeOffsetOverride = -1.0f;

    public int levelIndex = 0;
    public bool randomiseLevel = false;

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

        HUD = FindObjectOfType<HUDBehaviour>();
        Debug.Assert(HUD, "Failed to find the HUD");
    }

    // Start is called before the first frame update
    void Start()
    {
        if(randomiseLevel) worldGeneratorBehaviour.seed = Random.Range(int.MinValue, int.MaxValue);
        LoadWorld();
    }

    public void ProgressToNextLevel()
    {
        levelIndex++;
        if(levelIndex < gameConfig.musicConfigs.Count)
        {
            if (randomiseLevel) worldGeneratorBehaviour.seed = Random.Range(int.MinValue, int.MaxValue);
            LoadWorld();
        }
        else //restart
        {
            levelIndex = 0;
            LoadWorld();
        }
    }

    void LoadWorld()
    {
        //Debug.Assert(GameState == GameState.Uninitialised, $"Failed to load new world... GameState={GameState}");

        ObjectiveCompleteCount = 0;

        GameState = GameState.GeneratingWorld;

        worldGeneratorBehaviour.Generate();

        GameState = GameState.Playing;

        PlayMusic(levelIndex);

        HUD.ShowText($"Stage {levelIndex+1}");
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

    public void OnObjectiveComplete()
    {
        ObjectiveCompleteCount++;
        if (ObjectiveCompleteCount >= World.config.objectiveCount)
        {
            if (levelIndex == 0)
            {
                HUD.ShowText($"Things are gunna get spicy!", 10);
            }
            else if(levelIndex == 1)
            {
                HUD.ShowText($"IT AINT OVER TILL IT'S OVER", 10);
            }
            else
            {
                HUD.ShowText($"The Funk Dungeon Never Sleeps!\nThanks for playing!\n\nAaron, Luke & James", 10);
            }

            StopAllCoroutines();
            StartCoroutine(LevelComplete());

            
        }
        else
        {
            HUD.ShowText($"Found {ObjectiveCompleteCount}/{World.config.objectiveCount} party things!");
        }
    }

    public void OnPlayerDied()
    {
        StopAllCoroutines();
        StartCoroutine(RestartLevel());
    }

    IEnumerator LevelComplete()
    {
        Destroy(Player);
        if (levelIndex < gameConfig.musicConfigs.Count - 1)
        {
            yield return new WaitForSeconds(1.0f);
        }
        else
        {
            yield return new WaitForSeconds(3.0f);
        }
        yield return HUD.FadeOut(true);
        ProgressToNextLevel();
        yield return HUD.FadeOut(false);
    }

    IEnumerator RestartLevel()
    {
        Destroy(Player);
        yield return HUD.FadeOut(true);
        LoadWorld();
        yield return HUD.FadeOut(false);
        yield return null;
    }

}
