using System.Collections.Generic;
using UnityEngine;


public static class ResourceCache
{
    private static readonly Dictionary<string, GameObject> cache = new Dictionary<string, GameObject>();

    public static GameObject Load(string resourceName)
    {
        if (string.IsNullOrEmpty(resourceName)) return null;

        // Return cached successes and cached misses.
        if (cache.TryGetValue(resourceName, out GameObject cached))
        {
            return cached;
        }

        GameObject loaded = Resources.Load<GameObject>(resourceName);
        cache[resourceName] = loaded;
        return loaded;
    }
}
