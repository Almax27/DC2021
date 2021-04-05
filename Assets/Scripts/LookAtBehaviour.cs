using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class LookAtBehaviour : MonoBehaviour, ITickable
{
    public bool lockToY = false;

    Renderer defaultRenderer;

#if UNITY_EDITOR
    void OnRenderObject()
    {
        if (!Application.isPlaying && SceneView.lastActiveSceneView)
        {
            LookAt(SceneView.lastActiveSceneView.camera);
        }
    }
#endif

    private void Awake()
    {
        defaultRenderer = GetComponentInChildren<Renderer>();
        TickManagerBehaviour.RegisterLate(this);
    }

    void OnEnable()
    {
        //TickManagerBehaviour.RegisterLate(this);
    }

    void OnDisable()
    {
        //TickManagerBehaviour.UnregisterLate(this);
    }

    void LookAt(Camera camera)
    {
        if (camera == null) return;

        if (lockToY)
        {
            Vector3 dir = camera.transform.position - transform.position;
            dir.y = 0;
            transform.rotation = Quaternion.LookRotation(dir);
        }
        else
        {
            transform.LookAt(camera.transform);
        }
    }

    public void Tick()
    {
        throw new System.NotImplementedException();
    }

    public void LateTick()
    {
        if (!defaultRenderer || !defaultRenderer.isVisible) return;

        LookAt(Camera.main);
    }
}
