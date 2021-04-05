using System.Collections.Generic;
using UnityEngine;

public interface ITickable
{
    public void Tick();

    public void LateTick();
}


class TickManagerBehaviour : MonoBehaviour
{
    static TickManagerBehaviour instance = null;
    List<ITickable> tickables = new List<ITickable>();
    List<ITickable> lateTickables = new List<ITickable>();

    private void Awake()
    {
        Debug.Assert(instance == null, "There should only be one TickManagerBehaviour!");
        if (!instance)
        {
            instance = this;
        }
        else 
        {
            Destroy(this);
        }
    }

    private void Update()
    {
        for(int i = 0; i < tickables.Count; i++)
        {
            tickables[i].Tick();
        }
    }

    private void LateUpdate()
    {
        for (int i = 0; i < lateTickables.Count; i++)
        {
            lateTickables[i].LateTick();
        }
    }

    public static void Register(ITickable tickable)
    {
        instance?.tickables.Add(tickable);
    }

    public static void Unregister(ITickable tickable)
    {
        instance?.tickables.Remove(tickable);
    }

    public static void RegisterLate(ITickable tickable)
    {
        instance?.lateTickables.Add(tickable);
    }

    public static void UnregisterLate(ITickable tickable)
    {
        instance?.lateTickables.Remove(tickable);
    }
}