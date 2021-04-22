/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using UnityEditor;

namespace InfinityCode.uContext
{
    public partial class Prefs
    {
        public static bool frameSelectedBounds = true;

        private class FrameSelectedBounds : PrefManager, IHasShortcutPref
        {
            public override IEnumerable<string> keywords
            {
                get
                {
                    return new[]
                    {
                        "Frame", "Selected", "Bounds"
                    };
                }
            }

            public override float order
            {
                get { return -40; }
            }

            public override void Draw()
            {
                frameSelectedBounds = EditorGUILayout.ToggleLeft("Frame Selected Bounds", frameSelectedBounds, EditorStyles.boldLabel);
            }

            public IEnumerable<Shortcut> GetShortcuts()
            {
                if (!frameSelectedBounds) return new Shortcut[0];

                return new[]
                {
                    new Shortcut("Frame Selected Bounds", "Scene View", "SHIFT + F"),
                };
            }
        }
    }
}