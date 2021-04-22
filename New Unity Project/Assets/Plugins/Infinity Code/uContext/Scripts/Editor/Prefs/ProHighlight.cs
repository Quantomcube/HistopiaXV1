/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using UnityEditor;

namespace InfinityCode.uContext
{
    public static partial class Prefs
    {
        public static bool proHighlight = true;

        private class ProHighlight : PrefManager
        {
            public override IEnumerable<string> keywords
            {
                get
                {
                    return new[]
                    {
                        "Pro Features",
                        "Highlight Pro features"
                    };
                }
            }

            public override float order
            {
                get { return -11; }
            }

            public override void Draw()
            {
#if !UCONTEXT_PRO
                EditorGUILayout.LabelField("Pro Features", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                proHighlight = EditorGUILayout.ToggleLeft("Highlight Pro features", proHighlight);
                EditorGUI.indentLevel--;
#endif
            }
        }
    }
}