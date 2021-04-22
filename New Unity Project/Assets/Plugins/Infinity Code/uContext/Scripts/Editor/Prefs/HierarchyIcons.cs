/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace InfinityCode.uContext
{
    public static partial class Prefs
    {
        public static bool hierarchyIcons = true;
        public static bool hierarchyErrorIcons = true;
        public static bool hierarchyOverrideMainIcon = true;

#if !UNITY_EDITOR_OSX
        public static EventModifiers hierarchyIconsModifiers = EventModifiers.Control;
#else
        public static EventModifiers hierarchyIconsModifiers = EventModifiers.Command;
#endif
        public static int hierarchyIconsMaxItems = 6;
        public static string hierarchyIconsRemovePrefix = "Online Maps;Real World Terrain";

        private static string[] _hierarchyIconsRemovePrefix;

        public static string HierarchyIconRemovePrefix(string text)
        {
            if (_hierarchyIconsRemovePrefix == null) _hierarchyIconsRemovePrefix = hierarchyIconsRemovePrefix.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string p in _hierarchyIconsRemovePrefix)
            {
                if (text.Length > p.Length && text.StartsWith(p)) return text.Substring(p.Length);
            }

            return text;
        }

        public class HierarchyIconManager : StandalonePrefManager<HierarchyIconManager>, IHasShortcutPref
        {
#if !UCONTEXT_PRO
            private const string sectionLabel = "Show Components In Hierarchy (PRO)";
#else
            private const string sectionLabel = "Show Components In Hierarchy";
#endif

            public override IEnumerable<string> keywords
            {
                get { return new[] { "Hierarchy Icons", "Max Items", "Show error icon if GameObject has an error or exception" }; }
            }

            public override float order
            {
                get { return -46; }
            }

            public override void Draw()
            {
                hierarchyIcons = EditorGUILayout.ToggleLeft(sectionLabel, hierarchyIcons, EditorStyles.label);

                EditorGUI.indentLevel++;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Modifiers", GUILayout.Width(labelWidth - 17));
                hierarchyIconsModifiers = DrawModifiers(hierarchyIconsModifiers);
                EditorGUILayout.EndHorizontal();

                hierarchyIconsMaxItems = EditorGUILayout.IntField("Max Items", hierarchyIconsMaxItems);
                if (hierarchyIconsMaxItems < 1) hierarchyIconsMaxItems = 1;

                EditorGUI.indentLevel--;

                hierarchyErrorIcons = EditorGUILayout.ToggleLeft("Show Error Icon When GameObject Has an Error or Exception", hierarchyErrorIcons);
                EditorGUI.BeginChangeCheck();
                hierarchyOverrideMainIcon = EditorGUILayout.ToggleLeft("Show Best Component Icon Before Name", hierarchyOverrideMainIcon);
                if (EditorGUI.EndChangeCheck())
                {
                    Object[] windows = UnityEngine.Resources.FindObjectsOfTypeAll(EditorTypes.sceneHierarchyWindow);
                    foreach (Object wnd in windows)
                    {
                        EditorWindow window = wnd as EditorWindow;
                        HierarchyHelper.SetDefaultIconsSize(window, hierarchyOverrideMainIcon? 0: 18);
                        window.Repaint();
                    }
                }

                if (_hierarchyIconsRemovePrefix == null) _hierarchyIconsRemovePrefix = hierarchyIconsRemovePrefix.Split(new[] {";"}, StringSplitOptions.RemoveEmptyEntries);

                EditorGUILayout.LabelField("Remove Class Prefix");

                EditorGUI.BeginChangeCheck();

                EditorGUI.indentLevel++;

                int removeIndex = -1;
                for (int i = 0; i < _hierarchyIconsRemovePrefix.Length; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    _hierarchyIconsRemovePrefix[i] = EditorGUILayout.TextField(_hierarchyIconsRemovePrefix[i]);
                    if (GUILayout.Button("X", GUILayout.ExpandWidth(false))) removeIndex = i;
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUI.indentLevel--;

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(16);
                if (GUILayout.Button("Add"))
                {
                    ArrayUtility.Add(ref _hierarchyIconsRemovePrefix, "");
                }
                EditorGUILayout.EndHorizontal();

                if (removeIndex != -1)
                {
                    ArrayUtility.RemoveAt(ref _hierarchyIconsRemovePrefix, removeIndex);
                    UpdateHierarchyIconsRemovePrefix();
                    GUI.changed = true;
                }

                if (EditorGUI.EndChangeCheck()) UpdateHierarchyIconsRemovePrefix();
            }

            public IEnumerable<Shortcut> GetShortcuts()
            {
                if (!hierarchyIcons) return new Shortcut[0];

                return new[]
                {
                    new Shortcut("Show Component Icons", "Hierarchy", hierarchyIconsModifiers)
                };
            }

            private static void UpdateHierarchyIconsRemovePrefix()
            {
                hierarchyIconsRemovePrefix = string.Join(";", _hierarchyIconsRemovePrefix.Where(s => !string.IsNullOrEmpty(s)).ToArray());
            }
        }
    }
}