using UnityEngine;

public class WorldAgentBehaviour : MonoBehaviour
{
    public GameManager GameManager { get; private set; }
    public Vector2Int TilePosition { get; private set; }
    public int TileHeading { get; private set; }

    public int Health { get; private set; }

    public bool IsAttacking{ get; protected set; }
    public bool IsDefending { get; protected set; }

    protected Vector3 moveVelocity;
    protected float turnVelocity;

    public int maxHealth = 3;
    public bool destroyOnDeath = true;
    public DeathNugget[] deathNuggetPrefabs = new DeathNugget[0];

    public float moveSmoothTime = 0.1f;
    public float moveMaxSpeed = 10.0f;

    public float turnSmoothTime = 0.05f;
    public float turnMaxSpeed = 360.0f;

    public Vector3 tileOffset = Vector3.zero;

    protected virtual void Awake()
    {
        if (GameManager == null) GameManager = FindObjectOfType<GameManager>();
        Health = maxHealth;
    }

    Vector3 PosInTile()
    {
        Vector3 p = new Vector3(TilePosition.x + 0.5f, tileOffset.y, TilePosition.y + 0.5f);
        Vector2Int forwardDir = HeadingDirection(TileHeading);
        p.x += tileOffset.z * forwardDir.x;
        p.z += tileOffset.z* forwardDir.y;

        Vector2Int rightDir = HeadingDirection(TileHeading + 1);
        p.x += tileOffset.x * rightDir.x;
        p.z += tileOffset.x * rightDir.y;

        return p;
    }

    bool IsWalkable(Vector2Int targetPos)
    {
        var tile = GameManager.World.TileAtPostion(targetPos.x, targetPos.y);
        if (tile.type == WorldTileType.None) return false;

        foreach(var otherAgent in FindObjectsOfType<WorldAgentBehaviour>())
        {
            if (otherAgent.TilePosition == targetPos)
            {
                return false; //another agent occupies this spot
            }
        }

        return true;
    }

    public bool WarpTo(Vector2Int targetPos, int heading = 0)
    {
        if (!IsWalkable(targetPos)) return false;

        TilePosition = targetPos;
        TileHeading = heading;

        transform.SetPositionAndRotation(PosInTile(), Quaternion.Euler(0, RotationFromHeading(TileHeading), 0));

        return true;
    }

    public bool MoveTo(Vector2Int targetPos)
    {
        if (!IsWalkable(targetPos)) return false;
        TilePosition = targetPos;
        return true;
    }

    public bool MoveBy(Vector2Int delta)
    {
        return MoveTo(TilePosition + delta);
    }

    public bool MoveForward()
    {
        return MoveTo(TilePosition + HeadingDirection(TileHeading));
    }

    public bool MoveBackwards()
    {
        return MoveTo(TilePosition - HeadingDirection(TileHeading));
    }

    public void TurnClockwise()
    {
        TileHeading++;
    }

    public void TurnAntiClockwise()
    {
        TileHeading--;
    }

    protected virtual void Update()
    {
        Vector3 newPosition = Vector3.SmoothDamp(transform.position, PosInTile(), ref moveVelocity, moveSmoothTime, moveMaxSpeed);

        float newHeading = Mathf.SmoothDampAngle(transform.rotation.eulerAngles.y, RotationFromHeading(TileHeading), ref turnVelocity, turnSmoothTime, turnMaxSpeed);
        Quaternion newRotation = Quaternion.Euler(0, newHeading, 0);

        transform.SetPositionAndRotation(newPosition, newRotation);
    }

    public bool IsDead()
    {
        return Health <= 0;
    }

    public void TakeDamage(int damage, Object source)
    {
        if (IsDead()) return;

        Debug.Log($"{source} dealt {damage} damage to {this.name}");

        Health -= damage;

        if(IsDead())
        {
            Debug.Log($"{this.name} died!");

            OnDeath();

            foreach(var nugget in deathNuggetPrefabs)
            {
                if(nugget) Instantiate(nugget, this.transform.position, this.transform.rotation);
            }

            if(destroyOnDeath)
            {
                Destroy(gameObject);
            }
        }
    }

    public virtual void OnDeath()
    {

    }

    #region static methods

    public static float RotationFromHeading(int heading)
    {
        return 90.0f * (int)heading;
    }

    public static int BestHeadingForDirection(Vector2 direction)
    {
        float angle = Vector2.Angle(direction, Vector2.up) % 360.0f;
        int cardinal = (int)((angle + 45.0f) / 90);
        return cardinal;
    }

    public static Vector2Int HeadingDirection(int heading)
    {
        heading = ((heading %= 4) < 0) ? heading + 4 : heading;
        switch (heading)
        {
            case 0:   return Vector2Int.up;
            case 1:    return Vector2Int.right;
            case 2:   return Vector2Int.down;
            case 3:    return Vector2Int.left;
        }
        return Vector2Int.zero;
    }
    #endregion
}