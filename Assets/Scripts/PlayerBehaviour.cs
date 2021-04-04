using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum PlayerAction
{
    None,
    MoveForward,
    MoveBack,
    TurnLeft,
    TurnRight,
    Attack,
    Defend
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
    public int beatsToSkipOnMiss = 1;
    int beatsToSkip = 0;

    PlayerAction pendingAction = PlayerAction.None;
    PlayerActionAccuracy pendingActionAccuracy = PlayerActionAccuracy.Miss;
    bool graceUsed = false;

    private void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
        gameManager.onBeatEvent.AddListener(OnBeat);
        Debug.Assert(gameManager, "Failed to find GameManager");
    }

    void OnBeat()
    {
        TryConsumePendingAction();
        beatsToSkip--;
    }

    void TryConsumePendingAction()
    {
        if (pendingActionAccuracy == PlayerActionAccuracy.Miss)
            return;

        switch (pendingAction)
        {
            case PlayerAction.MoveForward:
                MoveForward();
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
            case PlayerAction.Attack:
                break;
            case PlayerAction.Defend:
                break;
        }
        pendingAction = PlayerAction.None;
    }

    void RequestAction(PlayerAction action)
    {
        float grace = 0.1f;

        if (graceUsed && gameManager.MusicTime < grace)
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

        float beatTime = gameManager.MusicTime % 1.0f;
        if (beatTime < grace)
        {
            graceUsed = true;
            pendingActionAccuracy = PlayerActionAccuracy.Perfect;
            TryConsumePendingAction();
        }
        else if (beatTime < 0.5f)
        {
            pendingActionAccuracy = PlayerActionAccuracy.Miss;
            beatsToSkip = beatsToSkipOnMiss + 1;
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

        Debug.Log($"{pendingAction} : {pendingActionAccuracy} : {beatTime:F2}" + (graceUsed ? " : (grace)" : ""));
    }

        // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    protected override void Update()
    {
        if (gameManager.World == null) return;

        if(Input.GetKeyDown(KeyCode.UpArrow))
        {
            RequestAction(PlayerAction.MoveForward);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            RequestAction(PlayerAction.MoveBack);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            RequestAction(PlayerAction.TurnLeft);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            RequestAction(PlayerAction.TurnRight);
        }

        base.Update();
    }
}
