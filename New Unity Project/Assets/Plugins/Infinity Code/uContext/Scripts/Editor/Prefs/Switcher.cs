/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.uContext
{
    public static partial class Prefs
    {
        public static bool switcher = true;

        public static bool switcherWindows = true;
        public static KeyCode switcherWindowsKeyCode = KeyCode.Tab;
#if !UNITY_EDITOR_OSX
        public static EventModifiers switcherWindowsModifiers = EventModifiers.Control | EventModifiers.Shift;
#else
        public static EventModifiers switcherWindowsModifiers = EventModifiers.Command | EventModifiers.Shift;
#endif
        public static bool switcherWindowsPause = true;

        private class SwitcherManager : PrefManager, IHasShortcutPref
        {
            public override IEnumerable<string> keywords
            {
                get
                {
                    return new[]
                    {
                        "Switcher",
                        "Game View <-> Scene View",
                        "Pause When Switching"
                    };
                }
            }

            public override float order
            {
                get { return -40; }
            }

            public override void Draw()
            {
                switcher = EditorGUILayout.ToggleLeft("Switcher", switcher, EditorStyles.boldLabel);
                EditorGUI.indentLevel++;

                EditorGUI.BeginDisabledGroup(!switcher);

                DrawFieldWithHotKey("Game View <-> Scene View", ref switcherWindows, ref switcherWindowsKeyCode, ref switcherWindowsModifiers);
                switcherWindowsPause = EditorGUILayout.ToggleLeft("Pause When Switching", switcherWindowsPause);

                EditorGUI.indentLevel--;

                EditorGUI.EndDisabledGroup();
            }

            public IEnumerable<Shortcut> GetShortcuts()
            {
                if (!switcher) return new Shortcut[0];

                return new[]
                {
                    new Shortcut("Switch Between Scene View and Game View", "Scene View or Game View", switcherWindowsModifiers, switcherWindowsKeyCode), 
                };
            }
        }
    }
}