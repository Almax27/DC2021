using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class WorldPathFinder
{
    public delegate void ResultCallback(List<Vector2Int> tiles, bool reachedGoal);
    public delegate bool GoalEvaluator(Vector2Int current);
    public delegate float HeurisicCalculator(Vector2Int a);
    public delegate void DebugRenderCallback();

    static Vector2Int[] s_neighborOffsets = new Vector2Int[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
    SortedList<float, Vector2Int> openSet = new SortedList<float, Vector2Int>(Comparer<float>.Create((a, b) => (a > b) ? -1 : 1));
    Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>(0);
    Dictionary<Vector2Int, float> gScore = new Dictionary<Vector2Int, float>(0);
    Dictionary<Vector2Int, float> fScore = new Dictionary<Vector2Int, float>(0);

    public IList<Vector2Int> OpenSet => openSet.Values;

    public bool IsProcessing { get; private set; }

    public bool IsWalkable(WorldGeneratorBehaviour worldGen, Vector2Int pos)
    {
        var tile = worldGen.World.TileAtPostion(pos);
        return tile.type != WorldTileType.None && (!tile.interactable || !tile.interactable.IsBlocking);
    }

    public IEnumerator CalculatePathTo(WorldGeneratorBehaviour worldGen, Vector2Int start, Vector2Int goal, ResultCallback callback, DebugRenderCallback debugRenderCallback = null)
    {
        yield return Calculate( worldGen, start, 
                                a => a == goal, 
                                a => Vector2Int.Distance(a, goal), 
                                a => IsWalkable(worldGen, a) ? 1.0f : -1.0f, 
                                callback, debugRenderCallback);
    }

    public IEnumerator CalculateClosestValid(WorldGeneratorBehaviour worldGen, Vector2Int start, ResultCallback callback, DebugRenderCallback debugRenderCallback = null)
    {
        yield return Calculate(worldGen, start, a => IsWalkable(worldGen, a), a => Vector2Int.Distance(a, start), a => 1.0f, callback, debugRenderCallback);
    }

    public IEnumerator Calculate(WorldGeneratorBehaviour worldGen, Vector2Int start, GoalEvaluator goalEvaluator, HeurisicCalculator heurisicCalculator, HeurisicCalculator weightCalculator, ResultCallback callback, DebugRenderCallback debugRenderCallback = null)
    {
        if(worldGen == null)
        {
            UnityEngine.Debug.LogError("Invalid WorldGeneratorBehaviour!");
            yield break;
        }
        if(goalEvaluator == null)
        {
            UnityEngine.Debug.LogError("Invalid goalEvaluator!");
            yield break;
        }
        if(IsProcessing)
        {
            UnityEngine.Debug.LogError("Path finder in progress!");
            yield break;
        }

        IsProcessing = true;

        Stopwatch totalStopwatch = new Stopwatch();
        totalStopwatch.Start();
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        openSet.Clear();
        cameFrom.Clear();
        gScore.Clear();
        fScore.Clear();

        while(worldGen.World == null)
        {
            yield return null;
        }

        foreach (var p in worldGen.World.Bounds.allPositionsWithin)
        {
            gScore.Add(p, float.PositiveInfinity);
            fScore.Add(p, float.PositiveInfinity);
        }

        gScore[start] = 0.0f;
        fScore[start] = heurisicCalculator(start);

        openSet.Add(fScore[start], start);

        Vector2Int current = new Vector2Int();

        while (openSet.Count > 0)
        {
            int currentIndex = openSet.Count - 1;
            current = openSet.Values[openSet.Count - 1];

            if (goalEvaluator(current))
            {
                callback.Invoke(CalculateFinalPath(current), true);
                IsProcessing = false;
                //UnityEngine.Debug.Log($"Pathing complete: distance={finalPath.Count}, time={totalStopwatch.Elapsed.TotalSeconds:F2}");
                yield break;
            }

            openSet.RemoveAt(currentIndex);

            for(int i = 0; i < s_neighborOffsets.Length; i++)
            {
                Vector2Int neighbor = current + s_neighborOffsets[i];

                if (!worldGen.World.IsTilePosValid(neighbor.x, neighbor.y)) continue;

                float weight = weightCalculator(neighbor);
                if (weight < 0) continue;

                float tentitiveScore = gScore[current] + weight;
                if(tentitiveScore < gScore[neighbor])
                {
                    //This path is better than any previous ones
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentitiveScore;

                    float costToReachGoal = heurisicCalculator(neighbor);
                    fScore[neighbor] = gScore[neighbor] + costToReachGoal;
                    if(!openSet.ContainsValue(neighbor))
                    {
                        openSet.Add(fScore[neighbor], neighbor);
                    }
                }
            }

            if (stopwatch.Elapsed.TotalMilliseconds > 2.0f)
            {
                //UnityEngine.Debug.Log($"Pathing: time={stopwatch.Elapsed.TotalSeconds:F2}");
                yield return null;
                stopwatch.Restart();
            }
#if UNITY_EDITOR
            if (debugRenderCallback != null)
            {
                debugRenderCallback.Invoke();
            }
#endif
        }

        //return the best path
        callback.Invoke(new List<Vector2Int>(), false);

        IsProcessing = false;

        //UnityEngine.Debug.Log($"Pathing failed: time={totalStopwatch.Elapsed.TotalSeconds:F2}");
    }

    List<Vector2Int> CalculateFinalPath(Vector2Int current)
    {
        List<Vector2Int> finalPath = new List<Vector2Int>() { current };
        while (cameFrom.TryGetValue(current, out Vector2Int prev))
        {
            current = prev;
            finalPath.Add(prev);
        }
        finalPath.Reverse();
        return finalPath;
    }
}
