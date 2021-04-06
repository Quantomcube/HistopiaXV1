/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using UnityEditor;

namespace InfinityCode.uContext
{
    public partial class Prefs
    {
        public static bool objectToolbar = true;
        public static SceneViewVisibleRules objectToolbarVisibleRules = SceneViewVisibleRules.onMaximized;
        public static bool objectToolbarOpenBestComponent = true;

        private class ObjectToolbarManager : PrefManager, IHasShortcutPref
        {
            public override IEnumerable<string> keywords
            {
                get
                {
                    return new[]
                    {
                        "Object Toolbar"
                    };
                }
            }

            public override float order
            {
                get { return -40; }
            }

            public override void Draw()
            {
                objectToolbar = EditorGUILayout.ToggleLeft("Object Toolbar", objectToolbar, EditorStyles.boldLabel);
                EditorGUI.indentLevel++;

                EditorGUI.BeginDisabledGroup(!objectToolbar);
                objectToolbarVisibleRules = (SceneViewVisibleRules)EditorGUILayout.EnumPopup("Visible Rules", objectToolbarVisibleRules);
#if !UCONTEXT_PRO
                string label = "Open Best Component (PRO)";
#else
                string label = "Open Best Component";
#endif
                objectToolbarOpenBestComponent = EditorGUILayout.ToggleLeft(label, objectToolbarOpenBestComponent);
                EditorGUI.EndDisabledGroup();

                EditorGUI.indentLevel--;
            }

            public IEnumerable<Shortcut> GetShortcuts()
            {
                if (!objectToolbar) return new Shortcut[0];

                return new[]
                {
#if UNITY_EDITOR_OSX
                    new Shortcut("Drag Object Toolbar", "Scene View", "CMD"),
#else
                    new Shortcut("Drag Object Toolbar", "Scene View", "CTRL"),
#endif
                    new Shortcut("Open Component In Object Toolbar", "Scene View", "ALT + {1-9}"),
                };
            }
        }
    }
}