using System.Collections.Generic;
using UnityEngine;

public class PoolReturn : MonoBehaviour
{
    [HideInInspector] public GameObject prefab;
    public void ReturnToPool() => SimplePool.Return(prefab, gameObject);
}

public static class SimplePool
{
    static readonly Dictionary<GameObject, Queue<GameObject>> pools = new();

    public static GameObject Get(GameObject prefab, Vector3 pos, Quaternion rot)
    {
        if (!pools.TryGetValue(prefab, out var queue))
        {
            queue = new Queue<GameObject>();
            pools[prefab] = queue;
        }

        GameObject obj;
        if (queue.Count > 0)
        {
            obj = queue.Dequeue();
            obj.transform.SetPositionAndRotation(pos, rot);
            obj.SetActive(true);
        }
        else
        {
            obj = Object.Instantiate(prefab, pos, rot);
            var pr = obj.GetComponent<PoolReturn>() ?? obj.AddComponent<PoolReturn>();
            pr.prefab = prefab;
        }
        return obj;
    }

    public static void Return(GameObject prefab, GameObject obj)
    {
        obj.SetActive(false);
        if (!pools.TryGetValue(prefab, out var queue))
        {
            queue = new Queue<GameObject>();
            pools[prefab] = queue;
        }
        queue.Enqueue(obj);
    }

    public static void Clear() => pools.Clear();
}
