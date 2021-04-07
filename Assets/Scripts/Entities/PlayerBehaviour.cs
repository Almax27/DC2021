using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum PlayerAction
{
    None,
    MoveForward,
    MoveBack,
    TurnLeft,
    TurnRight
}

enum PlayerActionAccuracy
{
    Miss,
    Poor,
    Good,
    Perfect
}

public class PlayerBehaviour : WorldAgentBehaviour
{
    public float beatGrace = 0.1f;
    public float humanResponseTime = 0.15f;
    public int beatsToSkipOnMiss = 1;
    int beatsToSkip = 0;

    public float viewDistance = 5.0f;

    PlayerAction pendingAction = PlayerAction.None;
    PlayerActionAccuracy pendingActionAccuracy = PlayerActionAccuracy.Miss;
    bool graceUsed = false;

    public bool debugIgnoreBeats = false;

    public AudioClip missClip;
    public AudioSource actionAudioSource;

    private void Start()
    {
        GameManager.onBeatEvent.AddListener(OnBeat);
        Debug.Log($"Human response time = {GameManager.SecondsToMusicTime(humanResponseTime)}");
    }

    void OnBeat()
    {
        IsDefending = false;
        IsAttacking = false;

        TryConsumePendingAction();
        beatsToSkip--;

        GameManager.World.RevealTiles(TilePosition, viewDistance);
    }

    void TryConsumePendingAction()
    {
        if (pendingActionAccuracy == PlayerActionAccuracy.Miss)
            return;

        switch (pendingAction)
        {
            case PlayerAction.MoveForward:

                var tile = GameManager.World.TileAtPostion(TilePosition + HeadingDirection(TileHeading));
                if(tile.agent && tile.agent != this)
                {
                    Attack();
                }
                else if(tile.interactable)
                {
                    Debug.Log($"INTERACT: {tile.interactable.name}");
                    tile.interactable.OnInteract();
                }
                else
                {
                    MoveForward();
                }
                break;
            case PlayerAction.MoveBack:
                MoveBackwards();
                break;
            case PlayerAction.TurnLeft:
                TurnAntiClockwise();
                break;
            case PlayerAction.TurnRight:
                TurnClockwise();
                break;
        }
        pendingAction = PlayerAction.None;
    }

    void RequestAction(PlayerAction action)
    { 
       if(pendingAction != PlayerAction.None)
        {

        }

        if (graceUsed && GameManager.MusicTime < beatGrace)
        {
            Debug.LogWarning($"{action}: Pressed while locked!");
            return;
        }

        if (beatsToSkip > 0)
        {
            Debug.LogWarning($"{action}: Pressed while skipping beats!");
            return;
        }

        graceUsed = false;
        pendingAction = action;

        float responseTimeOffet = GameManager.SecondsToMusicTime(humanResponseTime);

        float beatTime = (GameManager.MusicTime - responseTimeOffet) % 1.0f;
        beatTime -= beatGrace;
        if(debugIgnoreBeats)
        {
            pendingActionAccuracy = PlayerActionAccuracy.Perfect;
            TryConsumePendingAction();
            return;
        }
        else if (beatTime < 0.0f)
        {
            pendingActionAccuracy = PlayerActionAccuracy.Perfect;
        }
        else if (beatTime < 0.5f)
        {
            pendingActionAccuracy = PlayerActionAccuracy.Miss;
            beatsToSkip = beatsToSkipOnMiss + 1;
            actionAudioSource.pitch = 1.0f;
            actionAudioSource.PlayOneShot(missClip);
        }
        else if(beatTime < 0.7f)
        {
            pendingActionAccuracy = PlayerActionAccuracy.Poor;
        }
        else if(beatTime < 0.9f)
        {
            pendingActionAccuracy = PlayerActionAccuracy.Good;
        }
        else
        {
            pendingActionAccuracy = PlayerActionAccuracy.Perfect;
        }

        //if we've missed the beat, consume immediately
        if(beatTime < 0 && pendingActionAccuracy != PlayerActionAccuracy.Miss)
        {
            Debug.Log("Grace!");
            graceUsed = true;
            TryConsumePendingAction();
        }
        

        Debug.Log($"{pendingAction} : {pendingActionAccuracy} : b{beatTime:F2} : m{GameManager.MusicTime % 1.0f:F2}" + (graceUsed ? " : (grace)" : ""));
    }

    // Update is called once per frame
    protected override void Update()
    {
        if (GameManager.World == null) return;

        if(Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            RequestAction(PlayerAction.MoveForward);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            RequestAction(PlayerAction.MoveBack);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            RequestAction(PlayerAction.TurnLeft);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            RequestAction(PlayerAction.TurnRight);
        }

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Keypad0))
        {
            debugIgnoreBeats = !debugIgnoreBeats;
            Debug.Log($"debugIgnoreBeats={debugIgnoreBeats}");
        }

        if(Input.GetKeyDown(KeyCode.H))
        {
            if(Input.GetKey(KeyCode.LeftShift))
            {
                Heal(1);
            }
            else
            {
                TakeDamage(1);
            }
        }

        if(Input.GetKeyDown(KeyCode.O))
        {
            GameManager.OnObjectiveComplete();
        }
#endif

        base.Update();
    }

    bool Attack()
    {
        IsAttacking = true;

        Vector2Int attackTilePos = TilePosition + HeadingDirection(TileHeading);
        var attackTile = GameManager.World.TileAtPostion(attackTilePos);

        if(attackTile.agent && attackTile.agent != this)
        {
            attackTile.agent.TakeDamage(1, this);
            return true;
        }
        return false;
    }

    void Defend()
    {
        IsDefending = true;
    }

    public Vector2Int GetTileInFront()
    {
        return TilePosition + HeadingDirection(TileHeading);
    }

    public override void OnDeath(Object killer)
    {
        base.OnDeath(killer);

        GameManager.OnPlayerDied();
    }
}
