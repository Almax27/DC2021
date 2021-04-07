using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.EditorCoroutines.Editor;

[CustomEditor(typeof(MinimapBehaviour))]
// ^ This is the script we are making a custom editor for.
public class MinimapBehaviourEditor : Editor
{
    WorldGeneratorBehaviour worldGen = null;

    private void OnSceneGUI()
    {
        var behaviour = target as MinimapBehaviour;
        if (behaviour == null) return;

        var worldGen = FindObjectOfType<WorldGeneratorBehaviour>();

        if (worldGen.World == null) return;
    }
}
