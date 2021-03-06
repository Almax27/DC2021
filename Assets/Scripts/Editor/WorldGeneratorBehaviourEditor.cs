using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.EditorCoroutines.Editor;

[CustomEditor(typeof(WorldGeneratorBehaviour))]
 // ^ This is the script we are making a custom editor for.
 public class WorldGeneratorBehaviourEditor : Editor
{
    Tool LastTool = Tool.None;
    bool wasGenerating = false;

    public void StartGeneration(WorldGeneratorBehaviour behaviour)
    {
        behaviour.Generate();
    }

    public void OnSceneGUI()
    {
        Tools.current = Tool.None;
    }

    void OnEnable()
    {
        LastTool = Tools.current;
        Tools.current = Tool.None;
    }

    void OnDisable()
    {
        Tools.current = LastTool;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        WorldGeneratorBehaviour behaviour = target as WorldGeneratorBehaviour;

        //Repaint world
        bool isGenerating = behaviour.worldGenerator.IsRunning;
        if (isGenerating || isGenerating != wasGenerating || behaviour.pathFinder.IsProcessing)
        {
            wasGenerating = isGenerating;
            EditorWindow.GetWindow<SceneView>().Repaint();
        }

        GUILayout.BeginHorizontal();
        {
            GUILayout.Label("Speed:", GUILayout.ExpandWidth(false));
            behaviour.worldGenerator.Speed = GUILayout.HorizontalSlider(behaviour.worldGenerator.Speed, 0.0f, 1, GUILayout.Width(50));
            if (!behaviour.worldGenerator.IsRunning)
            {
                if (GUILayout.Button("Generate"))
                {
                    StartGeneration(behaviour);
                }
                if (GUILayout.Button("Randomise", GUILayout.ExpandWidth(false)))
                {
                    behaviour.seed = Random.Range(int.MinValue, int.MaxValue);
                    StartGeneration(behaviour);
                }
            }
            else
            {
                if (GUILayout.Button($"(Cancel) {behaviour.worldGenerator.Status}"))
                {
                    behaviour.CancelGeneration();
                }
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginVertical();
        if(behaviour.World != null)
        {
            if (!behaviour.pathFinder.IsProcessing)
            {
                if (GUILayout.Button("Pathfind"))
                {
                    behaviour.pathResults.Clear();

                    WorldPathFinder.DebugRenderCallback debugCallback = () =>
                    {
                        EditorWindow.GetWindow<SceneView>().Repaint();
                    };

                    EditorCoroutineUtility.StartCoroutine(behaviour.pathFinder.CalculatePathTo(behaviour, behaviour.pathStart, behaviour.pathEnd, 
                    (results, reachedGoal) =>
                    {
                        Debug.Log($"Path result: count={results.Count}");
                        behaviour.pathResults = results;
                    },
                    behaviour.debugPathing ? debugCallback : null), this);
                }
            }
            else
            {
                GUILayout.Label("Pathfinding...", GUILayout.ExpandWidth(false));
            }
        }
        GUILayout.EndVertical();
    }
}
