using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformAnimBehaviour : MonoBehaviour
{
    public Vector3 rotationRate;

    // Update is called once per frame
    void Update()
    {
        if(rotationRate.sqrMagnitude > 0)
        {
            transform.Rotate(rotationRate * Time.deltaTime);
        }
    }
}
