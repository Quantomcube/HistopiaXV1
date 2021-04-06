/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.uContext
{
    public static partial class Prefs
    {
        public static bool preview = true;

        public static KeyCode previewKeyCode = KeyCode.Q;

#if !UNITY_EDITOR_OSX
        public static EventModifiers previewModifiers = EventModifiers.Control | EventModifiers.Shift;
#else
        public static EventModifiers previewModifiers = EventModifiers.Command | EventModifiers.Shift;
#endif

        private class PreviewManager : PrefManager, IHasShortcutPref
        {
            public override IEnumerable<string> keywords
            {
                get
                {
                    return new[]
                    {
                        "Preview"
                    };
                }
            }

            public override float order
            {
                get { return -33; }
            }

            public override void Draw()
            {
                DrawFieldWithHotKey("Preview", ref preview, ref previewKeyCode, ref previewModifiers, EditorStyles.boldLabel, 17);
            }

            public IEnumerable<Shortcut> GetShortcuts()
            {
                if (!preview) return new Shortcut[0];

                return new []
                {
                    new Shortcut("Preview of Cameras", "Scene View", previewModifiers, previewKeyCode),
                    new Shortcut("Set View", "Preview of Cameras", "F"),
                };
            }
        }
    }
}