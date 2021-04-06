/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace InfinityCode.uContext
{
    public static partial class Prefs
    {
        public static string createBrowserBlacklist = "";
        private static string[] createBrowserBlacklists;

        private class CreateBrowserManager : PrefManager
        {
            private static GUIStyle centeredHelpbox;

            public override IEnumerable<string> keywords
            {
                get { return new[] { "Create Browser", "Prefabs Folder Blacklist" }; }
            }

            public override float order
            {
                get { return -41; }
            }

            public override void Draw()
            {
                if (centeredHelpbox == null)
                {
                    centeredHelpbox = new GUIStyle(EditorStyles.helpBox);
                    centeredHelpbox.alignment = TextAnchor.MiddleCenter;
                }

                EditorGUILayout.LabelField("Create Browser", EditorStyles.boldLabel);
                EditorGUI.indentLevel++; 

                EditorGUILayout.LabelField("Prefabs Folder Blacklist", EditorStyles.boldLabel);
                if (createBrowserBlacklists == null) createBrowserBlacklists = createBrowserBlacklist.Split(new []{";"}, StringSplitOptions.RemoveEmptyEntries);

                int removeIndex = -1;
                EditorGUI.indentLevel++;
                for (int i = 0; i < createBrowserBlacklists.Length; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(createBrowserBlacklists[i], EditorStyles.textField);
                    if (GUILayout.Button("X", GUILayout.ExpandWidth(false))) removeIndex = i;
                    EditorGUILayout.EndHorizontal();
                }

                if (removeIndex != -1)
                {
                    ArrayUtility.RemoveAt(ref createBrowserBlacklists, removeIndex);
                    createBrowserBlacklist = string.Join(";", createBrowserBlacklists);
                    GUI.changed = true;
                }

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(34); 
                GUILayout.Box("To add items, drag folders here", centeredHelpbox, GUILayout.Height(30));
                ProcessDragAndDropFolder();

                EditorGUILayout.EndHorizontal();
                EditorGUI.indentLevel--;
                EditorGUI.indentLevel--;
            }

            private static void ProcessDragAndDropFolder()
            {
                Event e = Event.current;
                if (e.type != EventType.DragUpdated && e.type != EventType.DragPerform) return;
                Rect rect = GUILayoutUtility.GetLastRect();
                if (!rect.Contains(e.mousePosition)) return;
                if (DragAndDrop.objectReferences.Length <= 0) return;

                foreach (Object o in DragAndDrop.objectReferences)
                {
                    if (!(o is DefaultAsset))
                    {
                        return;
                    }
                }

                if (e.type == EventType.DragUpdated)
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                }
                else
                {
                    DragAndDrop.AcceptDrag();
                    ArrayUtility.AddRange(ref createBrowserBlacklists, DragAndDrop.paths);
                    createBrowserBlacklist = string.Join(";", createBrowserBlacklists);
                    GUI.changed = true;
                }

                e.Use();
            }
        }
    }
}