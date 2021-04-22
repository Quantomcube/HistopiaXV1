/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using UnityEditor;

namespace InfinityCode.uContext
{
    public static partial class Prefs
    {
        public static bool selectionBounds = true;

        private class SelectionBoundsManager : PrefManager, IHasShortcutPref
        {
            public override IEnumerable<string> keywords
            {
                get
                {
                    return new[]
                    {
                        "Display Bounds of Selected Renderers (Hotkey - CapsLock)"
                    };
                }
            }

            public override float order
            {
                get { return -10f; }
            }

            public override void Draw()
            {
                selectionBounds = EditorGUILayout.ToggleLeft("Display Bounds of Selected Renderers (Hotkey - CapsLock)", selectionBounds, EditorStyles.boldLabel);
            }

            public IEnumerable<Shortcut> GetShortcuts()
            {
                if (!selectionBounds) return new Shortcut[0];

                return new[]
                {
                    new Shortcut("Display Bounds of Selected Renderers", "Scene View", "CapsLock"),
                };
            }
        }
    }
}