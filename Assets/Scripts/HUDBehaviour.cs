using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDBehaviour : MonoBehaviour
{
    public Image musicPulsePrefab;
    public readonly int numberOfPulses = 5;

    public Vector2 minimapSize = new Vector2(100,100);
    public Vector2 minimapInset = new Vector2(20, 20);
    public Color minimapBackgroundColor = new Color(0, 0, 0, 0.2f);
    
    public int minimapTileSize = 5;
    public int minimapTilePadding = 1;
    public Color minimapTileColor = new Color(1, 1, 1, 1);
    public Color minimapTileUnexporedColor = new Color(0.5f, 0.5f, 0.5f, 1);
    public Color minimapPlayerColor = new Color(0, 0, 1, 1);
    public Color minimapPlayerHeadingColor = new Color(0, 0, 1, 1);
    public Color minimapEnemyColor = new Color(1, 0, 0, 1);
    public Color minimapObjectiveColor = new Color(0, 1, 0, 1);
    public Texture2D minimapTileTexture;
    public Texture2D minimapPlayerTexture;

    GameManager gameManager;

    RectTransform hudTransform;

    List<Image> leftMusicPulses = new List<Image>();
    List<Image> rightMusicPulses = new List<Image>();

    // Start is called before the first frame update
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        hudTransform = GetComponent<RectTransform>();

        leftMusicPulses.Capacity = numberOfPulses;
        rightMusicPulses.Capacity = numberOfPulses;
        for (int i = 0; i < numberOfPulses; i++)
        {
            var left = Instantiate<GameObject>(musicPulsePrefab.gameObject, transform).GetComponent<Image>();
            left.rectTransform.pivot = new Vector2(1, 0);
            leftMusicPulses.Add(left);

            var right = Instantiate<GameObject>(musicPulsePrefab.gameObject, transform).GetComponent<Image>();
            right.rectTransform.pivot = new Vector2(0, 0);
            rightMusicPulses.Add(right);
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        float beatTime = gameManager.MusicTime % 1.0f;
        float invBeatTime = 1.0f - beatTime;

        float beatWidth = hudTransform.rect.width / (2 * numberOfPulses);
        for (int i = 0; i < numberOfPulses; i++)
        {
            float timeRemaining = (invBeatTime + i);
            Color color = new Color(1, 1, 1, (numberOfPulses - 1) - timeRemaining);
            leftMusicPulses[i].color = color;
            rightMusicPulses[i].color = color;

            leftMusicPulses[i].rectTransform.anchoredPosition = new Vector2(-(invBeatTime + i) * beatWidth, 0);
            rightMusicPulses[i].rectTransform.anchoredPosition = new Vector2((invBeatTime + i) * beatWidth, 0);
        }

    }

    void OnGUI()
    {
        if (!gameManager || gameManager.World == null || gameManager.Player == null) return;

        if (!minimapTileTexture)
        {
            minimapTileTexture = Texture2D.whiteTexture;
        }

        var world = gameManager.World;

        float UIScale = Mathf.Min(Screen.width, Screen.height) / 1080.0f;

        Rect minimapArea = new Rect(Screen.width - (minimapSize.x + minimapInset.x) * UIScale,
                                    minimapInset.y * UIScale,
                                    minimapSize.x * UIScale,
                                    minimapSize.y * UIScale);

        GUI.color = minimapBackgroundColor;
        GUI.DrawTexture(minimapArea, minimapTileTexture, ScaleMode.ScaleToFit);

        int xTileCount = Mathf.CeilToInt(minimapSize.x / minimapTileSize);
        int yTileCount = Mathf.CeilToInt(minimapSize.y / minimapTileSize);

        GUI.BeginClip(minimapArea);
        {
            Vector3 playerPos = gameManager.Player.transform.position;
            Vector2Int playerTile = gameManager.Player.TilePosition;

            Vector2 playerOffset = playerTile - new Vector2(playerPos.x, playerPos.z);

            RectInt worldBounds = gameManager.World.Bounds;

            RectInt visibleArea = new RectInt(playerTile.x - xTileCount / 2, playerTile.y - yTileCount / 2, xTileCount, yTileCount);

            Rect tileRect = new Rect(0, 0, (int)(minimapTileSize * UIScale), (int)(minimapTileSize * UIScale));
            World.Tile tile;

            foreach (var p in visibleArea.allPositionsWithin)
            {
                int tileIndex = p.x + world.Bounds.width * p.y;
                tile = world.tiles[tileIndex];
                if(tile.type == WorldTileType.None || !tile.explored)
                {
                    continue;
                }

                Vector2 playerRelativePos = p - playerTile;
                tileRect.x = (int)minimapArea.width / 2 + (playerRelativePos.x + playerOffset.x) * (tileRect.width + minimapTilePadding);
                tileRect.y = (int)minimapArea.width / 2 - (playerRelativePos.y + playerOffset.y) * (tileRect.height + minimapTilePadding);

                Color color = minimapTileColor;
                bool flash = false;

                if (tile.agent is PlayerBehaviour)
                {
                    GUI.color = color;
                    GUI.DrawTexture(tileRect, minimapTileTexture, ScaleMode.ScaleToFit);

                    var rotation = PlayerBehaviour.RotationFromHeading(tile.agent.TileHeading);
                    rotation = tile.agent.transform.rotation.eulerAngles.y;

                    GUI.color = minimapPlayerColor;
                    GUIUtility.RotateAroundPivot(rotation, tileRect.center);
                    GUI.DrawTexture(tileRect, minimapPlayerTexture, ScaleMode.ScaleToFit);
                    GUI.matrix = Matrix4x4.identity;

                    continue;
                }
                else if (tile.agent is EnemyBehaviour)
                {
                    color = minimapEnemyColor;
                }
                else if (tile.interactable is ObjectiveInteractableBehaviour)
                {
                    color = minimapObjectiveColor;
                    flash = true;
                }
                else if(world.HasUnexporedNeighbors(tileIndex))
                {
                    color = minimapTileUnexporedColor;
                }

                if(flash)
                {
                    color.a = gameManager.MusicTime % 1.0f < 0.3f ? 0.2f : 1.0f;
                }

                GUI.color = color;
                GUI.DrawTexture(tileRect, minimapTileTexture, ScaleMode.ScaleToFit);
            }

        }
        GUI.EndClip();
        //gameManager.World
    }
}
