using UnityEngine;

public class EntityBehaviour : MonoBehaviour
{

    private void Awake()
    {
        Initialise();
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
}
