/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.uContext
{
    public static partial class Prefs
    {
        public static bool showToolValues = true;
#if !UNITY_EDITOR_OSX
        public static EventModifiers toolValuesModifiers = EventModifiers.Control;
#else
        public static EventModifiers toolValuesModifiers = EventModifiers.Command;
#endif

        private class ToolValuesManager : PrefManager, IHasShortcutPref
        {
            public override IEnumerable<string> keywords
            {
                get
                {
                    return new[]
                    {
                        "Show Tool Values In Scene View"
                    };
                }
            }

            public override float order
            {
                get { return -9f; }
            }

            public override void Draw()
            {
                showToolValues = EditorGUILayout.ToggleLeft("Show Tool Values In SceneView", showToolValues, EditorStyles.boldLabel);
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(20);
                GUILayout.Label("Modifiers", GUILayout.Width(modifierLabelWidth + 15));
                toolValuesModifiers = DrawModifiers(toolValuesModifiers, true);
                EditorGUILayout.EndHorizontal();
            }

            public IEnumerable<Shortcut> GetShortcuts()
            {
                if (!group) return new Shortcut[0];

                return new[]
                {
                    new Shortcut("Show Tool Values", "Scene View", toolValuesModifiers),
                };
            }
        }
    }
}