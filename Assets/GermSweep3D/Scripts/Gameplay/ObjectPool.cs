using System.Collections.Generic;
using UnityEngine;

namespace GermSweep3D
{
    public sealed class ObjectPool
    {
        private readonly Dictionary<string, Queue<GameObject>> pool = new Dictionary<string, Queue<GameObject>>();
        private readonly Transform root;
        public ObjectPool(Transform root) { this.root = root; }
        public GameObject Get(string key, GameObject prefab, Vector3 position, Quaternion rotation)
        {
            Queue<GameObject> q;
            if (!pool.TryGetValue(key, out q)) { q = new Queue<GameObject>(); pool.Add(key, q); }
            GameObject go = q.Count > 0 ? q.Dequeue() : Object.Instantiate(prefab, root);
            go.transform.SetPositionAndRotation(position, rotation); go.SetActive(true); return go;
        }
        public void Release(string key, GameObject go)
        {
            go.SetActive(false); go.transform.SetParent(root, false);
            Queue<GameObject> q; if (!pool.TryGetValue(key, out q)) { q = new Queue<GameObject>(); pool.Add(key, q); }
            q.Enqueue(go);
        }
        public void ClearActiveChildren()
        {
            for (int i = root.childCount - 1; i >= 0; i--) Object.Destroy(root.GetChild(i).gameObject);
            pool.Clear();
        }
    }
}
