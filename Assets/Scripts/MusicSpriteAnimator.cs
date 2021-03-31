using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicSpriteAnimator : MonoBehaviour
{
    public SpriteRenderer spriteRenderer = null;
    public Sprite[] spriteFrames = new Sprite[0];

    [Tooltip("Frame to start the animation - use this to align motion with music")]
    public int StartFrame = 0;

    [Tooltip("Speed of the animation in frames per beat")]
    public float FramesPerBeat = 1.0f;

    GameManager gameManager = null;

    private void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
        if(!spriteRenderer)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (gameManager && spriteRenderer)
        {
            int spriteIndex = Mathf.FloorToInt(gameManager.MusicTime * FramesPerBeat) % spriteFrames.Length;
            if (spriteIndex >= 0)
            {
                spriteRenderer.sprite = spriteFrames[spriteIndex % spriteFrames.Length];
            }
        }
    }
}
