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

    public GameState GameState { get; protected set; }

    //Normalised music time relative to BPM. I.e. 0-1 per beat
    public float MusicTime { get; private set; }

    public float BeatDuration => musicConfig.BPM / 60.0f;

    WorldGeneratorBehaviour worldGeneratorBehaviour;
    public World World => worldGeneratorBehaviour ? worldGeneratorBehaviour.World : null;
    public WorldGeneratorBehaviour WorldGen => worldGeneratorBehaviour;

    float previousMusicTime = 0.0f;
    public UnityEvent onBeatEvent;

    public PlayerBehaviour Player { get; private set; }

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
        StartCoroutine(LoadNewWorld());
    }

    void OnGUI()
    {
        GUI.color = Color.white;
        GUI.Label(new Rect(0, 0, 100, 20), $"Music = {MusicTime:F2}");
        GUI.color = (MusicTime + 0.1f) % 2.0f < 1.0f ? Color.magenta : Color.white;
        GUI.Label(new Rect(0, 30, 100, 20), $"Beat = {MusicTime%1.0f:F2}");
    }

    IEnumerator LoadNewWorld()
    {
        Debug.Assert(GameState == GameState.Uninitialised, $"Failed to load new world... GameState={GameState}");

        GameState = GameState.GeneratingWorld;

        yield return worldGeneratorBehaviour.Generate();

        GameState = GameState.SpawningPlayer;

        yield return SpawnPlayer();

        GameState = GameState.Playing;
    }

    public float SecondsToMusicTime(float seconds)
    {
        return seconds * (musicConfig.BPM / 60.0f);
    }

    void Update()
    {
        if (musicSource && musicSource.isPlaying)
        {
            MusicTime = (musicConfig.TimeOffset+ musicSource.time) * BeatDuration;
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

    public IEnumerator SpawnPlayer()
    {
        var playerGO = Instantiate<GameObject>(gameConfig.playerPrefab.gameObject);
        Player = playerGO.GetComponent<PlayerBehaviour>();

        //Find rooms furthest from each other
        int[] bestRoomPair = new int[2];
        float bestDistSq = 0.0f;
        for(int iRoomA = 0; iRoomA < World.rooms.Count; iRoomA++)
        {
            for (int iRoomB = iRoomA; iRoomB < World.rooms.Count; iRoomB++)
            {
                float distSq = (World.rooms[iRoomA].rect.center - World.rooms[iRoomB].rect.center).sqrMagnitude;
                if(distSq > bestDistSq)
                {
                    bestDistSq = distSq;
                    bestRoomPair[0] = iRoomA;
                    bestRoomPair[1] = iRoomB;
                }
            }
        }

        bestRoomPair.Shuffle();

        var spawnRoom = World.rooms[bestRoomPair[0]];
        Vector2Int spawnTile = new Vector2Int((int)spawnRoom.rect.center.x, (int)spawnRoom.rect.center.y);

        var spawnRoomExitTile = spawnRoom.exitTiles[0];
        var spawnHeading = WorldAgentBehaviour.BestHeadingForDirection(spawnRoomExitTile - spawnTile);

        Player.WarpTo(spawnTile, spawnHeading);

        yield return null;
    }
}
