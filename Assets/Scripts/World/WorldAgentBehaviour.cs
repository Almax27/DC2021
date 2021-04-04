using UnityEngine;

public class WorldAgentBehaviour : MonoBehaviour
{
    protected GameManager gameManager;
    protected Vector2Int tilePosition;
    protected int tileHeading;

    protected Vector3 moveVelocity;
    protected float turnVelocity;

    public float moveSmoothTime = 0.1f;
    public float moveMaxSpeed = 10.0f;

    public float turnSmoothTime = 0.05f;
    public float turnMaxSpeed = 360.0f;

    public float offsetInTile = -0.25f;

    void OnSpawn(GameManager _gameManager)
    {
        gameManager = _gameManager;
    }

    Vector3 PosInTile()
    {
        Vector3 p = new Vector3(tilePosition.x + 0.5f, 0.5f, tilePosition.y + 0.5f);
        Vector2Int dir = HeadingDirection(tileHeading);
        p.x += offsetInTile * dir.x;
        p.z += offsetInTile * dir.y;
        return p;
    }

    bool IsWalkable(Vector2Int targetPos)
    {
        var tile = gameManager.World.TileAtPostion(targetPos.x, targetPos.y);
        return tile.type != WorldTileType.None;
    }

    public void WarpTo(Vector2Int targetPos, int heading)
    {
        if (!IsWalkable(targetPos)) return;

        tilePosition = targetPos;
        tileHeading = heading;

        transform.SetPositionAndRotation(PosInTile(), Quaternion.Euler(0, RotationFromHeading(tileHeading), 0));
    }

    public void MoveTo(Vector2Int targetPos)
    {
        if (!IsWalkable(targetPos)) return;

        tilePosition = targetPos;
    }

    public void MoveBy(Vector2Int delta)
    {
        if (!IsWalkable(tilePosition + delta)) return;
        tilePosition += delta;
    }

    public void MoveForward()
    {
        Vector2Int newPos = tilePosition + HeadingDirection(tileHeading);
        if (!IsWalkable(newPos)) return;
        tilePosition = newPos;
    }

    public void MoveBackwards()
    {
        Vector2Int newPos = tilePosition - HeadingDirection(tileHeading);
        if (!IsWalkable(newPos)) return;
        tilePosition = newPos;
    }

    public void TurnClockwise()
    {
        tileHeading++;
    }

    public void TurnAntiClockwise()
    {
        tileHeading--;
    }

    protected virtual void Update()
    {
        Vector3 newPosition = Vector3.SmoothDamp(transform.position, PosInTile(), ref moveVelocity, moveSmoothTime, moveMaxSpeed);

        float newHeading = Mathf.SmoothDampAngle(transform.rotation.eulerAngles.y, RotationFromHeading(tileHeading), ref turnVelocity, turnSmoothTime, turnMaxSpeed);
        Quaternion newRotation = Quaternion.Euler(0, newHeading, 0);

        transform.SetPositionAndRotation(newPosition, newRotation);
    }

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
}