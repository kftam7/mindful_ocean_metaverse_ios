using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ProGamer
{
    /// This is a class of managing resuable gameObject. Use it to create or remove the gameObjects which are continually spawned or destroyed in the scene.
    /// 
    /// Usage: ReuseableObjectManager.Instantiate (GameObject, Vector3, Quaternion).
    public static class ReuseableObjectManager
    {
        static Dictionary<string, List<GameObject>> _objectPool = new Dictionary<string, List<GameObject>>();

        /// Create a reuseable gameObject. (Details: This function will look for an inactive gameObject which is the same prefab with the target, and active it instead of instantiation.)
        public static GameObject Instantiate(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (_objectPool.ContainsKey(prefab.name))
            {
                // Reuse
                List<GameObject> list = _objectPool[prefab.name];
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] == null)
                    {
                        list.RemoveAt(i);
                        continue;
                    }

                    if (!list[i].activeSelf)
                    {
                        //fix local position get altered
                        list[i].transform.localPosition = position;
                        list[i].transform.localRotation = rotation;
                        list[i].gameObject.SetActive(true);
                        return list[i];
                    }
                }

                GameObject newObj = (GameObject.Instantiate(prefab, position, rotation) as GameObject);
                list.Add(newObj);
                list = null;

                return newObj;
            }
            else
            {
                // If we can't find a reusable object, add a new object into the pool;
                List<GameObject> list = new List<GameObject>();

                GameObject newObj = (GameObject.Instantiate(prefab, position, rotation) as GameObject);
                list.Add(newObj);
                _objectPool.Add(prefab.name, list);
                list = null;
                return newObj;
            }
        }

        /// Create a reuseable gameObject. (Details: This function will look for an inactive gameObject which is the same prefab with the target, and active it instead of instantiation.)
        public static GameObject Instantiate(string prefabName, Vector3 position, Quaternion rotation)
        {
            if (_objectPool.ContainsKey(prefabName))
            {
                try
                {
                    return Instantiate(_objectPool[prefabName][0], position, rotation);
                }
                catch (UnityException e)
                {
                    Debug.LogError(e.Message);
                    return Instantiate((Resources.Load(prefabName) as GameObject), position, rotation);
                }
            }
            else
            {
                return Instantiate((Resources.Load(prefabName) as GameObject), position, rotation);
            }
        }

        /// Create a reuseable gameObject and assign parent with local size and position
        public static GameObject Instantiate(string prefabName, Transform parent)
        {
            GameObject go = Instantiate(prefabName, Vector3.zero, Quaternion.identity);
            go.transform.SetParent(parent);
            go.transform.localScale = Vector3.one;
            go.transform.localPosition = Vector3.zero;

            return go;
        }

        /// Flush the inactive gameObjects in the specified array with a remaining number.
        public static bool Flush(string prefabName, uint remainingObjectsNumber = 0)
        {
            if (!_objectPool.ContainsKey(prefabName))
            {
                return false;
            }

            List<GameObject> list = _objectPool[prefabName];
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (remainingObjectsNumber == 0 || !list[i].activeSelf)
                {
                    GameObject.Destroy(list[i]);
                    list.RemoveAt(i);
                }
            }

            list = null;
            return true;
        }

        /// Remove the references in the specified array.
        public static bool Clear(string prefabName)
        {
            if (!_objectPool.ContainsKey(prefabName))
            {
                return false;
            }

            _objectPool[prefabName].Clear();
            return true;
        }

        /// Remove the specified array.
        public static bool Delete(string prefabName)
        {
            if (!Clear(prefabName))
                return false;
            _objectPool.Remove(prefabName);
            return true;
        }
    }
}