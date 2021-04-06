using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class TileCullerBehaviour : MonoBehaviour
{
    GameManager gameManager;

    public bool debugDisable = false;
    public float cullDistance = 30.0f;

    int tileIndex = 0;

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        debugDisable = false;
    }

    private void Update()
    {
        if (debugDisable || gameManager.WorldGen == null) return;

        Vector3 camPos = new Vector3();
        if (Camera.main)
        {
            camPos = Camera.main.transform.position;
        }
        else return;

        float cullDistSq = cullDistance * cullDistance;

        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        for (; tileIndex < gameManager.WorldGen.GeneratedObjects.Count; tileIndex++)
        {
            if (stopwatch.Elapsed.TotalMilliseconds > 2.0f)
            {
                return;
            }

            var go = gameManager.WorldGen.GeneratedObjects[tileIndex];
            if (go == null) continue;

            bool inRange = (camPos - go.transform.position).sqrMagnitude < cullDistSq;
            if (inRange != go.activeSelf)
            {
                go.SetActive(inRange);
            }
        }

        tileIndex = 0;
    }
}
