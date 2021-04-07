using UnityEngine;

public class InteractableBehaviour : MonoBehaviour
{
    public bool IsBlocking = false;
    public Vector2Int Tile;

    public virtual void OnInteract()
    {

    }
}
