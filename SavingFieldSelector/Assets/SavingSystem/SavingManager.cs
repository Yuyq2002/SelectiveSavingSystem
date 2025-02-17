using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using PersistentManager;
using UnityEngine.SubsystemsImplementation;
using System.Collections;

#if UNITY_EDITOR
namespace SavingSystem
{   
    public static class SavingManager
    {
        class SelectorCategory
        {
            public string fileName;
            public List<SavingVariableSelector> selectors;
        }

        class SaveBuffer
        {
            public string file, buffer;
        }

        static List<SavingVariableSelector> selectors = new();
        static List<SaveBuffer> buffers = new();

        // Start is called once before the first execution of Update+ after the MonoBehaviour is created
        private static void UpdateReferences()
        {
            selectors.Clear();
            buffers.Clear();

            foreach (var gameObject in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                SavingVariableSelector s = gameObject.GetComponentInChildren<SavingVariableSelector>();
                if (s != null) selectors.Add(s);
            }

            foreach (var persistentObj in PersistentObjectManager.GetPersistentObjects())
            {
                SavingVariableSelector s = persistentObj.GetComponentInChildren<SavingVariableSelector>();
                if(s != null) selectors.Add(s);
            }

            HashSet<string> existingFile = new();
            foreach(var s in selectors)
            {
                if (string.IsNullOrEmpty(s.GetFileName())) continue;

                if(!existingFile.Contains(s.GetFileName()))
                {
                    SaveBuffer newBuffer = new();
                    newBuffer.file = s.GetFileName();
                    newBuffer.buffer = "";
                    buffers.Add(newBuffer);
                    existingFile.Add(s.GetFileName());
                }
            }
        }

        public static void SaveAll()
        {
            UpdateReferences();

            foreach(var b in buffers)
            {
                b.buffer = "";
            }

            foreach(SavingVariableSelector s in selectors)
            {
                if (string.IsNullOrEmpty(s.GetFileName()))
                {
                    Debug.LogWarning($"Selector on GameObject {s.gameObject.name} have an empty file name");
                    continue;
                }

                ref string b = ref buffers.Find(b => b.file == s.GetFileName()).buffer;

                if (!s.HaveDataToSave()) continue;
                Type type;

                b += "ID " + s.GetID() + '\n';

                foreach (SavingVariableSelector.ComponentSavingData d in s.GetFieldToSave())
                {
                    type = d.monoBehaviour.GetType();
                    b += "COMP " + type.FullName + '\n';

                    foreach (string n in d.fieldsToSaveName)
                    {
                        FieldInfo info = type.GetField(n, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        
                        b += "DATA " + n + ' ' + FormatTypeString(info.FieldType) + ' ' + FormatDataString(info.FieldType, info.GetValue(d.monoBehaviour)) + '\n';
                    }
                }
            }

            foreach (SaveBuffer b in buffers)
                SavingSystem.WriteToFile(b.file, b.buffer);
        }

        private static string FormatTypeString(Type type)
        {
            if (type.IsEnum || type.IsPrimitive || type == typeof(string)) return type.FullName;
            else
            {
                string output = "";
                output += type.GetGenericTypeDefinition().FullName + '|';

                Type[] generic = type.GetGenericArguments();
                for (int i = 0; i < generic.Length; i++)
                {
                    output += generic[i].FullName;
                    if (i < generic.Length - 1) output += ',';
                }

                return output;
            }
        }

        private static string FormatDataString(Type type, object field)
        {
            if (type.IsEnum || type.IsPrimitive || type == typeof(string)) return field.ToString();
            else
            {
                Type generic = type.GetGenericTypeDefinition();
                if(generic == typeof(List<>))
                {
                    if (!typeof(IList).IsAssignableFrom(type)) return "";

                    IList list = field as IList;
                    string output = "";

                    if(list != null)
                        for (int i = 0; i < list.Count; i++)
                        {
                            output += list[i].ToString();
                            if (i < list.Count - 1) output += ',';
                        }

                    return output;
                }
            }

            return "";
        }

        public static void LoadAll()
        {
            UpdateReferences();

            string[] splitBuffer;

            foreach (SaveBuffer b in buffers)
            {
                if (!SavingSystem.ReadFromFile(b.file, out b.buffer)) continue;

                splitBuffer = b.buffer.Split('\n');
                GameObject obj = null;
                Type type = null;
                MonoBehaviour comp = null;

                foreach(string line in splitBuffer)
                {
                    string[] splitLine = line.Split(' '); 

                    switch(splitLine[0])
                    {
                        case "ID":
                            obj = selectors.Find(s => s.GetID() == splitLine[1]).gameObject;
                            break;
                        case "COMP":
                            type = Type.GetType(splitLine[1]);
                            comp = obj.GetComponent(type) as MonoBehaviour;
                            break;
                        case "DATA":
                            if (!obj.GetComponent<SavingVariableSelector>().GetValue(comp).Contains(splitLine[1])) break;
                            FieldInfo field = type.GetField(splitLine[1], BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                            string typeString = splitLine[2];
                            ConvertDataToField(field, comp, typeString, splitLine[3]);
                            break;
                    }
                }
            }
        }

        private static void ConvertDataToField(FieldInfo field, MonoBehaviour comp, string typeString, string value)
        {
            string[] types = typeString.Split('|');
            switch(types.Length)
            {
                case 1:
                    {
                        if (types[0].Split('.')[0] == "UnityEngine")
                            types[0] += ", UnityEngine";
                        Type parseType = Type.GetType(types[0]);
                        var convertor = TypeDescriptor.GetConverter(parseType);
                        field.SetValue(comp, convertor.ConvertFrom(value));
                        break;
                    }
                case 2:
                    {
                        if (types[1].Split('.')[0] == "UnityEngine")
                            types[1] += ", UnityEngine";
                        Type outerType = Type.GetType(types[0]);
                        Type innerType = Type.GetType(types[1]);
                        var convertor = TypeDescriptor.GetConverter(innerType);
                        string[] data = value.Split(',');
                        Array a = Array.CreateInstance(innerType, data.Length);
                        for(int i = 0; i < data.Length; i++)
                            a.SetValue(convertor.ConvertFrom(data[i]), i);
                        field.SetValue(comp, Activator.CreateInstance(outerType.MakeGenericType(innerType), a));
                        break;
                    }
            }
        }
    }
}

#endif
