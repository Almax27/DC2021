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

    WorldGeneratorBehaviour worldGeneratorBehaviour;
    public World World => worldGeneratorBehaviour ? worldGeneratorBehaviour.World : null;

    float previousMusicTime = 0.0f;
    public UnityEvent onBeatEvent;

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
        GUI.color = MusicTime % 2.0f < 1.0f ? Color.magenta : Color.grey;
        GUI.DrawTexture(Rect.MinMaxRect(0, 0, 20, 20), Texture2D.whiteTexture);
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

    void Update()
    {
        if (musicSource && musicSource.isPlaying)
        {
            MusicTime = (musicConfig.TimeOffset+ musicSource.time) * (musicConfig.BPM / 60.0f);
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
        playerGO.SendMessage("OnSpawn", this);

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

        playerGO.GetComponent<PlayerBehaviour>().WarpTo(spawnTile, spawnHeading);

        yield return null;
    }
}
