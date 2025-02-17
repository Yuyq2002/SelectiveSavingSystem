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
            public List<EditorFieldData> field;
            public bool show;

            public EditorComponentSavingData()
            {
                show = false;
                monoBehaviour = null;
                field = new();
            }

            public EditorComponentSavingData(MonoBehaviour b, List<EditorFieldData> f)
            {
                show = false;
                monoBehaviour = b;
                field = f;
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

                foreach (var f in m.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
                {
                    if (IsPrimitiveOrIncludedType(f.FieldType))
                        newData.field.Add(new(f.Name, arr.Contains(f.Name)));
                }

                editorData.Add(newData);
            }
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            List<ComponentSavingData> newSavingData = new();

            foreach (var d in editorData)
            {
                EditorGUILayout.LabelField(d.monoBehaviour.GetType().Name, titleStyle);

                d.show = EditorGUILayout.ToggleLeft("Show detail", d.show);

                List<string> newFieldList = new((target as SavingVariableSelector).GetValue(d.monoBehaviour));

                if (d.show)
                {
                    foreach (var f in d.field)
                    {
                        f.willSave = EditorGUILayout.Toggle(f.fieldName, newFieldList.Contains(f.fieldName));

                        if (f.willSave) { if (!newFieldList.Contains(f.fieldName)) 
                                newFieldList.Add(f.fieldName); }
                        else newFieldList.Remove(f.fieldName);
                    }
                }

                if(newFieldList.Count > 0) newSavingData.Add(new ComponentSavingData(d.monoBehaviour, newFieldList.ToArray()));

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
