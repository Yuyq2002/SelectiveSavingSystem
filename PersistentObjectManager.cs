using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace PersistentManager
{
    public static class PersistentObjectManager
    {
        static List<GameObject> persistentObjects = new();

        public static void DonNotDestroyOnLoad(GameObject obj)
        {
            GameObject.DontDestroyOnLoad(obj);
            if(!persistentObjects.Contains(obj)) persistentObjects.Add(obj);
        }

        public static GameObject[] GetPersistentObjects()
        {
            persistentObjects.RemoveAll(po => po == null);

            return persistentObjects.ToArray();
        }
    }
}
