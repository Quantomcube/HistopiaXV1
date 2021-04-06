/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

namespace InfinityCode.uContext.Tools
{
    [InitializeOnLoad]
    [EditorTool("Duplicate Tool")]
    public class DuplicateTool : EditorTool
    {
        public static int phase = 0;
        private static Vector3 position;
        private static Quaternion rotation;

        private static GUIContent passiveContent;
        private static GUIContent activeContent;

        public override GUIContent toolbarIcon
        {
            get
            {
#if UNITY_2020_2_OR_NEWER
                if (ToolManager.IsActiveTool(this))
#else
                if (EditorTools.IsActiveTool(this))
#endif
                {
                    if (activeContent == null) activeContent = new GUIContent(Icons.duplicateToolActive, "Duplicate Tool");
                    return activeContent;
                }

                if (passiveContent == null) passiveContent = new GUIContent(Icons.duplicateTool, "Duplicate Tool");
                return passiveContent;
            }
        }

        static DuplicateTool()
        {
            Selection.selectionChanged += OnSelectionChanged;
        }

        private static void OnSelectionChanged()
        {
            if (phase == 0)
            {
                position = UnityEditor.Tools.handlePosition;
                rotation = UnityEditor.Tools.handleRotation;
            }
        }

        public override void OnToolGUI(EditorWindow window)
        {
            Transform[] transforms = Selection.transforms;
            if (transforms.Length == 0) return;

            if (phase == 0)
            {
                position = UnityEditor.Tools.handlePosition;
                rotation = UnityEditor.Tools.handleRotation;
                phase = 1;
            }

            if (phase == 1)
            {
                EditorGUI.BeginChangeCheck();

                Vector3 newPosition = Handles.DoPositionHandle(position, rotation);

                if (EditorGUI.EndChangeCheck())
                {
                    Waila.Close();
                    Vector3 delta = newPosition - position;
                    if (Math.Abs(delta.sqrMagnitude) < float.Epsilon) return;

                    Undo.SetCurrentGroupName("Duplicate GameObjects");
                    int group = Undo.GetCurrentGroup();

                    List<GameObject> newGameObjects = new List<GameObject>();
                    foreach (GameObject go in Selection.gameObjects)
                    {
                        GameObject prefabRoot = PrefabUtility.GetCorrespondingObjectFromSource(go);
                        GameObject newGO;
                        if (prefabRoot != null) newGO = PrefabUtility.InstantiatePrefab(prefabRoot) as GameObject;
                        else newGO = Instantiate(go);
                        newGO.transform.SetParent(go.transform.parent);
                        newGO.transform.position = go.transform.position + delta;
                        newGO.transform.rotation = go.transform.rotation;
                        newGameObjects.Add(newGO);
                        Undo.RegisterCreatedObjectUndo(newGO, "Duplicate GameObjects");
                    }

                    Selection.objects = newGameObjects.ToArray();
                    Undo.CollapseUndoOperations(group);

                    phase = 2;
                }

                position = newPosition;
            }
            else if (phase == 2)
            {
                EditorGUI.BeginChangeCheck();

                Vector3 newPosition = Handles.DoPositionHandle(position, rotation);

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.SetCurrentGroupName("Move GameObjects");
                    int group = Undo.GetCurrentGroup();

                    Vector3 delta = newPosition - position;
                    foreach (Transform t in Selection.transforms)
                    {
                        Undo.RecordObject(t, "Move GameObject");
                        t.position += delta;
                    }

                    Undo.CollapseUndoOperations(group);
                }

                position = newPosition;
            }
        }
    }
}