/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using UnityEditor;

namespace InfinityCode.uContext
{
    public static partial class Prefs
    {
        public static bool jumpToPoint = true;
        public static bool highJumpToPoint = true;

        private class JumpToPointManager : PrefManager, IHasShortcutPref
        {
            public override IEnumerable<string> keywords
            {
                get
                {
                    return new[]
                    {
                        "Jump To Point", "High"
                    };
                }
            }

            public override float order
            {
                get { return -35; }
            }

            public override void Draw()
            {
                jumpToPoint = EditorGUILayout.ToggleLeft("Jump To Point", jumpToPoint, EditorStyles.boldLabel);
#if UCONTEXT_PRO
                string label = "High Jump To Point";
#else
                string label = "High Jump To Point (PRO)";
#endif
                highJumpToPoint = EditorGUILayout.ToggleLeft(label, highJumpToPoint, EditorStyles.boldLabel);
            }

            public IEnumerable<Shortcut> GetShortcuts()
            {
                List<Shortcut> shortcuts = new List<Shortcut>();

                if (jumpToPoint)
                {
                    shortcuts.Add(new Shortcut("Jump To Point", "Scene View", "SHIFT + MMB"));
                }

#if UCONTEXT_PRO
                if (highJumpToPoint)
                {
#if UNITY_EDITOR_OSX
                    string shortcut = "CMD + SHIFT + MMB";
#else
                    string shortcut = "CTRL + SHIFT + MMB";
#endif
                    shortcuts.Add(new Shortcut("High Jump To Point", "Scene View", shortcut));
                }
#endif
                    return shortcuts;
            }
        }
    }
}