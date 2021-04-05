using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDBehaviour : MonoBehaviour
{
    public Image musicPulsePrefab;
    public readonly int numberOfPulses = 5;

    public float debugStartTimeDelay = 0.0f;

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
        float startTimeDelay = debugStartTimeDelay;
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
}
