using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDBehaviour : MonoBehaviour
{
    public Image musicPulsePrefab;
    public int numberOfPulses = 5;

    public Vector2 minimapSize = new Vector2(100,100);
    public Vector2 minimapInset = new Vector2(20, 20);
    public Color minimapBackgroundColor = new Color(0, 0, 0, 0.2f);
    
    public int minimapTileSize = 5;
    public int minimapTilePadding = 1;
    
    public Texture2D minimapTileTexture;
    public Color minimapTileColor = new Color(1, 1, 1, 1);
    public Color minimapTileUnexporedColor = new Color(0.5f, 0.5f, 0.5f, 1);

    public Texture2D minimapPlayerTexture;
    public Color minimapPlayerColor = new Color(0, 0, 1, 1);
   
    public Texture2D minimapEnemyTexture;
    public Color minimapEnemyColor = new Color(1, 0, 0, 1);

    public Texture2D minimapObjectiveTexture;
    public Color minimapObjectiveColor = new Color(0, 1, 0, 1);
    
    public Vector2Int heartInset = new Vector2Int(20,20);
    public Vector2Int heartSize = new Vector2Int(60,60);
    public int heartSpacing = 5;
    public Texture2D fullHeartTexture;
    public Texture2D emptyHeartTexture;

    public Texture2D damageOverlayTexture;

    public GUIStyle helpTextStyle = new GUIStyle();

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
            //left.rectTransform.pivot = new Vector2(1, 0);
            leftMusicPulses.Add(left);

            var right = Instantiate<GameObject>(musicPulsePrefab.gameObject, transform).GetComponent<Image>();
            //right.rectTransform.pivot = new Vector2(0, 0);
            rightMusicPulses.Add(right);
        }

        StartCoroutine(FadeOut(false));
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

            leftMusicPulses[i].rectTransform.anchoredPosition = new Vector2(-(invBeatTime + i) * beatWidth, 30);
            rightMusicPulses[i].rectTransform.anchoredPosition = new Vector2((invBeatTime + i) * beatWidth, 30);
        }

    }

    void OnGUI()
    {
        if (!gameManager || gameManager.World == null) return;

        if (!minimapTileTexture) minimapTileTexture = Texture2D.whiteTexture;
        if (!minimapPlayerTexture) minimapPlayerTexture = Texture2D.whiteTexture;
        if (!minimapEnemyTexture) minimapEnemyTexture = Texture2D.whiteTexture;
        if (!minimapObjectiveTexture) minimapObjectiveTexture = Texture2D.whiteTexture;

        DrawDamageOverlayGUI();

        DrawHelpText();

        DrawPlayerGUI();

        if (fadeValue != 0)
        {
            GUI.color = new Color(0, 0, 0, fadeValue);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        }
    }

    void DrawPlayerGUI()
    {
        if (gameManager.Player == null) return;

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
                if (tile.type == WorldTileType.None || !tile.explored)
                {
                    continue;
                }

                Vector2 playerRelativePos = p - playerTile;
                tileRect.x = (int)minimapArea.width / 2 + (playerRelativePos.x + playerOffset.x) * (tileRect.width + minimapTilePadding);
                tileRect.y = (int)minimapArea.width / 2 - (playerRelativePos.y + playerOffset.y) * (tileRect.height + minimapTilePadding);

                GUI.color = world.HasUnexporedNeighbors(tileIndex) ? minimapTileUnexporedColor : minimapTileColor;
                GUI.DrawTexture(tileRect, minimapTileTexture, ScaleMode.ScaleToFit);

                if (tile.agent is PlayerBehaviour)
                {
                    var color = minimapPlayerColor;
                    color.a = gameManager.MusicTime % 1.0f < 0.3f ? 0.2f : 1.0f;

                    var rotation = tile.agent.transform.rotation.eulerAngles.y;

                    GUI.color = color;
                    GUIUtility.RotateAroundPivot(rotation, tileRect.center);
                    GUI.DrawTexture(tileRect, minimapPlayerTexture, ScaleMode.ScaleToFit);
                    GUI.matrix = Matrix4x4.identity;
                }
                else if (tile.agent is EnemyBehaviour)
                {
                    GUI.color = minimapEnemyColor;
                    GUI.DrawTexture(tileRect, minimapEnemyTexture, ScaleMode.ScaleToFit);
                }
                else if (tile.interactable is ObjectiveInteractableBehaviour)
                {
                    var color = minimapObjectiveColor;
                    color.a = gameManager.MusicTime % 1.0f < 0.3f ? 0.2f : 1.0f;
                    GUI.color = color;
                    GUI.DrawTexture(tileRect, minimapObjectiveTexture, ScaleMode.ScaleToFit);
                }
            }

        }
        GUI.EndClip();

        GUI.color = Color.white;
        Rect heartRect = new Rect(heartInset, heartSize);
        for (int i = 0; i < gameManager.Player.maxHealth; i++)
        {
            Texture2D heartTexture = emptyHeartTexture;
            if (i < gameManager.Player.Health)
            {
                heartTexture = fullHeartTexture;
            }
            GUI.DrawTexture(heartRect, heartTexture, ScaleMode.ScaleToFit);
            heartRect.x += heartSize.x + heartSpacing;
        }
    }

    void DrawDamageOverlayGUI()
    {
        const float fadeTime = 0.3f;
        if(gameManager.Player && gameManager.Player.LastDamagedTime != 0)
        {
            float timeSinceLastDamaged = Time.time - gameManager.Player.LastDamagedTime;
            if(timeSinceLastDamaged < fadeTime)
            {
                float tval = 1.0f - (timeSinceLastDamaged / fadeTime);
                GUI.color = new Color(1, 1, 1, tval);
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), damageOverlayTexture);
            }
        }
    }

    float helpFadeTime = 1.5f;
    void DrawHelpText()
    {
        if (helpTextToShow.Length > 0)
        {
            float timeSinceTextStarted = Time.time - helpTextStartTime;
            if (timeSinceTextStarted < helpFadeTime)
            {
                float tval = 1.0f - Mathf.Clamp01((timeSinceTextStarted* timeSinceTextStarted) / helpFadeTime);
                GUI.color = new Color(1, 1, 1, tval);
                GUI.Label(new Rect(Screen.width / 2 - 50, Screen.height / 2 - 25, 100, 50), helpTextToShow, helpTextStyle);
            }
        }
    }

    string helpTextToShow = "";
    float helpTextStartTime;

    public void ShowText(string text, float duration = 2.5f)
    {
        helpFadeTime = duration;
        helpTextToShow = text;
        helpTextStartTime = Time.time;
    }

    float fadeValue = 0;

    public IEnumerator FadeOut(bool fadeOut)
    {
        const float duration = 1.5f;
        fadeValue = fadeOut ? 0 : 1;
        while (true)
        {
            fadeValue += (fadeOut ? 1 : -1) * (Time.deltaTime / duration);
            if(fadeValue < 0 || fadeValue > 1)
            {
                fadeValue = 0;
                break;
            }
            yield return null;
        }
    }
}
