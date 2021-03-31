using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameConfig gameConfig = null;

    public AudioSource musicSource = null;
    GameConfig.MusicConfig musicConfig;

    public float GameSpeed = 1.0f;

    //Normalised music time relative to BPM. I.e. 0-1 per beat
    public float MusicTime { get; private set; }

    private void Awake()
    {
        if (!musicSource)
        {
            musicSource = GetComponent<AudioSource>();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        PlayMusic(0);
    }

    void Update()
    {
        if (musicSource && musicSource.isPlaying)
        {
            MusicTime = musicSource.time * (musicConfig.BPM / 60.0f);
            musicSource.pitch = GameSpeed;
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
