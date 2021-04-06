/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.uContext
{
    public static partial class Prefs
    {
        public static bool replace = true;

        public static KeyCode replaceKeyCode = KeyCode.H;
#if !UNITY_EDITOR_OSX
        public static EventModifiers replaceModifiers = EventModifiers.Control | EventModifiers.Shift;
#else
        public static EventModifiers replaceModifiers = EventModifiers.Command | EventModifiers.Shift;
#endif

        private class ReplaceManager : PrefManager, IHasShortcutPref
        {
#if !UCONTEXT_PRO
            private const string replaceLabel = "Replace (PRO)";
#else
            private const string replaceLabel = "Replace";
#endif

            public override IEnumerable<string> keywords
            {
                get
                {
                    return new[]
                    {
                        "Replace"
                    };
                }
            }

            public override float order
            {
                get { return -45; }
            }

            public override void Draw()
            {
                DrawFieldWithHotKey(replaceLabel, ref replace, ref replaceKeyCode, ref replaceModifiers, EditorStyles.boldLabel, 17);
            }

            public IEnumerable<Shortcut> GetShortcuts()
            {
                if (!replace) return new Shortcut[0];

                return new[]
                {
                    new Shortcut("Replace Selected GameObjects", "Everywhere", replaceModifiers, replaceKeyCode), 
                };
            }
        }
    }
}