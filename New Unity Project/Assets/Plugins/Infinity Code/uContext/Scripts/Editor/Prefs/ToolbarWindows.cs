/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using UnityEditor;

namespace InfinityCode.uContext
{
    public static partial class Prefs
    {
        public static bool windowsToolbarIcon = true;
        public static bool recentWindowsInToolbar = true;
        public static bool favoriteWindowsInToolbar = true;
        public static int maxRecentWindows = 10;

        private class ToolbarWindowsManager : PrefManager
        {
            public override IEnumerable<string> keywords
            {
                get
                {
                    return new[]
                    {
                        "Windows In Toolbar",
                        "Favorite Windows",
                        "Recent Windows",
                        "Max Recent Windows"
                    };
                }
            }

            public override float order
            {
                get { return -38; }
            }

            public override void Draw()
            {
                windowsToolbarIcon = EditorGUILayout.ToggleLeft("Windows In Toolbar", windowsToolbarIcon, EditorStyles.boldLabel);

                EditorGUI.BeginDisabledGroup(!windowsToolbarIcon);
                EditorGUI.indentLevel++;

                recentWindowsInToolbar = EditorGUILayout.ToggleLeft("Recent Windows", recentWindowsInToolbar);
                EditorGUI.BeginDisabledGroup(!recentWindowsInToolbar);
                maxRecentWindows = EditorGUILayout.IntField("Max Recent Windows", maxRecentWindows);
                EditorGUI.EndDisabledGroup();
                favoriteWindowsInToolbar = EditorGUILayout.ToggleLeft("Favorite Windows", favoriteWindowsInToolbar);

                EditorGUI.indentLevel--;
                EditorGUI.EndDisabledGroup();
            }
        }
    }
}