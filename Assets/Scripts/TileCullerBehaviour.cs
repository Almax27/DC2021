using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class TileCullerBehaviour : MonoBehaviour
{
    GameManager gameManager;

    public float cullDistance = 30.0f;

    int tileIndex = 0;

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    private void LateUpdate()
    {
        if (gameManager.World == null) return;

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
            bool inRange = (gameManager.Player.transform.position - go.transform.position).sqrMagnitude < cullDistSq;
            if (inRange != go.activeSelf)
            {
                go.SetActive(inRange);
            }
        }

        tileIndex = 0;
    }
}
