using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class LookAtBehaviour : MonoBehaviour
{
    public bool lockToY = false;

#if UNITY_EDITOR
    void OnRenderObject()
    {
        LookAt(SceneView.lastActiveSceneView.camera);
    }
#endif

    private void LateUpdate()
    {
        LookAt(Camera.main);
    }

    void LookAt(Camera camera)
    {
        if (camera == null) return;

        if (lockToY)
        {
            Vector3 dir = camera.transform.position - transform.position;
            Vector3 rot = Quaternion.LookRotation(dir.normalized).eulerAngles;
            transform.rotation = Quaternion.Euler(0, rot.y, 0);
        }
        else
        {
            transform.LookAt(camera.transform);
        }
    }
}
