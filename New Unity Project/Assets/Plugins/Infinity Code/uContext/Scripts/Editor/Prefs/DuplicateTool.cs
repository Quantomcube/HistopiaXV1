/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.uContext
{
    public static partial class Prefs
    {
        public static bool switchToDuplicateToolByHotKey = false;

        private class DuplicateToolManager : PrefManager, IHasShortcutPref
        {
            public override IEnumerable<string> keywords
            {
                get { return new[] { "Switch to Duplicate Tool By HotKey" }; }
            }

            public override float order
            {
                get { return -11f; }
            }

            public override void Draw()
            {
#if !UCONTEXT_PRO
                switchToDuplicateToolByHotKey = EditorGUILayout.ToggleLeft("Switch to Duplicate Tool By HotKey (PRO)", switchToDuplicateToolByHotKey, EditorStyles.boldLabel);
#else
                switchToDuplicateToolByHotKey = EditorGUILayout.ToggleLeft("Switch to Duplicate Tool By HotKey", switchToDuplicateToolByHotKey, EditorStyles.boldLabel);
#endif
            }

            public IEnumerable<Shortcut> GetShortcuts()
            {
                if (switchToDuplicateToolByHotKey)
                {
                    return new[]
                    {
                        new Shortcut("Switch To Duplicate Tool",
                            "SceneView",
#if UNITY_EDITOR_OSX
                            EventModifiers.Command | EventModifiers.Shift
#else
                            EventModifiers.Control | EventModifiers.Shift
#endif
                        )
                    };
                }
                return new Shortcut[0];
            }
        }
    }
}