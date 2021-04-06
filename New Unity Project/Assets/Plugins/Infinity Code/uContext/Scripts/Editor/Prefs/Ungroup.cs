/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.uContext
{
    public static partial class Prefs
    {
        public static bool ungroup = true;

        public static KeyCode ungroupKeyCode = KeyCode.G;

#if !UNITY_EDITOR_OSX
        public static EventModifiers ungroupModifiers = EventModifiers.Control | EventModifiers.Alt;
#else
        public static EventModifiers ungroupModifiers = EventModifiers.Command | EventModifiers.Alt;
#endif

        private class UngroupManager : PrefManager, IHasShortcutPref
        {
            public override IEnumerable<string> keywords
            {
                get
                {
                    return new[]
                    {
                        "Ungroup"
                    };
                }
            }

            public override float order
            {
                get { return -34.9999f; }
            }

            public override void Draw()
            {
                DrawFieldWithHotKey("Ungroup", ref ungroup, ref ungroupKeyCode, ref ungroupModifiers, EditorStyles.boldLabel, 17);
            }

            public IEnumerable<Shortcut> GetShortcuts()
            {
                if (!ungroup) return new Shortcut[0];

                return new[]
                {
                    new Shortcut("Ungroup GameObjects", "Everywhere", ungroupModifiers, ungroupKeyCode),
                };
            }
        }
    }
}