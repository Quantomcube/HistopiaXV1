/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using UnityEditor;

namespace InfinityCode.uContext
{
    public static partial class Prefs
    {
        public static bool showExceptionsInConsole = true;

        private class LogManager : PrefManager
        {
            public override IEnumerable<string> keywords
            {
                get
                {
                    return new[]
                    {
                        "Log",
                        "Show Exceptions"
                    };
                }
            }

            public override float order
            {
                get { return -5; }
            }

            public override void Draw()
            {
                EditorGUILayout.LabelField("Log", EditorStyles.boldLabel);

                EditorGUI.indentLevel++;
                showExceptionsInConsole = EditorGUILayout.ToggleLeft("Show Exceptions", showExceptionsInConsole);
                EditorGUI.indentLevel--;
            }
        }
    }
}