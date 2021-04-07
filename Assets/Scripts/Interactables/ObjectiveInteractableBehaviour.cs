using UnityEngine;

public class ObjectiveInteractableBehaviour : InteractableBehaviour
{
    GameManager gameManager;
    public Sprite[] pickupSprites = new Sprite[0];

    public SpriteRenderer pickupSpriteRenderer = null;

    Vector3 startLocalPos;

    private void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
        if (pickupSprites.Length > 0)
        {
            pickupSpriteRenderer.sprite = pickupSprites[Random.Range(0, pickupSprites.Length)];
        }
        startLocalPos = pickupSpriteRenderer.transform.localPosition;
    }

    public override void OnInteract()
    {
        base.OnInteract();

        gameManager.OnObjectiveComplete();

        gameManager.World.tiles[gameManager.World.TileIndex(Tile)].interactable = null;

        Destroy(pickupSpriteRenderer.gameObject);
    }
    float bob;
    Vector3 vel;
    public void Update()
    {
        if (pickupSpriteRenderer)
        {
            bob = (gameManager.MusicTime) % 2.0f;
            pickupSpriteRenderer.transform.localPosition = Vector3.SmoothDamp(pickupSpriteRenderer.transform.localPosition,
                                                                                startLocalPos + new Vector3(0, bob > 1 ? 0.2f : 0, 0),
                                                                                ref vel,
                                                                                0.3f);
        }
    }
}