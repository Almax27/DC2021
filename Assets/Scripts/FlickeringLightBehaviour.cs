using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

[RequireComponent(typeof(Light)), ExecuteInEditMode]
public class FlickeringLightBehaviour : MonoBehaviour
{
    public MinMaxFloat intensityRange = new MinMaxFloat(0,1);
    public float intensitySmoothTime = 0.1f;

    Light lightToFlicker;
    float intensityVelocity;

    private void Awake()
    {
        lightToFlicker = GetComponent<Light>();    
    }

    // Update is called once per frame
    void Update()
    {
        if(lightToFlicker)
        {
            lightToFlicker.intensity = Mathf.SmoothDamp(lightToFlicker.intensity, intensityRange.Random, ref intensityVelocity, intensitySmoothTime);
        }
    }
}
