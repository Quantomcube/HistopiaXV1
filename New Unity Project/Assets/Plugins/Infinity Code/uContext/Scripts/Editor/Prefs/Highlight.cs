/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.uContext
{
    public static partial class Prefs
    {
        public static bool highlight = true;
        public static Color highlightColor = new Color(1, 1, 0, 0.2f);
        public static bool highlightOnWaila = true;
        public static bool highlightOnHierarchy = true;

#if UNITY_EDITOR_OSX
        public static EventModifiers highlightOnHierarchyModifiers = EventModifiers.Command;
#else
        public static EventModifiers highlightOnHierarchyModifiers = EventModifiers.Control;
#endif
        public static bool highlightHierarchyRow = true;
        public static Color highlightHierarchyRowColor = Color.red;

        private class HighlightManager : PrefManager, IHasShortcutPref
        {
#if !UCONTEXT_PRO
            private const string highlightRowLabel = "Highlight Hierarchy Row (PRO)";
#else
            private const string highlightRowLabel = "Highlight Hierarchy Row";
#endif

            public override IEnumerable<string> keywords
            {
                get { return new[]
                {
                    "Highlighter", 
                    "Color", 
                    "Highlight On Waila", 
                    "Highlight On Hierarchy", 
                    "Hierarchy Row Color", 
                    "Highlight Hierarchy Row"
                }; }
            }

            public override float order
            {
                get { return -47; }
            }

            public override void Draw()
            {
                highlight = EditorGUILayout.ToggleLeft("Highlighter", highlight, EditorStyles.boldLabel);

                EditorGUI.BeginDisabledGroup(!highlight);
                EditorGUI.indentLevel++;

                highlightColor = EditorGUILayout.ColorField("Color", highlightColor);
                highlightOnWaila = EditorGUILayout.ToggleLeft("Highlight On Waila", highlightOnWaila);
                DrawFieldWithModifiers("Highlight On Hierarchy", ref highlightOnHierarchy, ref highlightOnHierarchyModifiers);
                highlightHierarchyRow = EditorGUILayout.ToggleLeft(highlightRowLabel, highlightHierarchyRow);
                highlightHierarchyRowColor = EditorGUILayout.ColorField("Hierarchy Row Color", highlightHierarchyRowColor);

                EditorGUI.indentLevel--;
                EditorGUI.EndDisabledGroup();
            }

            public IEnumerable<Shortcut> GetShortcuts()
            {
                if (!highlight || !highlightOnHierarchy) return new Shortcut[0];

                return new[]
                {
                    new Shortcut("Highlight GameObject In Scene View", "Hierarchy", highlightOnHierarchyModifiers)
                };
            }
        }
    }
}