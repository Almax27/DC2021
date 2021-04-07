using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

[RequireComponent(typeof(Light)), ExecuteInEditMode]
public class FlickeringLightBehaviour : MonoBehaviour
{
    public GameManager gameManager;

    public MinMaxFloat intensityRange = new MinMaxFloat(0,1);
    public float intensitySmoothTime = 0.1f;

    Light lightToFlicker;
    float intensityVelocity;

    private void Awake()
    {
        lightToFlicker = GetComponent<Light>();
        gameManager = FindObjectOfType<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if(lightToFlicker && gameManager)
        {
            float tval = gameManager.MusicTime % 1.0f;
            tval = tval < 0.5f ? tval * 2.0f : (1.0f - (tval - 0.5f) * 2.0f);
            tval = 1.0f - tval* tval;

            lightToFlicker.intensity = intensityRange.min + (tval)* (intensityRange.max - intensityRange.min);
        }
    }
}
