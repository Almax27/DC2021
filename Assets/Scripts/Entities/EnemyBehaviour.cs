using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpriteMusicAnimation
{
    public Sprite[] spriteFrames = new Sprite[0];

    [Tooltip("Frame to start the animation - use this to align motion with music")]
    public int StartFrame = 0;

    [Tooltip("Speed of the animation in frames per beat")]
    public float FramesPerBeat = 1.0f;
}

public enum EnemyState
{
    Idle,
    AttackTell,
    Attacking,
    EnterDefend,
    ExitDefend,
    Defending
}

[System.Serializable]
public class EnemyStateFlow
{
    public List<EnemyState> States = new List<EnemyState>();
}

public class EnemyBehaviour : WorldAgentBehaviour
{
    public SpriteRenderer spriteRenderer;

    WorldPathFinder pathFinder = new WorldPathFinder();

    public List<EnemyStateFlow> combatStateFlows = new List<EnemyStateFlow>();

    public SpriteMusicAnimation idleAnimation = new SpriteMusicAnimation();
    public SpriteMusicAnimation attackTellAnimation = new SpriteMusicAnimation();
    public SpriteMusicAnimation attackAnimation = new SpriteMusicAnimation();
    public SpriteMusicAnimation enterDefendAnimation = new SpriteMusicAnimation();
    public SpriteMusicAnimation defendAnimation = new SpriteMusicAnimation();
    public SpriteMusicAnimation exitDefendAnimation = new SpriteMusicAnimation();

    int combatFlowIndex;
    int combatStateIndex;

    int lastSpriteFrameIndex = 0;

    public int beatsPerMove = 2;
    int beatCounter = 0;

    private void Start()
    {
        Initialise();

        Debug.Assert(spriteRenderer, "No spriteRenderer set!");

        GameManager.onBeatEvent.AddListener(OnBeat);

        StartCoroutine(pathFinder.CalculateClosestValid(GameManager.WorldGen, new Vector2Int((int)transform.position.x, (int)transform.position.z), results =>
        {
            if (results.Count > 0)
            {
                WarpTo(results[results.Count - 1], 0);
            }
        }));
    }

    [ContextMenu("Initialise")]
    void Initialise()
    {
        foreach (var spriteRenderer in GetComponentsInChildren<SpriteRenderer>())
        {
            spriteRenderer.receiveShadows = true;
            spriteRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;
        }
    }

    void OnBeat()
    {
        if (GameManager.World == null || GameManager.Player == null)
            return;

        beatCounter++;

        if (beatCounter % beatsPerMove != 0)
            return;

        StartCoroutine(pathFinder.CalculatePathTo(GameManager.WorldGen, TilePosition, GameManager.Player.TilePosition, results =>
        {
            int distanceFromTarget = 1;
            if (results.Count > 1 + distanceFromTarget)
            {
                MoveTo(results[1]);
            }
        }));
    }

    protected override void Update()
    {
        base.Update();

        UpdateCombat();
    }

    void UpdateCombat()
    {
        if (combatFlowIndex >= 0 && combatFlowIndex < combatStateFlows.Count)
        {
            var combatFlow = combatStateFlows[combatFlowIndex];
            if (combatStateIndex >= 0 && combatStateIndex < combatFlow.States.Count)
            {
                var currentState = combatFlow.States[combatStateIndex];
                if (RunAnimation(GetAnimationForState(currentState)))
                {
                    combatStateIndex++;
                    UpdateCombat();
                }
            }
            else if(combatFlow.States.Count > 0)
            {
                combatStateIndex = 0;
                combatFlowIndex++;
                UpdateCombat();
            }
        }
        else if(combatStateFlows.Count > 0)
        {
            combatStateIndex = 0;
            combatFlowIndex = Random.Range(0, combatStateFlows.Count - 1);
            UpdateCombat();
        }
    }

    int GetNumberOfStates()
    {
        return System.Enum.GetNames(typeof(EnemyState)).Length;
    }

    SpriteMusicAnimation GetAnimationForState(EnemyState state)
    {
        switch (state)
        {
            case EnemyState.Idle:
                return idleAnimation;
            case EnemyState.AttackTell:
                return attackTellAnimation;
            case EnemyState.Attacking:
                return attackAnimation;
            case EnemyState.EnterDefend:
                return enterDefendAnimation;
            case EnemyState.Defending:
                return defendAnimation;
            case EnemyState.ExitDefend:
                return exitDefendAnimation;
        }
        return null;
    }

    bool RunAnimation(SpriteMusicAnimation animation)
    {
        //animation.spriteFrames
        if (animation != null && GameManager && spriteRenderer)
        {
            int spriteIndex = Mathf.FloorToInt(GameManager.MusicTime * animation.FramesPerBeat) % animation.spriteFrames.Length;
            if (spriteIndex >= 0)
            {
                spriteRenderer.sprite = animation.spriteFrames[spriteIndex % animation.spriteFrames.Length];
            }

            if(spriteIndex < lastSpriteFrameIndex)
            {
                lastSpriteFrameIndex = 0;
                return true; //return true when loop detected
            }
            else
            {
                lastSpriteFrameIndex = spriteIndex;
                return false;
            }
        }
        return true;
    }
}