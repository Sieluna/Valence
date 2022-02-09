using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Valence.Environment
{
    [CustomEditor(typeof(BlockComponent))]
    public class BlockComponentEditor : Editor
    {
        private SerializedProperty m_BlocksProperty;
        private ReorderableList m_BlockAssetList;
        private List<Type> m_DerivedTypes;

        private void OnEnable()
        {
            m_DerivedTypes = GetAllDerivedTypes<BlockAsset>();

            m_BlocksProperty = serializedObject.FindProperty("blocks");
            m_BlockAssetList = new ReorderableList(serializedObject, m_BlocksProperty, true, true, true, true)
            {
                drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Block Assets"),
                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    var element = m_BlockAssetList.serializedProperty.GetArrayElementAtIndex(index);
                    var halfWidth = rect.width / 2;
                    rect.y += 2;

                    EditorGUI.BeginProperty(rect, GUIContent.none, element);

                    EditorGUI.PropertyField(new Rect(rect.x, rect.y, halfWidth - 2.5f, EditorGUIUtility.singleLineHeight), element, GUIContent.none);

                    if (element.objectReferenceValue != null)
                    {
                        var elementSo = new SerializedObject(element.objectReferenceValue);
                        var nameProperty = elementSo.FindProperty("m_Name");
                        EditorGUI.PropertyField(new Rect(rect.x + halfWidth + 2.5f, rect.y, halfWidth - 2.5f, EditorGUIUtility.singleLineHeight), nameProperty, GUIContent.none);
                        elementSo.ApplyModifiedProperties();
                    }

                    EditorGUI.EndProperty();
                },
                onAddCallback = list =>
                {
                    var menu = new GenericMenu();

                    foreach (var type in m_DerivedTypes)
                    {
                        menu.AddItem(new GUIContent(type.Name), false, () => AddItem(type));
                    }

                    menu.ShowAsContext();
                },
                onRemoveCallback = list =>
                {
                    var index = list.index;
                    var element = list.serializedProperty.GetArrayElementAtIndex(index).objectReferenceValue;

                    if (element != null)
                    {
                        Undo.DestroyObjectImmediate(element);
                        AssetDatabase.SaveAssets();
                    }

                    ReorderableList.defaultBehaviours.DoRemoveButton(list);
                }
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            m_BlockAssetList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }

        private void AddItem(Type type)
        {
            var instance = CreateInstance(type);
            instance.name = type.Name;
            AssetDatabase.AddObjectToAsset(instance, target);
            AssetDatabase.SaveAssets();

            if (m_BlockAssetList.serializedProperty != null)
            {
                var listProperty = m_BlockAssetList.serializedProperty;
                listProperty.arraySize++;
                listProperty.serializedObject.ApplyModifiedProperties();

                m_BlocksProperty.GetArrayElementAtIndex(m_BlocksProperty.arraySize - 1).objectReferenceValue = instance;
                m_BlockAssetList.serializedProperty.serializedObject.ApplyModifiedProperties();

                AssetDatabase.SaveAssets();
            }
        }

        private static List<Type> GetAllDerivedTypes<T>()
        {
            var derivedType = typeof(T);
            var assembly = derivedType.Assembly;

            return assembly.GetTypes()
                .Where(type => type.IsSubclassOf(derivedType) && !type.IsAbstract)
                .ToList();
        }
    }
}