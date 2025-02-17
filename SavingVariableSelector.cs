using System;
using System.Collections.Generic;
using UnityEngine;

namespace SavingSystem
{
    public class SavingVariableSelector : MonoBehaviour
    {
        [Serializable]
        public class ComponentSavingData
        {
            public MonoBehaviour monoBehaviour;
            public string[] fieldsToSaveName;

            public ComponentSavingData(MonoBehaviour monoBehaviour, string[] fieldsDetails)
            {
                this.monoBehaviour = monoBehaviour;
                this.fieldsToSaveName = fieldsDetails;
            }
        }

        [SerializeField] private string saveFile;
        [SerializeField, HideInInspector] private string id;
        [SerializeField, HideInInspector] private List<ComponentSavingData> componentSavingData = new();

        public void SetSavingField(List<ComponentSavingData> data)
        {
            componentSavingData = data;
        }

        public string[] GetValue(MonoBehaviour key)
        {
            ComponentSavingData d = componentSavingData.Find(d => d.monoBehaviour == key);

            return d == null ? new string[0] : d.fieldsToSaveName;
        }

        public bool ContainsKey(MonoBehaviour key)
        {
            ComponentSavingData d = componentSavingData.Find(d => d.monoBehaviour == key);

            return d != null;
        }

        public string GetFileName()
        {
            return saveFile;
        }

        public ComponentSavingData[] GetFieldToSave()
        {
            return componentSavingData.ToArray();
        }

        public string GetID()
        {
            return id;
        }

        public bool HaveDataToSave()
        {
            return componentSavingData.Count > 0;
        }

        protected void OnValidate()
        {
            SetID();
        }

        [ContextMenu("SetID")]
        private void SetID()
        {
            id = gameObject.GetInstanceID().ToString();
        }
    }
}
