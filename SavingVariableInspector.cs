using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using SavingSystem;
using static SavingSystem.SavingVariableSelector;

#if UNITY_EDITOR
namespace UnityEditor
{
    [CustomEditor(typeof(SavingVariableSelector)), CanEditMultipleObjects]
    public class SavingVariableInspector : Editor
    {
        class EditorFieldData
        {
            public string fieldName = "";
            public bool willSave = false;

            public EditorFieldData(string fieldName, bool willSave)
            {
                this.fieldName = fieldName;
                this.willSave = willSave;
            }
        }

        class EditorComponentSavingData
        {
            public MonoBehaviour monoBehaviour;
            public List<EditorFieldData> fields;
            public bool show;

            public EditorComponentSavingData()
            {
                show = false;
                monoBehaviour = null;
                fields = new();
            }

            public EditorComponentSavingData(MonoBehaviour inMonoBehaviour, List<EditorFieldData> inFields)
            {
                show = false;
                monoBehaviour = inMonoBehaviour;
                fields = inFields;
            }
        }

        private SavingVariableSelector selector;
        private List<EditorComponentSavingData> editorData = new();
        GUIStyle titleStyle = new GUIStyle();

        private void OnEnable()
        {
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.normal.textColor = Color.white;

            List<ComponentSavingData> temp = new();

            selector = target as SavingVariableSelector;
            MonoBehaviour[] l = selector.GetComponents<MonoBehaviour>();

            foreach (MonoBehaviour m in l)
            {
                if (m.GetType() == selector.GetType()) continue;

                string[] arr = (target as SavingVariableSelector).GetValue(m);

                EditorComponentSavingData newData = new();
                newData.monoBehaviour = m;

                foreach (var field in m.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
                {
                    if (IsPrimitiveOrIncludedType(field.FieldType))
                        newData.fields.Add(new(field.Name, arr.Contains(field.Name)));
                }

                editorData.Add(newData);
            }
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            List<ComponentSavingData> newSavingData = new();

            foreach (var data in editorData)
            {
                EditorGUILayout.LabelField(data.monoBehaviour.GetType().Name, titleStyle);

                data.show = EditorGUILayout.ToggleLeft("Show detail", data.show);

                List<string> newFieldList = new((target as SavingVariableSelector).GetValue(data.monoBehaviour));

                if (data.show)
                {
                    foreach (var field in data.fields)
                    {
                        field.willSave = EditorGUILayout.Toggle(field.fieldName, newFieldList.Contains(field.fieldName));

                        if (field.willSave) { if (!newFieldList.Contains(field.fieldName)) 
                                newFieldList.Add(field.fieldName); }
                        else newFieldList.Remove(field.fieldName);
                    }
                }

                if(newFieldList.Count > 0) newSavingData.Add(new ComponentSavingData(data.monoBehaviour, newFieldList.ToArray()));

                EditorGUILayout.Space();
            }

            if (GUI.changed)
            {
                (target as SavingVariableSelector).SetSavingField(newSavingData);

                EditorUtility.SetDirty(target as SavingVariableSelector);
            }
        }

        HashSet<string> writableTypes = new()
        {
            typeof(string).FullName,
        };

        private bool IsPrimitiveOrIncludedType(Type type)
        {
            if(type == null) return false;
            if(type.IsPrimitive || type.IsEnum || writableTypes.Contains(type.FullName) || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))) return true;

            return false; 
        }
    }
}

#endif
