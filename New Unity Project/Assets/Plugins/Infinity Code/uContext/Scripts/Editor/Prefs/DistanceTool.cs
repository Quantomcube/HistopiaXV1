/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using UnityEditor;

namespace InfinityCode.uContext
{
    public static partial class Prefs
    {
        public static bool showDistanceInScene = true;

        private class DistanceToolManager : PrefManager
        {
            public override IEnumerable<string> keywords
            {
                get { return new[] { "Show Distance In Scene View" }; }
            }

            public override float order
            {
                get { return -11f; }
            }

            public override void Draw()
            {
                showDistanceInScene = EditorGUILayout.ToggleLeft("Show Distance In Scene View", showDistanceInScene, EditorStyles.boldLabel);
            }
        }
    }
}