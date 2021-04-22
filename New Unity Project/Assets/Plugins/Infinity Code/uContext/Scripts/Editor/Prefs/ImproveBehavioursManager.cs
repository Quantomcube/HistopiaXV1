/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using UnityEditor;

namespace InfinityCode.uContext
{
    public static partial class Prefs
    {
        public static bool improveAddComponentBehaviour = true;
        public static bool improveDragAndDropBehaviour = true;
        public static bool improveMaximizeGameViewBehaviour = true;

        private class ImproveBehavioursManager : PrefManager, IHasShortcutPref
        {
            public override IEnumerable<string> keywords
            {
                get
                {
                    return new[]
                    {
                        "Improve Behaviours",
                        "Add Component By Shortcut",
                        "Drag And Drop To Canvas",
                        "Maximize Game View By Shortcut (SHIFT + Space)"
                    };
                }
            }

            public override float order
            {
                get { return -6; }
            }

            public override void Draw()
            {
                EditorGUILayout.LabelField("Improve Behaviours", EditorStyles.boldLabel);

                EditorGUI.indentLevel++;

                improveAddComponentBehaviour = EditorGUILayout.ToggleLeft("Add Component By Shortcut", improveAddComponentBehaviour);
                improveDragAndDropBehaviour = EditorGUILayout.ToggleLeft("Drag And Drop To Canvas", improveDragAndDropBehaviour);
                improveMaximizeGameViewBehaviour = EditorGUILayout.ToggleLeft("Maximize Game View By Shortcut (SHIFT + Space)", improveMaximizeGameViewBehaviour);

                EditorGUI.indentLevel--;
            }

            public IEnumerable<Shortcut> GetShortcuts()
            {
                List<Shortcut> shortcuts = new List<Shortcut>();
                if (improveAddComponentBehaviour)
                {
                    shortcuts.Add(new Shortcut("Add Component To Selected GameObject", "Everywhere",
#if !UNITY_EDITOR_OSX
                    "CTRL + SHIFT + A"
#else
                            "CMD + SHIFT + A"
#endif
                    ));
                }

                if (improveMaximizeGameViewBehaviour)
                {
                    shortcuts.Add(new Shortcut("Maximize GameView", "Game View", "SHIFT + SPACE"));
                }

                return shortcuts;
            }
        }
    }
}